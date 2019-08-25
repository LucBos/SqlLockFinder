using System;
using System.Diagnostics;

namespace SqlLockFinder.ActivityMonitor
{
    public class SessionDto
    {
        public int SPID { get; set; }
        public string DatabaseName { get; set; }
        public string Status { get; set; }
        public int OpenTransactions { get; set; }
        public string Command { get; set; }
        public string ProgramName { get; set; }
        public int WaitTimeMs { get; set; }
        public string WaitType { get; set; }
        public string WaitResource { get; set; }
        public int TotalSessionCPUms { get; set; }
        public string PhysicalIO { get; set; }
        public string MemoryUsage { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastBatch { get; set; }
        public string HostName { get; set; }
        public string NetAddress { get; set; }
        public int? BlockedBy { get; set; }

        public override string ToString()
        {
            return $"{SPID} - {DatabaseName} - {Status} - {OpenTransactions} - {ProgramName}";
        }
    }
}