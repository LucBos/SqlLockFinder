namespace SqlLockFinder.SessionDetail
{
    public class LockSummaryDto
    {
        public string FullObjectName { get; set; }
        public int Count { get; set; }
        public string Mode { get; set; }
    }
}