namespace SqlLockFinder.SessionDetail.LockSummary
{
    public class LockSummaryDto
    {
        public string FullObjectName { get; set; }
        public int Count { get; set; }
        public string Mode { get; set; }

        public override string ToString()
        {
            return $"{FullObjectName,30} {Count,6}{Mode,-3}";
        }
    }
}