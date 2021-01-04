using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RemotingFramework.Utilities
{
    /// <summary>
    /// Static Utility Methods for working with BLOBs.
    /// </summary>
    public static class BlobUtils
    {
        #region Compression

        /// <summary>
        /// Deflates a BLOB.
        /// </summary>
        /// <param name="input">The input BLOB.</param>
        /// <remarks>
        /// For small BLOBs, this may return a bigger BLOB than the input; but for large
        /// BLOBs, it will compress them.
        /// </remarks>
        public static byte[] DeflateBlob(byte[] input)
        {
            if (input == null) { return null; }

            using (var ms = new MemoryStream())
            using (var cs = new DeflateStream(ms, CompressionLevel.Optimal))
            {
                cs.Write(input, 0, input.Length);

                // *REQUIRED* or last chunk will be omitted. Do NOT call any other close or
                // flush method.
                cs.Close();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// GZips a BLOB.
        /// </summary>
        /// <param name="input">The input BLOB.</param>
        /// <remarks>
        /// For small BLOBs, this may return a bigger BLOB than the input; but for large
        /// BLOBs, it will compress them.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static byte[] ZipBlob(byte[] input)
        {
            if (input == null) { return null; }

            using (var ms = new MemoryStream())
            using (var cs = new GZipStream(ms, CompressionLevel.Optimal))
            {
                cs.Write(input, 0, input.Length);

                // *REQUIRED* or last chunk will be omitted. Do NOT call any other close or
                // flush method.
                cs.Close();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deflates a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <remarks>
        /// For short strings, this may return a longer string than the input; but for very
        /// long strings, it will compress them.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string DeflateString(string input)
        {
            if (input == null) { return null; }

            return Convert.ToBase64String
            (
                DeflateBlob(Encoding.UTF8.GetBytes(input))
            );
        }

        /// <summary>
        /// GZips a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <remarks>
        /// For short strings, this may return a longer string than the input; but for very
        /// long strings, it will compress them.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string ZipString(string input)
        {
            if (input == null) { return null; }

            return Convert.ToBase64String
            (
                ZipBlob(Encoding.UTF8.GetBytes(input))
            );
        }

        #endregion

        #region Decompression

        /// <summary>
        /// Unzips a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string UnzipString(string input)
        {
            if (input == null) { return null; }

            return Encoding.UTF8.GetString
            (
                UnzipBlob(Convert.FromBase64String(input))
            );
        }

        /// <summary>
        /// Inflates a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string InflateString(string input)
        {
            if (input == null) { return null; }

            return Encoding.UTF8.GetString
            (
                InflateBlob(Convert.FromBase64String(input))
            );
        }

        /// <summary>
        /// Unzips a BLOB.
        /// </summary>
        /// <param name="input">The input BLOB.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static byte[] UnzipBlob(byte[] input)
        {
            if (input == null) { return null; }

            using (var ms = new MemoryStream(input))
            using (var ds = new GZipStream(ms, CompressionMode.Decompress))
            using (var os = new MemoryStream())
            {
                ds.CopyTo(os);
                return os.ToArray();
            }
        }

        /// <summary>
        /// Inflates a BLOB.
        /// </summary>
        /// <param name="input">The input BLOB.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static byte[] InflateBlob(byte[] input)
        {
            if (input == null) { return null; }

            using (var ms = new MemoryStream(input))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var os = new MemoryStream())
            {
                ds.CopyTo(os);
                return os.ToArray();
            }
        }

        #endregion
    }
}