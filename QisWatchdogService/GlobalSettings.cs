using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QisWatchdogService
{
    public class GlobalSettings
    {
        public int? TrackingIntervalSeconds { get; set; }
        public int? RecoveryRetryCount { get; set; }

    }
}
