using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace QisWatchdogService
{
    internal class WatchdogWorkerService : IHostedService, IDisposable
    {
        private readonly ILogger<WatchdogWorkerService> logger;
        private readonly List<ServiceTracking> serviceTracking;
        private readonly GlobalSettings globalSettings;
        public WatchdogWorkerService(ILogger<WatchdogWorkerService> logger, IOptions<List<ServiceTracking>> serviceTracking, IOptions<GlobalSettings> globalSettingsOptions)
        {
            this.logger = logger;
            this.serviceTracking = serviceTracking.Value;
            this.globalSettings = globalSettingsOptions.Value; 
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WatchdogServiceTimerInitTimer();
            return Task.CompletedTask;
        }

        #region WatchdogServiceTimer
        private System.Timers.Timer WatchdogServiceTimer;
        private void WatchdogServiceTimerInitTimer()
        {
            if (WatchdogServiceTimer == null)
            {
                WatchdogServiceTimer = new System.Timers.Timer();
                WatchdogServiceTimer.Elapsed += WatchdogServiceTimer_Elapsed;
                WatchdogServiceTimer.Interval = globalSettings.TrackingIntervalSeconds.HasValue ? globalSettings.TrackingIntervalSeconds.Value * 1000 : 60000;
                WatchdogServiceTimer.AutoReset = false;
            }
            WatchdogServiceTimer.Start();
        }

        private void WatchdogServiceTimerStopTimer()
        {
            if (WatchdogServiceTimer != null)
            {
                WatchdogServiceTimer.Enabled = false;
                WatchdogServiceTimer.Stop();
            }
        }

        private void WatchdogServiceTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                foreach (var _serviceTracking in serviceTracking)
                {
                    ServiceController service = new ServiceController(_serviceTracking.ServiceName);
                    if (service == null) continue;
                    if (service.Status == ServiceControllerStatus.Running || service.Status== ServiceControllerStatus.StartPending)
                    {
                        _serviceTracking.RecoveryCount = 0;
                        //var directory = new DirectoryInfo(_serviceTracking.LogFilePath);
                        //var logFile = directory.GetFiles().Where(f=>f.Name.Contains(_serviceTracking.LogFileContains))
                        //             .OrderByDescending(f => f.LastWriteTime)
                        //             .FirstOrDefault();
                        //if (logFile != null)
                        //{
                        //    if (logFile.Name == _serviceTracking.LogFileName)
                        //    {

                        //    }
                        //    else
                        //    {
                        //        _serviceTracking.LogFileName = logFile.Name;
                        //        _serviceTracking.LastLogLine = 0;
                        //    }
                        //}
                    }
                    else
                    {
                        if (_serviceTracking.RecoveryCount < globalSettings.RecoveryRetryCount)
                        {

                            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RestartService.bat");
                            var StartInfo = new ProcessStartInfo(path)
                            {
                                Arguments = $"{_serviceTracking.ServiceName} {_serviceTracking.ProcessName}",
                                CreateNoWindow = false,
                                UseShellExecute = false,
                                //RedirectStandardOutput = true,
                                //RedirectStandardError = true,
                                Verb = "runas"

                            };
                            //Process.Start(StartInfo);
                            Process p = new Process();
                            p.StartInfo = StartInfo;
                            p.Start();
                            //while (!p.StandardOutput.EndOfStream)
                            //{
                            //    logger.LogInformation(p.StandardOutput.ReadLine());
                            //}
                            //p.WaitForExit();
                            _serviceTracking.RecoveryCount++;
                        }
                        //TODO: in case we have more than RecoveryRetryCount should be mail alarm 
                        else
                        {

                        }
                    }
                    //_serviceTracking.LastLogLine = 0;

                    

                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "WatchdogServiceTimer_Elapsed");
            }
            finally
            {
                WatchdogServiceTimer.Start();
            }

        }
        #endregion

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StopAsync Called");
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            logger.LogInformation("Dispose Called");
        }
    }
}
