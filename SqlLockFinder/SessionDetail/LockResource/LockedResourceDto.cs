namespace SqlLockFinder.SessionDetail.LockResource
{
    public class LockedResourceDto
    {
        public int SPID { get; set; }
        public string SchemaName { get; set; }
        public string EntityName { get; set; }
        public int IndexId { get; set; }
        public string ResourceType { get; set; }
        public string ResourceSubType { get; set; }
        public string Description { get; set; }
        public string Mode { get; set; }
        public string Status { get; set; }
        public string RequestType { get; set; }

        public string FullObjectName => $"{SchemaName ?? ""}.{EntityName ?? ""}";
        public bool IsKeyLock => ResourceType == "KEY";
        public bool IsRIDLock => ResourceType == "RID";
        public bool IsPageLock => ResourceType == "PAGE";
        public bool IsTableLock => ResourceType == "OBJECT";
        public bool IsDbLock => ResourceType == "DATABASE";

        public bool SameLockAs(LockedResourceDto other)
        {
            return other.SchemaName == SchemaName
                   && other.EntityName == EntityName
                   && other.IndexId == IndexId
                   && other.ResourceType == ResourceType
                   && other.Description == Description;
        }
    }
}