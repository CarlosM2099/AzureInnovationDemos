using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace RemotingFramework.PowerShell
{
    /// <summary>
    /// Executes PowerShell against a remote machine.
    /// </summary>
    public interface IRemotePowerShell : IDisposable
    {
        /// <summary>
        /// Gets a copy of the current connection information.
        /// </summary>
        RemotePowerShellConnectionInfo ConnectionInfo { get; }

        /// <summary>
        /// Gets the associated <see cref="RemotePowerShellHost" />.
        /// </summary>
        RemotePowerShellHost Host { get; }

        /// <summary>
        /// Gets the associated <see cref="RemotePowerShellHostUserInterface" />.
        /// </summary>
        RemotePowerShellHostUserInterface HostUserInterface { get; }

        /// <summary>
        /// Gets the remote file system.
        /// </summary>
        RemoteFileSystem FileSystem { get; }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        void Open(RemotePowerShellConnectionInfo connectionInfo);

        /// <summary>
        /// Opens the connection, asynchronously.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        Task OpenAsync(RemotePowerShellConnectionInfo connectionInfo);

        /// <summary>
        /// Tests that a given connection is valid and can be opened.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        bool TestConnection(RemotePowerShellConnectionInfo connectionInfo);

        /// <summary>
        /// Tests that a given connection is valid and can be opened, asynchronously.
        /// </summary>
        /// <param name="connectionInfo">
        /// The remote powershell connection information.
        /// </param>
        Task<bool> TestConnectionAsync(RemotePowerShellConnectionInfo connectionInfo);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes the connection, asynchronously.
        /// </summary>
        Task CloseAsync();

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
        ICollection<T> InvokeCommand<T>(string cmdlet, object parameters = null, params string[] switches);

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        void InvokeCommand(string cmdlet, object parameters = null, params string[] switches);

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
        Task<ICollection<T>> InvokeCommandAsync<T>(string cmdlet, object parameters = null, params string[] switches);

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        Task InvokeCommandAsync(string cmdlet, object parameters = null, params string[] switches);

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="command">The PowerShell command to invoke.</param>
        ICollection<T> InvokeCommand<T>(Command command);

        /// <summary>
        /// Invokes a PowerShell command.
        /// </summary>
        /// <param name="command">The PowerShell command to invoke.</param>
        void InvokeCommand(Command command);

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="command">The PowerShell command to invoke.</param>
        Task<ICollection<T>> InvokeCommandAsync<T>(Command command);

        /// <summary>
        /// Invokes a PowerShell command asynchronously.
        /// </summary>
        /// <param name="command">The PowerShell command to invoke.</param>
        Task InvokeCommandAsync(Command command);

        /// <summary>
        /// Invokes PowerShell command(s).
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        ICollection<T> InvokePipedCommands<T>(params Command[] commands);

        /// <summary>
        /// Invokes PowerShell command(s).
        /// </summary>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        void InvokePipedCommands(params Command[] commands);

        /// <summary>
        /// Invokes PowerShell command(s) asynchronously.
        /// </summary>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        Task InvokeCommandsAsync(params Command[] commands);

        /// <summary>
        /// Invokes PowerShell command(s) asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="commands">The PowerShell commands to invoke.</param>
        Task<ICollection<T>> InvokeCommandsAsync<T>(params Command[] commands);

        /// <summary>
        /// Invokes an arbitrary block of PowerShell script.
        /// </summary>
        /// <param name="script">The PowerShell script block.</param>
        void InvokeScript(string script);

        /// <summary>
        /// Invokes an arbitrary block of PowerShell script.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="script">The PowerShell script block.</param>
        ICollection<T> InvokeScript<T>(string script);

        /// <summary>
        /// Invokes an arbitrary PowerShell script block, asynchronously.
        /// </summary>
        /// <param name="script">The PowerShell script block.</param>
        Task InvokeScriptAsync(string script);

        /// <summary>
        /// Invokes an arbitrary PowerShell script block, asynchronously.
        /// </summary>
        /// <typeparam name="T">
        /// The type of results to return from the output stream.
        /// </typeparam>
        /// <param name="script">The PowerShell script block.</param>
        Task<ICollection<T>> InvokeScriptAsync<T>(string script);
    }
}