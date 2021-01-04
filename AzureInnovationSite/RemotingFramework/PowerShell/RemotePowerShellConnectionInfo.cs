using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemotingFramework.PowerShell
{
    /// <summary>
    /// Information used to establish Remote PowerShell Connections.
    /// </summary>
    public class RemotePowerShellConnectionInfo : ICloneable
    {
        /// <summary>
        /// The default Remote PowerShell port.
        /// </summary>
        public const int DefaultPowerShellPort = 5986;

        /// <summary>
        /// Gets a <see cref="RemotePowerShellConnectionInfo" /> object that represents a
        /// connection to the local machine (i.e. allows you to use to library for
        /// non-remote connections).
        /// </summary>
        public static RemotePowerShellConnectionInfo LocalMachineConnectionInfo
        {
            get
            {
                return new RemotePowerShellConnectionInfo
                {
                    ComputerAddress = "127.0.0.1",
                    Credentials = null,
                    Port = -1,
                    RequireValidCertificate = false,
                    UseSecurePowerShell = false
                };
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a secure connection is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if a secure connection is required; otherwise <c>false</c>.
        /// </value>
        public bool UseSecurePowerShell { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the SSL certificate must be validated
        /// or not.
        /// </summary>
        public bool RequireValidCertificate { get; set; } = false;

        /// <summary>
        /// Gets or sets the Remote Computer Address.
        /// </summary>
        /// <value>
        /// IP Address, Machine Name (local network), FQDN, etc. of the remote computer.
        /// </value>
        public string ComputerAddress { get; set; }

        /// <summary>
        /// Gets or sets the port used to connect to the Remote Computer.
        /// </summary>
        public int Port { get; set; } = DefaultPowerShellPort;

        /// <summary>
        /// Gets or sets the Credentials to be used by the remote connection.
        /// </summary>
        public PSCredential Credentials { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);

        internal bool IsLocalConnection()
        {
            bool result = false;
            if (Port <= 0)
            {
                if (IPAddress.TryParse(ComputerAddress, out IPAddress ip))
                {
                    result = IPAddress.IsLoopback(ip);
                }

                if (!result)
                {
                    result =
                    (
                        string.Equals(ComputerAddress, "localhost", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(ComputerAddress, "(local)", StringComparison.InvariantCultureIgnoreCase)
                    );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates Remote PowerShell Connection Information for a given computer, using a
        /// standard UserName and Password combination.
        /// </summary>
        /// <param name="computerAddress">
        /// The computer address (IP Address, Machine Name (local network), FQDN, etc.)
        /// </param>
        /// <param name="userName">The UserName to connect with.</param>
        /// <param name="password">The Password to connect with.</param>
        public static RemotePowerShellConnectionInfo Create
        (
            string computerAddress,
            string userName,
            SecureString password
        )
        {
            return new RemotePowerShellConnectionInfo
            {
                ComputerAddress = computerAddress,
                Credentials = new PSCredential
                (
                    userName,
                    password
                )
            };
        }

        /// <summary>
        /// Creates Remote PowerShell Connection Information for a given computer, using a
        /// standard UserName and Password combination.
        /// </summary>
        /// <param name="computerAddress">
        /// The computer address (IP Address, Machine Name (local network), FQDN, etc.)
        /// </param>
        /// <param name="port">
        /// The port to connect on.
        /// </param>
        /// <param name="userName">The UserName to connect with.</param>
        /// <param name="password">The Password to connect with.</param>
        public static RemotePowerShellConnectionInfo Create
        (
            string computerAddress,
            ushort port,
            string userName,
            SecureString password
        )
        {
            var rpsci = Create(computerAddress, userName, password);
            rpsci.Port = port;
            return rpsci;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object ICloneable.Clone() => this.Clone();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public RemotePowerShellConnectionInfo Clone() =>
            (RemotePowerShellConnectionInfo)this.MemberwiseClone();

        /// <summary>
        /// Converts this instance into runspace connection information.
        /// </summary>
        internal RunspaceConnectionInfo ToRunspaceConnectionInfo()
        {
            return new WSManConnectionInfo
            (
                UseSecurePowerShell,
                ComputerAddress,
                Port,
                "/wsman",
                "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                Credentials
            )
            {
                UseCompression = true,
                OperationTimeout = (int)this.OperationTimeout.TotalMilliseconds,
                OpenTimeout = (int)this.ConnectionTimeout.TotalMilliseconds,
                SkipCACheck = !RequireValidCertificate,
                SkipCNCheck = !RequireValidCertificate
            };
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = ComputerAddress;

            if (IsLocalConnection())
            {
                result = "localhost";
            }

            if (Port >= 0) { result += ":" + Port; }

            return result;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode() ^ GetType().GetHashCode();
        }
    }
}