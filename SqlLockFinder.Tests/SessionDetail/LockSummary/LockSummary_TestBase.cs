using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.Tests.SessionDetail.LockSummary
{
    class LockSummary_TestBase
    {
        protected LockedResourceDto CreateLockResource(string schema, string entity, string mode, string resourceType)
        {
            return new LockedResourceDto
            {
                SchemaName = schema,
                EntityName = entity,
                Mode = mode,
                ResourceType = resourceType
            };
        }

    }
}