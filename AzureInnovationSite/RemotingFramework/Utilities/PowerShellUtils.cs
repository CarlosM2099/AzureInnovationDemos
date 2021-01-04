using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RemotingFramework.PowerShell;

namespace RemotingFramework.Utilities
{
    /// <summary>
    /// Static Utility Methods for working with PowerShell.
    /// </summary>
    public static class PowerShellUtils
    {
        /// <summary>
        /// Escapes a string into the raw name of a variable.
        /// </summary>
        /// <param name="variableName">
        /// The name of the variable (without the dollar sign or escape characters).
        /// </param>
        /// <returns>
        /// <para>
        /// An escaped proper variable name (including the dollar sign and curly braces).
        /// </para>
        /// <para>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_variables
        /// </para>
        /// </returns>
        public static string EscapeVariableName(string variableName)
        {
            variableName = EscapePowerShellPartial(variableName, false);
            return variableName == null
                ? "$null"
                : "${" + variableName + "}";
        }

        /// <summary>
        /// Escapes a raw string value into a formatted and escaped string literal that can
        /// be used in a raw PowerShell script.
        /// </summary>
        /// <param name="rawValue">The raw string value to be escaped.</param>
        /// <returns>
        /// A formatted and escaped string literal that can be inserted into a powershell
        /// script verbatim, includes proper quotes and escape characters.
        /// </returns>
        /// <remarks>
        /// See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_special_characters
        /// </remarks>
        public static string EscapeString(string rawValue)
        {
            rawValue = EscapePowerShellPartial(rawValue, true);

            return rawValue == null
                ? "$null"
                : "\"" + rawValue + "\"";
        }

        private static string EscapePowerShellPartial(string input, bool escapeDoubleQuotes)
        {
            StringBuilder sb = null;

            if (input != null)
            {
                sb = new StringBuilder();
                foreach (char c in input)
                {
                    if (c == ((char)0)) { sb.Append("`0"); }
                    else if (c == ((char)7)) { sb.Append("`a"); }
                    else if (c == ((char)8)) { sb.Append("`b"); }
                    else if (c == ((char)9)) { sb.Append("`t"); }
                    else if (c == ((char)10)) { sb.Append("`n"); }
                    else if (c == ((char)11)) { sb.Append("`v"); }
                    else if (c == ((char)12)) { sb.Append("`f"); }
                    else if (c == ((char)13)) { sb.Append("`r"); }
                    else if (escapeDoubleQuotes && c == '"') { sb.Append("\"\""); }
                    else
                    {
                        if (c == '`' || c == '{' || c == '}' || c == '$') { sb.Append('`'); }
                        sb.Append(c);
                    }
                }
            }

            return sb?.ToString();
        }

        /// <summary>
        /// Creates a PowerShell <see cref="Command" /> that can be Invoked by an object
        /// that implements <see cref="IRemotePowerShell" />.
        /// </summary>
        /// <param name="cmdlet">The name of the cmdlet to invoke.</param>
        /// <param name="parameters">
        /// An anonymous object containing the parameters for the cmdlet.
        /// </param>
        /// <param name="switches">A collection of switches for the cmdlet.</param>
        public static Command CreateCommand
        (
            string cmdlet,
            object parameters = null,
            params string[] switches
        )
        {
            var cmd = new Command(cmdlet, false);

            foreach (var param in ConvertToDictionary(parameters))
            {
                cmd.Parameters.Add(param.Key, param.Value);
            }

            if (switches?.Any() == true)
            {
                foreach (var @switch in switches)
                {
                    if (!string.IsNullOrWhiteSpace(@switch))
                    {
                        cmd.Parameters.Add(@switch);
                    }
                }
            }

            return cmd;
        }

        private static Dictionary<string, object> ConvertToDictionary(object o)
        {
            var result = new Dictionary<string, object>();
            if (o != null)
            {
                bool done = false;

                try
                {
                    var col = o as IEnumerable;
                    if (col != null)
                    {
                        foreach (dynamic c in col)
                        {
                            if (c is null) { continue; }

                            string name = null;
                            try
                            {
                                name = c.Key;
                            }
                            catch
                            {
                                name = c.Name;
                            }

                            result[name] = c.Value;
                            done = true;
                        }
                    }
                }
                catch
                {
                    done = false;
                }

                if (!done)
                {
                    var props = o.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        var getter = prop.GetAccessors(false).Where
                        (
                            mi => mi.ReturnType != typeof(void)
                            && mi.GetParameters().Length == 0
                        )
                        .FirstOrDefault();

                        result[prop.Name] = getter.Invoke(o, new object[0]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Configures a connected PowerShell session to write its output to the Console.
        /// </summary>
        /// <param name="rps">
        /// The connected <see cref="IRemotePowerShell" /> instance to configure.
        /// </param>
        /// <returns>
        /// <c>true</c> if configuration is successful; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The PowerShell session must be open and connected, or this method will not
        /// succeed; additionally if you close and re-open it, it will need to be
        /// configured again.
        /// </remarks>
        public static bool ConfigureNonInteractiveConsoleHost(this IRemotePowerShell rps)
        {
            bool success = false;
            if (rps?.HostUserInterface != null)
            {
                rps.HostUserInterface.PromptCallback = (caption, message, descriptions) =>
                {
                    throw new NotSupportedException
                    (
                        "PowerShell is prompting for user input! " + Environment.NewLine
                        + JsonConvert.SerializeObject
                        (
                            new
                            {
                                Caption = caption,
                                Message = message,
                                Descriptions = descriptions
                            },
                            Formatting.Indented
                        )
                    );
                };

                rps.HostUserInterface.PromptForChoiceCallback = (caption, message, choices, defaultChoice) =>
                {
                    WithConsoleLock(() =>
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine
                        (
                            "[PowerShell-PromptForChoice] "
                        );
                        Console.WriteLine("Accepting DefaultChoice: " + defaultChoice);
                        Console.ResetColor();
                    });

                    return defaultChoice;
                };

                rps.HostUserInterface.PromptForCredentialsCallback = (caption, message, userName, targetName, allowedCredentialTypes, options) =>
                {
                    throw new NotSupportedException
                    (
                        "PowerShell Prompting for Credentials!" + Environment.NewLine
                        + JsonConvert.SerializeObject
                        (
                            new
                            {
                                Caption = caption,
                                Message = message,
                                UserName = userName,
                                TargetName = targetName,
                                AllowedCredentialTypes = allowedCredentialTypes,
                                Options = options
                            },
                            Formatting.Indented
                        )
                    );
                };

                rps.HostUserInterface.ReadLineCallback = () =>
                {
                    throw new NotSupportedException("PowerShell trying to ReadLine!");
                };

                rps.HostUserInterface.WriteCallback = (level, foregroundColor, backgroundColor, message) =>
                {
                    WithConsoleLock(() =>
                    {
                        Console.ResetColor();

                        if (!foregroundColor.HasValue && !backgroundColor.HasValue)
                        {
                            // default colors:
                            if (level == PowerShellWriteLevel.Debug) { foregroundColor = ConsoleColor.Cyan; }
                            else if (level == PowerShellWriteLevel.Verbose) { foregroundColor = ConsoleColor.Green; }
                            else if (level == PowerShellWriteLevel.Standard) { foregroundColor = ConsoleColor.White; }
                            else if (level == PowerShellWriteLevel.Warning) { foregroundColor = ConsoleColor.Yellow; }
                            else if (level == PowerShellWriteLevel.Error)
                            {
                                foregroundColor = ConsoleColor.White;
                                backgroundColor = ConsoleColor.DarkRed;
                            }
                        }

                        if (foregroundColor.HasValue) { Console.ForegroundColor = foregroundColor.Value; }
                        if (backgroundColor.HasValue) { Console.BackgroundColor = backgroundColor.Value; }

                        Console.WriteLine($"[PowerShell-{level}] {(message ?? string.Empty).Trim()}");
                        Console.ResetColor();
                    });
                };

                rps.HostUserInterface.WriteProgressCallback = (sourceId, record) =>
                {
                    if (record?.Activity?.EqualsIgnoreCase("Preparing modules for first use.") != true)
                    {
                        WithConsoleLock(() =>
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.BackgroundColor = ConsoleColor.Black;

                            Console.Write("[PowerShell-Progress");
                            if (sourceId != 0)
                            {
                                Console.Write($"({sourceId})");
                            }
                            Console.WriteLine("]");
                            Console.WriteLine(JsonConvert.SerializeObject(record, Formatting.Indented));
                            Console.WriteLine("[/PowerShell-Progress]");
                            Console.ResetColor();
                        });
                    }
                };

                success = true;
            }

            return success;
        }

        private static void WithConsoleLock(Action action)
        {
            bool locked = false;
            try
            {
                locked = Monitor.TryEnter(Console.Out, TimeSpan.FromMinutes(1));
                action?.Invoke();
            }
            finally
            {
                if (locked)
                {
                    Monitor.Exit(Console.Out);
                }
            }
        }
    }
}