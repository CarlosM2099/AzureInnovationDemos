using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell;
using RemotingFramework.Utilities;

namespace RemotingFramework.PowerShell
{
    /// <summary>
    /// Executes PowerShell against a remote machine.
    /// </summary>
    public class RemotePowerShell : IRemotePowerShell
    {
        private static readonly TimeSpan CommandWaitDelay = TimeSpan.FromMilliseconds(100);

        private struct NoResult { /* it's annoying that you can't use System.Void from C# */ }

        #region Escaping & Converting

        private static Exception ToException(object o)
        {
            if (o is Exception ex) { return ex; }
            if (o is IEnumerable<Exception> exs) { return new AggregateException(exs); }

            return new Exception(o == null ? "Unspecified Error" : GetString(o));
        }

        private static string GetString(object o)
        {
            if (o is string s) { return s; }

            if (o is IEnumerable en)
            {
                var sb = new StringBuilder();
                foreach (object c in en)
                {
                    sb.AppendLine(GetString(c));
                }
                return sb.ToString()?.Trim();
            }

            return o?.ToString() ?? "[null]";
        }

        private static bool ConvertResult<T>(object input, out T output)
        {
            if (input == null || typeof(NoResult).IsAssignableFrom(typeof(T)))
            {
                output = default(T);
                return typeof(T).IsClass;
            }
            else
            {
                var inputType = input.GetType();

                try
                {
                    if (typeof(T).IsAssignableFrom(inputType))
                    {
                        output = (T)input;
                        return true;
                    }
                }
                catch { /* do nothing */ }

                if (input is PSObject pso)
                {
                    if (ConvertResult(pso.BaseObject, out output))
                    {
                        return true;
                    }
                }

                try
                {
                    output = (T)Convert.ChangeType(input, typeof(T));
                    return true;
                }
                catch
                {
                    try
                    {
                        output = (T)Convert.ChangeType(input?.ToString(), typeof(T));
                        return true;
                    }
                    catch
                    { /* do nothing */ }
                }
            }

            output = default(T);
            return false;
        }

        #endregion

        private readonly object SyncRoot = new object();

        private RemotePowerShellConnectionInfo RawConnectionInfo = null;

        /// <summary>
        /// Gets a copy of the current connection information.
        /// </summary>
        public RemotePowerShellConnectionInfo ConnectionInfo
        {
            get { return RawConnectionInfo?.Clone(); }
            private set { RawConnectionInfo = value; }
        }

        /// <summary>
        /// Gets the associated <see cref="RemotePowerShellHost" />.
        /// </summary>
        /// <remarks>Overwritten every time the object is connected.</remarks>
        public RemotePowerShellHost Host { get; private set; }

        /// <summary>
        /// Gets the associated <see cref="RemotePowerShellHostUserInterface" />.
        /// </summary>
        /// <remarks>
        /// <para>Overwritten every time the object is connected.</para>
        /// <para>
        /// It's possible for this to be <c>null</c> (even when connected); so check the
        /// returned object before consuming it.
        /// </para>
        /// </remarks>
        public RemotePowerShellHostUserInterface HostUserInterface
        {
            get
            {
                return Host?.UI as RemotePowerShellHostUserInterface;
            }
        }

        /// <summary>
        /// Tests that a given connection is valid and can be opened.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        public bool TestConnection(RemotePowerShellConnectionInfo connectionInfo)
        {
            if (connectionInfo == null) { throw new ArgumentNullException(nameof(connectionInfo)); }

            try
            {
                using (var pshell = new RemotePowerShell())
                {
                    pshell.Open(connectionInfo);
                    pshell.Close();
                }

                return true;
            }
            catch (PSRemotingTransportException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Tests that a given connection is valid and can be opened, asynchronously.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        public async Task<bool> TestConnectionAsync(RemotePowerShellConnectionInfo connectionInfo)
        {
            return await Task.Run(() => TestConnection(connectionInfo));
        }

        /// <summary>
        /// Gets the remote runspace pool.
        /// </summary>
        /// <remarks>Will be <c>null</c> if the connection is not currently open.</remarks>
        public Runspace RemoteRunspace { get; private set; }

        /// <summary>
        /// Gets the remote file system.
        /// </summary>
        public RemoteFileSystem FileSystem { get; private set; }

        #region Open / Close

        /// <summary>
        /// Closes the connection, asynchronously.
        /// </summary>
        public async Task CloseAsync()
        {
            await Task.Run(() => Close());
        }

        /// <summary>
        /// Opens the connection, asynchronously.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        public async Task OpenAsync(RemotePowerShellConnectionInfo connectionInfo)
        {
            await Task.Run(() => Open(connectionInfo));
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            try
            {
                RemoteRunspace?.Close();
                RemoteRunspace?.Dispose();
            }
            catch { /* do nothing */ }

            RemoteRunspace = null;
            ConnectionInfo = null;
            Host = null;
            FileSystem = null;
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        public void Open(RemotePowerShellConnectionInfo connectionInfo)
        {
            if (connectionInfo == null) { throw new ArgumentNullException(nameof(connectionInfo)); }
            this.Close();

            try
            {
                Runspace rrs = null;
                this.Host = new RemotePowerShellHost();

                if (connectionInfo.IsLocalConnection())
                {
                    // Local connection
                    rrs = RunspaceFactory.CreateRunspace
                    (
                        Host
                    );
                }
                else
                {
                    // Remote connection
                    rrs = RunspaceFactory.CreateRunspace
                    (
                        Host,
                        connectionInfo.ToRunspaceConnectionInfo()
                    );
                }

                rrs.Open();
                this.RemoteRunspace = rrs;
                this.ConnectionInfo = connectionInfo;

                FileSystem = new RemoteFileSystem(this);
            }
            catch
            {
                this.Close();
                throw;
            }
        }

        #endregion

        #region Working with PowerShell Commands

        #region InvokeCommand Simple

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        public ICollection<T> InvokeCommand<T>(string cmdlet, object parameters = null, params string[] switches)
        {
            return InvokeCommand<T>(PowerShellUtils.CreateCommand(cmdlet, parameters, switches));
        }

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        public void InvokeCommand(string cmdlet, object parameters = null, params string[] switches)
        {
            InvokeCommand(PowerShellUtils.CreateCommand(cmdlet, parameters, switches));
        }

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        public async Task<ICollection<T>> InvokeCommandAsync<T>(string cmdlet, object parameters = null, params string[] switches)
        {
            return await Task.Run(() => InvokeCommand<T>(cmdlet, parameters, switches));
        }

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        public async Task InvokeCommandAsync(string cmdlet, object parameters = null, params string[] switches)
        {
            await Task.Run(() => InvokeCommand(cmdlet, parameters, switches));
        }

        #endregion

        #region InvokeCommand Regular

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="command">The PowerShell command to invoke.</param>
        public ICollection<T> InvokeCommand<T>(Command command)
        {
            return InvokePipedCommands<T>(command);
        }

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <param name="command">The PowerShell command to invoke.</param>
        public void InvokeCommand(Command command)
        {
            InvokePipedCommands<NoResult>(command);
        }

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="command">The PowerShell command to invoke.</param>
        public async Task<ICollection<T>> InvokeCommandAsync<T>(Command command)
        {
            return await Task.Run(() => InvokeCommand<T>(command));
        }

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <param name="command">The PowerShell command to invoke.</param>
        public async Task InvokeCommandAsync(Command command)
        {
            await Task.Run(() => InvokeCommand(command));
        }

        #endregion

        #region InvokeCommands

        /// <summary>
        /// Invokes a collection of PowerShell command(s), as if they were piped together.
        /// </summary>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        public void InvokePipedCommands(params Command[] commands)
        {
            InvokePipedCommands<NoResult>(commands);
        }

        /// <summary>
        /// Invokes a collection of PowerShell command(s), as if they were piped together.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        public ICollection<T> InvokePipedCommands<T>(params Command[] commands)
        {
            if (this.RemoteRunspace == null)
            { throw new InvalidOperationException("Remote PowerShell Connection is not currently open!"); }

            if (commands?.Where(c => c != null)?.Any() != true) { throw new ArgumentNullException(nameof(commands)); }

            // One command at a time, please.
            lock (SyncRoot)
            {
                using (var pipeline = RemoteRunspace.CreatePipeline())
                {
                    foreach (var cmd in commands)
                    {
                        if (cmd != null)
                        {
                            pipeline.Commands.Add(cmd);
                        }
                    }

                    // "... If you want this pipeline to execute as a standalone
                    // command(that is, using command-line parameters only), be sure to
                    // call Pipeline.Input.Close() before calling InvokeAsync(). Otherwise,
                    // the command will be executed as though it had external input. If you
                    // observe that the command isn't doing anything, this may be the
                    // reason."
                    // via: https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runspaces.pipeline.invokeasync?view=powershellsdk-1.1.0

                    pipeline.Input.Close();
                    pipeline.InvokeAsync();

                    var results = new List<T>();
                    var errors = new List<object>();

                    while (pipeline.Output.IsOpen || !pipeline.Output.EndOfPipeline || !pipeline.Error.EndOfPipeline)
                    {
                        bool didWork = false;

                        var err = pipeline.Error.NonBlockingRead();
                        if (err?.Any() == true)
                        {
                            errors.Add(err);
                            foreach (var e in err)
                            {
                                Host?.UI?.WriteErrorLine(e?.ToString());
                            }
                            didWork = true;
                        }

                        var values = pipeline.Output.NonBlockingRead();
                        if (values?.Any() == true)
                        {
                            foreach (var value in values)
                            {
                                if (ConvertResult(value, out T result))
                                {
                                    results.Add(result);
                                }
                                else
                                {
                                    Host?.UI?.WriteLine(value?.ToString());
                                }
                            }
                            didWork = true;
                        }

                        if (errors.Any() || pipeline.HadErrors || pipeline.PipelineStateInfo?.Reason != null)
                        {
                            break;
                        }
                        else if (!didWork)
                        {
                            // Keeps the CPU from burning a hole in your motherboard...
                            Thread.Sleep(CommandWaitDelay);
                        }
                    }
                    pipeline.Output.Close();
                    pipeline.Error.Close();

                    if (errors.Any() || pipeline.HadErrors || pipeline.PipelineStateInfo?.Reason != null)
                    {
                        var exs = errors.Select(ToException).ToList();

                        if (pipeline?.PipelineStateInfo?.Reason != null)
                        {
                            exs.Insert(0, pipeline?.PipelineStateInfo?.Reason);
                        }

                        if (exs.Count == 0)
                        {
                            throw new InvalidOperationException("PowerShell Execution Failed for an unspecified reason.");
                        }
                        else if (exs.Count == 1)
                        {
                            throw exs.First();
                        }
                        else
                        {
                            throw new AggregateException("PowerShell Execution Failed.", exs);
                        }
                    }

                    return results;
                }
            }
        }

        /// <summary>
        /// Invokes PowerShell command(s) asynchronously.
        /// </summary>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        public async Task InvokeCommandsAsync(params Command[] commands)
        {
            await Task.Run(() => InvokePipedCommands(commands));
        }

        /// <summary>
        /// Invokes PowerShell command(s) asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        public async Task<ICollection<T>> InvokeCommandsAsync<T>(params Command[] commands)
        {
            return await Task.Run(() => InvokePipedCommands<T>(commands));
        }

        #endregion

        #region InvokeScript

        /// <summary>
        /// Invokes an arbitrary block of PowerShell script.
        /// </summary>
        /// <param name="script">The PowerShell script block.</param>
        public void InvokeScript(string script)
        {
            InvokeCommand(new Command(script, true));
        }

        /// <summary>
        /// Invokes an arbitrary block of PowerShell script.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="script">The PowerShell script block.</param>
        public ICollection<T> InvokeScript<T>(string script)
        {
            return InvokeCommand<T>(new Command(script, true));
        }

        /// <summary>
        /// Invokes an arbitrary PowerShell script block, asynchronously.
        /// </summary>
        /// <param name="script">The PowerShell script block.</param>
        public async Task InvokeScriptAsync(string script)
        {
            await Task.Run(() => InvokeScript(script));
        }

        /// <summary>
        /// Invokes an arbitrary PowerShell script block, asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="script">The PowerShell script block.</param>
        public async Task<ICollection<T>> InvokeScriptAsync<T>(string script)
        {
            return await Task.Run(() => InvokeScript<T>(script));
        }

        #endregion

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> if called from <see cref="Dispose()" />, otherwise <c>false</c>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            Close();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RemotePowerShell" /> class.
        /// </summary>
        ~RemotePowerShell()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}