namespace SqlLockFinder.SessionDetail.LockSummary
{
    public class LockSummaryDto
    {
        public string FullObjectName { get; set; }
        public string ResourceType { get; set; }
        public int Count { get; set; }
        public string Mode { get; set; }

        public bool IsKeyLock => ResourceType == "KEY";
        public bool IsRIDLock => ResourceType == "RID";
        public bool IsPageLock => ResourceType == "PAGE";
        public bool IsTableLock => ResourceType == "OBJECT";
        public bool IsDbLock => ResourceType == "DATABASE";
        public bool IsApplicationLock => ResourceType == "APPLICATION";

        public override string ToString()
        {
            return $"{FullObjectName,30} {Count,6}{Mode,-3}";
        }
    }
}