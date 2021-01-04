using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using RemotingFramework.Utilities;

namespace RemotingFramework.PowerShell
{
    /// <summary>
    /// Utilities for accessing the File System on a Remote Machine via PowerShell.
    /// </summary>
    public class RemoteFileSystem
    {
        /// <summary>
        /// Gets the Remote PowerShell instance.
        /// </summary>
        public IRemotePowerShell RemotePowerShell { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteFileSystem" /> class.
        /// </summary>
        /// <param name="remotePowerShell">
        /// The Remote PowerShell instance to perform the commands against.
        /// </param>
        internal RemoteFileSystem(IRemotePowerShell remotePowerShell)
        {
            this.RemotePowerShell = remotePowerShell
                ?? throw new ArgumentNullException(nameof(remotePowerShell));
        }

        /// <summary>
        /// Determines if the specified path exists on the remote machine.
        /// </summary>
        /// <param name="path">The path to check for.</param>
        /// <param name="checkFiles">
        /// <c>true</c> if you want to check for files that exist.
        /// </param>
        /// <param name="checkFolders">
        /// <c>true</c> if you want to check for folders that exist.
        /// </param>
        public bool RemotePathExists(string path, bool checkFiles = true, bool checkFolders = true)
        {
            if (!checkFiles && !checkFolders) { return false; } // how could that exist?

            string testPathType = "Any";
            if (!checkFiles) { testPathType = "Container"; }
            if (!checkFolders) { testPathType = "Leaf"; }

            return RemotePowerShell.InvokeCommand<bool>
            (
                "Test-Path",
                new
                {
                    Path = path,
                    PathType = testPathType
                }
            )
            .Single();
        }

        /// <summary>
        /// Determines if the specified path exists on the remote machine asynchronously.
        /// </summary>
        /// <param name="path">The path to check for.</param>
        /// <param name="checkFiles">
        /// <c>true</c> if you want to check for files that exist.
        /// </param>
        /// <param name="checkFolders">
        /// <c>true</c> if you want to check for folders that exist.
        /// </param>
        public async Task<bool> RemotePathExistsAsync(string path, bool checkFiles = true, bool checkFolders = true)
        {
            return await Task.Run(() => RemotePathExists(path, checkFiles, checkFolders));
        }

        /// <summary>
        /// Ensures that the given directory exists on the remote machine.
        /// </summary>
        /// <param name="directory">The directory to check for.</param>
        public void EnsureDirectory(string directory)
        {
            if (!RemotePathExists(directory, false, true))
            {
                // Specify return type to prevent console output.
                RemotePowerShell.InvokeCommand<PSObject>("md", new { Path = directory });
            }
        }

        /// <summary>
        /// Ensures that the given directory exists on the remote machine asynchronously.
        /// </summary>
        /// <param name="directory">The directory to check for.</param>
        public async Task EnsureDirectoryAsync(string directory)
        {
            await Task.Run(() => EnsureDirectory(directory));
        }

        /// <summary>
        /// Gets raw bytes of a file on the remote file system.
        /// </summary>
        /// <param name="remoteFilePath">The path to the file.</param>
        public byte[] GetFile(string remoteFilePath)
        {
            byte[] results = null;

            if (RemotePathExists(remoteFilePath, true, false))
            {
                remoteFilePath = PowerShellUtils.EscapeString(remoteFilePath);
                var script = $@"[System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes({remoteFilePath}))";

                var data = RemotePowerShell.InvokeScript<string>(script).Single();
                results = Convert.FromBase64String(data);
            }

            return results;
        }

        /// <summary>
        /// Gets raw bytes of a file on the remote file system asynchronously.
        /// </summary>
        /// <param name="remoteFilePath">The path to the file.</param>
        public async Task<byte[]> GetFileAsync(string remoteFilePath)
        {
            return await Task.Run(() => GetFile(remoteFilePath));
        }

        /// <summary>
        /// Gets raw bytes of a file on the remote file system.
        /// </summary>
        /// <param name="remoteFilePath">The path to the file.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times",
            Justification = "Okay to dispose nested streams this way.")]
        public string GetFileText(string remoteFilePath)
        {
            string result = null;
            var bytes = GetFile(remoteFilePath);
            if (bytes != null)
            {
                using (var ms = new MemoryStream(bytes))
                using (var sr = new StreamReader(ms, true))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets raw bytes of a file on the remote file system asynchronously.
        /// </summary>
        /// <param name="remoteFilePath">The path to the file.</param>
        public async Task<string> GetFileTextAsync(string remoteFilePath)
        {
            return await Task.Run(() => GetFileText(remoteFilePath));
        }

        private bool VerifyFileContents(string path, byte[] expectedContents)
        {
            // Does the file already exist?
            if (RemotePathExists(path, true, false))
            {
                // Is it the same size?
                long measuredSize = GetRemoteFileSize(path);
                if (measuredSize == expectedContents.LongLength)
                {
                    // Is it the same MD5 hash?
                    using (var md5 = MD5.Create())
                    {
                        var localHash = BitConverter.ToString(md5.ComputeHash(expectedContents))
                            .Replace("-", string.Empty).ToUpperInvariant();

                        var remoteHash = GetRemoteFileHash(path);

                        if (localHash == remoteHash)
                        {
                            // File already exists, and contains correct contents!
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Puts a file on the remote machine.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="contents">The raw binary contents of the file.</param>
        /// <param name="unblock">
        /// <c>true</c> to unblock the file after it's created; otherwise <c>false</c>.
        /// </param>
        public void PutFile(string path, byte[] contents, bool unblock = true)
        {
            var escapedPath = PowerShellUtils.EscapeString(path);

            if (!VerifyFileContents(path, contents))
            {
                if (!RemotePathExists(path, true, false))
                {
                    EnsureDirectory(Path.GetDirectoryName(path));
                }

                const long maxBufferSize = 1024 * 1024;
                if (contents.LongLength < maxBufferSize)
                {
                    var rawData = "\"" + Convert.ToBase64String(contents) + "\"";

                    RemotePowerShell.InvokeScript
                    (
                        @"[System.IO.File]::WriteAllBytes" +
                        "(" +
                            escapedPath + ", " +
                            "[System.Convert]::FromBase64String(" + rawData + ")" +
                        ")"
                    );
                }
                else
                {
                    var files = new List<string>();
                    var buffer = new byte[maxBufferSize];
                    int fileNumber = 0;

                    using (var ms = new MemoryStream(contents))
                    {
                        while (ms.Position < ms.Length)
                        {
                            var oldPos = ms.Position;
                            int size = ms.Read(buffer, 0, buffer.Length);
                            if (size <= 0) { break; }
                            if (size < buffer.Length)
                            {
                                // do over with resized buffer.
                                buffer = new byte[size];
                                ms.Position = oldPos;
                                continue;
                            }

                            var rawData = "\"" + Convert.ToBase64String(buffer) + "\"";
                            var outputFile = PowerShellUtils.EscapeString(path + "_" + fileNumber);
                            RemotePowerShell.InvokeScript
                            (
                                @"[System.IO.File]::WriteAllBytes" +
                                "(" +
                                     outputFile + ", " +
                                    "[System.Convert]::FromBase64String(" + rawData + ")" +
                                ")"
                            );
                            files.Add(outputFile);
                            fileNumber++;

                            if (buffer.Length < maxBufferSize)
                            {
                                buffer = new byte[maxBufferSize];
                            }
                        }
                    }

                    var sb = new StringBuilder();
                    sb.Append("gc ");
                    sb.Append(string.Join(",", files));
                    sb.Append("-Enc Byte -Read 512 | sc ");
                    sb.Append(escapedPath);
                    sb.Append(" -Enc Byte");
                    RemotePowerShell.InvokeScript(sb.ToString());

                    foreach (var file in files)
                    {
                        RemotePowerShell.InvokeScript($"del {file} -Force;");
                    }

                    if (!VerifyFileContents(path, contents))
                    {
                        throw new InvalidOperationException("PUT FILE FAILED.");
                    }
                }
            }

            if (unblock)
            {
                RemotePowerShell.InvokeCommand("Unblock-File", new { Path = path });
            }
        }

        /// <summary>
        /// Puts a file on the remote machine asynchronously.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="contents">The raw binary contents of the file.</param>
        /// <param name="unblock">
        /// <c>true</c> to unblock the file after it's created; otherwise <c>false</c>.
        /// </param>
        public async Task PutFileAsync(string path, byte[] contents, bool unblock = true)
        {
            await Task.Run(() => PutFile(path, contents, unblock));
        }

        /// <summary>
        /// Unzips the supplied zip file to the specified remote file system path.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="outputPath">
        /// The path on the remote file system where the zip file should be unzipped.
        /// </param>
        /// <param name="token">A cancellation token to abort the unzipping operation.</param>
        public void UnzipTo(ZipFile zipFile, string outputPath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // Normalize the output Path to use correct slashes, and always contain a
            // trailing slash.
            outputPath = outputPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            string name = string.Empty;
            if (!string.IsNullOrWhiteSpace(zipFile.Name))
            {
                // include leading space in name.
                name = " '" + zipFile.Name?.Trim() + "'";
            }
            int id = Math.Abs(Guid.NewGuid().GetHashCode());

            var startedAt = DateTime.UtcNow;
            int entryIdx = 0;
            foreach (ZipEntry entry in zipFile)
            {
                token.ThrowIfCancellationRequested();

                if (entry.IsFile || entry.IsDirectory)
                {
                    double percentComplete = ((entryIdx * 100D) / (double)zipFile.Count);
                    double elapsedSeconds = (DateTime.UtcNow - startedAt).TotalSeconds;
                    double secondsRemaining = (elapsedSeconds / (double)percentComplete)
                        * (100d - (double)percentComplete);

                    RemotePowerShell.InvokeCommand
                    (
                        "Write-Progress",
                        new
                        {
                            Id = id,
                            Activity = $"Unzipping{name} to '{outputPath}' on '{RemotePowerShell.ConnectionInfo}'.",
                            Status = entry.Name,
                            PercentComplete = (int)Math.Round(percentComplete),
                            SecondsRemaining = (int)Math.Round(secondsRemaining),
                        }
                    );

                    var outputFileName = entry.Name.Replace
                    (
                        Path.AltDirectorySeparatorChar,
                        Path.DirectorySeparatorChar
                    )
                    .TrimStart(Path.DirectorySeparatorChar);

                    outputFileName = Path.Combine(outputPath, outputFileName);

                    if (entry.IsFile)
                    {
                        using (var ms = new MemoryStream())
                        using (var entryStream = zipFile.GetInputStream(entry))
                        {
                            entryStream.CopyTo(ms);
                            ms.Flush();

                            PutFile(outputFileName, ms.ToArray(), true);
                        }
                    }
                    else
                    {
                        EnsureDirectory(outputFileName);
                    }
                }

                entryIdx++;
            }

            RemotePowerShell.InvokeCommand
            (
                "Write-Progress",
                new
                {
                    Id = id,
                    Activity = $"Unzipping{name} to '{outputPath}' on '{RemotePowerShell.ConnectionInfo}'.",
                    PercentComplete = 100,
                    SecondsRemaining = 0,
                },
                "Completed"
            );
        }

        /// <summary>
        /// Unzips the supplied zip file to the specified remote file system path
        /// asynchronously.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="outputPath">
        /// The path on the remote file system where the zip file should be unzipped.
        /// </param>
        /// <param name="token">A cancellation token to abort the unzipping operation.</param>
        public async Task UnzipToAsync(ZipFile zipFile, string outputPath, CancellationToken token)
        {
            await Task.Run(() => UnzipTo(zipFile, outputPath, token));
        }

        /// <summary>
        /// Unzips the supplied zip file to the specified remote file system path.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="outputPath">
        /// The path on the remote file system where the zip file should be unzipped.
        /// </param>
        [Obsolete]
        public void UnzipTo(ZipFile zipFile, string outputPath)
        {
            UnzipTo(zipFile, outputPath, CancellationToken.None);
        }

        /// <summary>
        /// Unzips the supplied zip file to the specified remote file system path
        /// asynchronously.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="outputPath">
        /// The path on the remote file system where the zip file should be unzipped.
        /// </param>
        [Obsolete]
        public async Task UnzipToAsync(ZipFile zipFile, string outputPath)
        {
            await UnzipToAsync(zipFile, outputPath, CancellationToken.None);
        }

        /// <summary>
        /// Gets the MD5 hash of a remote file.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        public string GetRemoteFileHash(string remoteFilePath)
        {
            return GetRemoteFileHash(remoteFilePath, null);
        }

        /// <summary>
        /// Gets the hash of a remote file.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        public string GetRemoteFileHash(string remoteFilePath, string algorithm)
        {
            algorithm = (algorithm ?? string.Empty).Trim();
            bool appendFileLength = false;
            if (algorithm.EndsWith("+LENGTH", StringComparison.InvariantCultureIgnoreCase))
            {
                appendFileLength = true;
                algorithm = algorithm.Substring(0, algorithm.Length - 7).Trim();
            }

            if (string.IsNullOrWhiteSpace(algorithm))
            {
                algorithm = "MD5";
            }

            var hashResult = RemotePowerShell.InvokeCommand<PSObject>
            (
                "Get-FileHash",
                new
                {
                    Path = remoteFilePath,
                    Algorithm = algorithm
                }
            );

            var row = hashResult.FirstOrDefault();
            string result = null;
            if (row != null)
            {
                result = row.Properties.Where
                (
                    x => x.Name.StartsWith("Hash", StringComparison.InvariantCultureIgnoreCase)
                )
                .Select
                (
                    x => x?.Value as string
                )
                .Where
                (
                    x => !string.IsNullOrWhiteSpace(x)
                )
                .FirstOrDefault();
            }

            if (appendFileLength)
            {
                var length = GetRemoteFileSize(remoteFilePath);
                result = (result ?? string.Empty) + "::" + length;
            }

            return result;
        }

        /// <summary>
        /// Gets the hash of a remote file asynchronously.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        public async Task<string> GetRemoteFileHashAsync(string remoteFilePath, string algorithm)
        {
            return await Task.Run(() => GetRemoteFileHash(remoteFilePath, algorithm));
        }

        /// <summary>
        /// Gets the MD5 hash of a remote file asynchronously.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        public async Task<string> GetRemoteFileHashAsync(string remoteFilePath)
        {
            return await Task.Run(() => GetRemoteFileHash(remoteFilePath));
        }

        /// <summary>
        /// Gets the size of a remote file.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        public long GetRemoteFileSize(string remoteFilePath)
        {
            long result = 0;

            if (RemotePathExists(remoteFilePath, true, false))
            {
                var fileResult = RemotePowerShell.InvokeCommand<PSObject>
                (
                    "Get-ChildItem",
                    new
                    {
                        Path = remoteFilePath
                    }
                );

                return (long)fileResult.First().Properties["Length"].Value;
            }

            return result;
        }

        /// <summary>
        /// Gets the size of a remote file asynchronously.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        public async Task<long> GetRemoteFileSizeAsync(string remoteFilePath)
        {
            return await Task.Run(() => GetRemoteFileSize(remoteFilePath));
        }
    }
}