using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockSummary
{
    public interface IGetLockSummaryFromSpidQuery
    {
        Task<QueryResult<List<LockSummaryDto>>> Execute(int spid, string databaseName);
    }

    public class GetLockSummaryFromSpidQuery : IGetLockSummaryFromSpidQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public GetLockSummaryFromSpidQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public async Task<QueryResult<List<LockSummaryDto>>> Execute(int spid, string databaseName)
        {
            var queryResult = new QueryResult<List<LockSummaryDto>>();
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase(databaseName);

                var result = connection
                    .QueryAsync<LockSummaryDto>(@"
SELECT 
	(CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_SCHEMA_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_SCHEMA_NAME(p.object_id)
    END) + '.' +
    (CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_NAME(p.object_id)
    END) AS FullObjectName,
    t.resource_type as ResourceType,
    t.request_mode AS Mode,
	COUNT(1) AS COUNT
FROM sys.dm_tran_locks t
LEFT JOIN sys.partitions p ON p.partition_id = t.resource_associated_entity_id
WHERE (t.resource_type = 'KEY' OR t.resource_type = 'RID' OR t.resource_type = 'PAGE' OR t.resource_type = 'APPLICATION'  OR t.resource_type = 'OBJECT')
AND t.request_session_id  = @spid
GROUP BY (CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_SCHEMA_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_SCHEMA_NAME(p.object_id)
    END) + '.' +
    (CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_NAME(p.object_id)
    END),
	t.resource_type,
    t.request_mode
ORDER BY COUNT(1) DESC", new { spid });
                queryResult.Result = (await result).ToList();
            }
            catch (Exception e)
            {
                queryResult.Errors.Add(e.ToString());
            }

            return queryResult;
        }
    }
}