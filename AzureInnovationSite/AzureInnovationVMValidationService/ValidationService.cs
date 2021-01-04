using AzureInnovationVMValidationService.Models;
using Microsoft.MD;
using Microsoft.MD.Common.Remoting.RemoteDesktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

using Exception = System.Exception;

namespace AzureInnovationVMValidationService
{
    public class ValidationService : ServiceControl, IDisposable
    {
        private readonly object SyncRoot = new object();
        private HostControl HostControl { get; set; } = null;
        private CancellationTokenSource CancellationTokenSource;
        private Task ServiceTask;
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
         Justification = "While it's not desired, it's okay for the wrapped code block to silently fail.")]
        public bool Start(HostControl hostControl)
        {
            
            if (hostControl != null)
            {
                this.HostControl = hostControl;
            }

            lock (SyncRoot)
            {
                Log("Validation Service has started", EventLogEntryType.Information);

                Stop(HostControl);
                try
                {
                    this.HostControl?.RequestAdditionalTime(TimeSpan.FromMinutes(5));
                }
                catch { /* do nothing */ }

                CancellationTokenSource = new CancellationTokenSource();
                ServiceTask = Run(CancellationTokenSource.Token);
                //Logger.Info("Started Management Service.");

                try
                {
                    if (Environment.UserInteractive)
                    {
                        var ver = typeof(Program).Assembly.GetName().Version;
                        var buildDate = DateTime.Parse("1/1/2000")
                            + TimeSpan.FromDays(ver.Build)
                            + TimeSpan.FromSeconds(ver.Revision * 2);

                        Console.Title = Program.ServiceName + " " + ver + " (Built @ "
                            + buildDate + ") [on " + Environment.MachineName + "]";

                        Console.WindowWidth = 180;
                        Console.WindowHeight = 40;
                        Console.BufferWidth = 300;
                        Console.BufferHeight = 9000;
                    }
                }
                catch { /* do nothing */ }

                return true;
            }
        }
        /// <summary>
        /// This is the main service method of the service.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        private async Task Run(CancellationToken token)
        {
            try
            {
                var cfg = Models.Config.AzureInnovationVMValidationServiceCfg.Config.Value;
                List<RemoteSession> vmSessions = new List<RemoteSession>();
                List<string> vmDeskContent = new List<string>();

                foreach (var vmUser in cfg.VmUsers.Items)
                {
                    vmSessions.Add(new RemoteSession
                    {
                        ClientWidth = 1280,
                        ClientHeight = 720,
                        AllowRemoteClipboard = true,
                        CompatibilityMode = true,
                        ImageEncoding = ImageEncoding.AUTO,
                        ServerAddress = $"{cfg.VmData.HostName}:{cfg.VmData.RemoteDesktopPort}",
                        UserName = $"azuread\\{vmUser.RemoteDesktopUserName}",
                        UserPassword = vmUser.RemoteDesktopPassword
                    });
                }

                try
                {
                    foreach (var vmSession in vmSessions)
                    {
                        vmSession.Connect(token, TimeSpan.FromMinutes(1));

                        Log($"User {vmSession.UserName} connected to VM : {vmSession.ServerAddress}", EventLogEntryType.Information);
                    }
                }
                catch (Exception ex)
                {
                    Log($"VM RDP initialization error: {ex.Message}, stacktrace: {ex.StackTrace}", EventLogEntryType.Error);
                    SendNotification("VM RDP initialization error", ex);                    
                    return;
                }
                
                string[] desktopTextContent = cfg.VmData.ValidDesktopText.Split(',');

                foreach (var vmSession in vmSessions)
                {
                    string desktopText = "";
                    var startedAt = DateTime.UtcNow;
                    int tryCount = 1;
                    while (!token.IsCancellationRequested && desktopText == "" && tryCount  < 5 )
                    {
                        Log($"Getting VM : {vmSession.ServerAddress} desktop", EventLogEntryType.Information);

                        var imgCol = vmSession.GetLatestImage();

                        if (imgCol.ScreenBuffer != null)
                        {                            
                            var img = imgCol.ScreenBuffer;

                            Log($"Getting VM desktop text content", EventLogEntryType.Information);

                            desktopText = await Utils.GetImageContent(img, vmSession.UserName.Replace("azuread\\", "vm_val_").ToLower().Replace(".com", "") + ".jpg");
                             
                            if (desktopText.ToLower().Contains("welcome"))
                            {
                                desktopText = "";
                            }
                            vmSession.RequestFullscreenUpdate();
                        }

                        if (desktopText == "")
                        {
                            Console.Out.WriteLine($"1 min wait.");
                            Thread.Sleep(TimeSpan.FromMinutes(1));
                        }
                        else
                        {
                            vmDeskContent.Add(desktopText);
                        }
                       
                        tryCount++;
                    }
                }

                if (vmDeskContent.Count != vmSessions.Count)
                {
                    string content="";
                    vmDeskContent.ForEach(c => content += c);

                    Log($"Not all VMs returned desktop content, error: {content}", EventLogEntryType.Error);
                    SendNotification("VM content result count", new Exception("Not all VMs returned desktop content"));                    
                    return;
                }

                foreach (var vmDesktop in vmDeskContent)
                {
                    var hasContent = true;
                    string desktopContent = System.Text.RegularExpressions.Regex.Unescape(vmDesktop);
                    desktopContent = desktopContent.Replace("\n", "").Replace(" ", "");

                    desktopTextContent.ForEach(t => { hasContent &= desktopContent.Contains(t); });

                    if (!hasContent)
                    {
                        Log($"Not all VMs returned valid desktop content, error: {vmDesktop}", EventLogEntryType.Error);
                        SendNotification($"VM content result", new Exception($"Not all VMs returned valid desktop content, error: {vmDesktop}"));                        
                        return;
                    }
                }

                Log("VMs validated successfully", EventLogEntryType.Information);               
            }
            catch (OperationCanceledException)
            { /* swallow */ }
            catch (Exception ex)
            {
                //Logger.Error(ex);

                if (!token.IsCancellationRequested)
                {                    
                    Log("Service has terminated unexpectedly.", EventLogEntryType.Error);
                    SendNotification("Service has terminated unexpectedly.", ex);
                }

                // MUST *NOT* WAIT FOR THE "STOP" METHOD TO FINISH EXECUTING.
                Task.Run(() => this.HostControl?.Stop()).ConfigureAwait(false).GetAwaiter();
            }
        }


        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
           Justification = "Must not throw.")]
        private void SendNotification(string operation, Exception ex)
        {
            try
            {
                Utils.SendEmail
                (
                    MailPriority.High,
                    "Azure Innovation Demos - VM validation Service Failure Notification",
                    new Block
                    (
                        new Header(1, "Azure Innovation Demo - VM validation Service Failure Notification"),
                        "Operation: " + operation,
                        "Exception: " + (ex?.ToString() ?? "Unknown")
                    )
                );
            }
            catch (Exception ex2)
            {
                Log($"Unable to send outage email!, err {ex2.Message},  {ex2.StackTrace}", EventLogEntryType.Error);              
            }
        }

        public bool Stop(HostControl hostControl)
        {
            if (hostControl != null)
            {
                this.HostControl = hostControl;
            }

            lock (SyncRoot)
            {
                try
                {
                    this.HostControl?.RequestAdditionalTime(TimeSpan.FromMinutes(5));
                }
                catch { /* do nothing */ }

                if (CancellationTokenSource != null)
                {
                    // Logger.Debug($"Stopping WD-ATP Management Service.");
                    CancellationTokenSource.Cancel();
                }

                try
                {
                    ServiceTask?.Wait();
                }
                catch (Exception ex)
                {
                    //Logger?.Error(ex);
                }

                ServiceTask = null;
                CancellationTokenSource = null;

                return true;
            }
        }

        public void Log(string logMessage, EventLogEntryType eventType)
        {
            string source = "VM Validation Service";
            string log = "AzureInnovation";
            EventLog demoLog = new EventLog(log)
            {
                Source = source
            };
            demoLog.WriteEntry(logMessage, eventType);
        }
    }
}
