using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockResource
{
    public interface IGetLockResourcesBySpidQuery
    {
        Task<QueryResult<List<LockedResourceDto>>> Execute(int[] spids, string databaseName);
    }

    public class GetLockResourcesBySpidQuery : IGetLockResourcesBySpidQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public GetLockResourcesBySpidQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public async Task<QueryResult<List<LockedResourceDto>>> Execute(int[] spids, string databaseName)
        {
            var queryResult = new QueryResult<List<LockedResourceDto>>();
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase(databaseName);

                var spidStrings = spids.Select(x => x.ToString()).ToArray();

                var result = connection
                    .QueryAsync<LockedResourceDto>(@"
SELECT t.request_session_id AS SPID,
    CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_NAME(p.object_id)
    END AS EntityName,
    CASE
        WHEN t.resource_type = 'OBJECT' THEN OBJECT_SCHEMA_NAME(t.resource_associated_entity_id)
        WHEN t.resource_associated_entity_id = 0 THEN 'n/a'
        ELSE OBJECT_SCHEMA_NAME(p.object_id)
    END AS SchemaName,
    p.index_id as IndexId,
    t.resource_type as ResourceType,
    t.resource_subtype AS ResourceSubType,
    t.resource_description AS Description,
    t.request_mode AS Mode,
    t.request_status AS Status,
    t.request_owner_type AS RequestType
FROM sys.dm_tran_locks t
LEFT JOIN sys.partitions p ON p.partition_id = t.resource_associated_entity_id
WHERE t.resource_database_id = DB_ID() AND t.request_session_id IN @spids
AND (t.resource_type = 'KEY' OR t.resource_type = 'RID' OR t.resource_type = 'PAGE')", new {spids = spidStrings});
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