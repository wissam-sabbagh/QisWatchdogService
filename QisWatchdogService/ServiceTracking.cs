using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QisWatchdogService
{
    internal class ServiceTracking
    {
        public string ServiceName { get; set; }
        public string ProcessName { get; set; }
        public string LogFilePath { get; set; }
        public string LogFileContains { get; set; }
        public List<string> LogTracking { get; set; }

        public int LastLogLine { get; set; }
        public string LogFileName { get; set; }

        public int RecoveryCount { get; set; } 

       
    }
}
