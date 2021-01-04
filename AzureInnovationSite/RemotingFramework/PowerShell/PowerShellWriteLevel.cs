using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemotingFramework.PowerShell
{
    /// <summary>
    /// Specifies the PowerShell Write Level, for the various Write-* cmdlets.
    /// </summary>
    public enum PowerShellWriteLevel
    {
        /// <summary>
        /// Write-Verbose
        /// </summary>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-verbose
        /// </remarks>
        Verbose,

        /// <summary>
        /// Write-Debug
        /// </summary>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-debug
        /// </remarks>
        Debug,

        /// <summary>
        /// Write-Host
        /// </summary>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-host
        /// </remarks>
        Standard,

        /// <summary>
        /// Write-Warning
        /// </summary>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-warning
        /// </remarks>
        Warning,

        /// <summary>
        /// Write-Error
        /// </summary>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-error
        /// </remarks>
        Error
    }
}