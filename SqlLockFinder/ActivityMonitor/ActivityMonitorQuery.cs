using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.ActivityMonitor
{
    public interface IActivityMonitorQuery
    {
        Task<QueryResult<List<SessionDto>>> Execute();
    }

    public class ActivityMonitorQuery : IActivityMonitorQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public ActivityMonitorQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public async Task<QueryResult<List<SessionDto>>> Execute()
        {
            var queryResult = new QueryResult<List<SessionDto>>();
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase("master");
                var result = connection.QueryAsync<SessionDto>(@"
SELECT
       s.session_id AS SPID,
       s.login_name AS LoginName,
       DB_NAME(s.database_id) AS DatabaseName,
       ISNULL(r.status, s.status) AS [Status],
       s.open_transaction_count AS OpenTransactions,
       ISNULL(( SELECT text FROM sys.dm_exec_sql_text(r.sql_handle)), r.command) AS Command,
       s.[program_name] AS ProgramName,
       r.wait_time AS [WaitTimeMs],
       ISNULL(r.wait_type, r.last_wait_type + ' (last_wait_type)') AS WaitType,
       r.wait_resource,
       s.cpu_time AS [TotalSessionCPUms],
       CASE WHEN (s.writes + s.reads) > 131072 THEN CAST(CAST(((s.writes + s.reads) *8/1024/1024.00) AS NUMERIC(9,2)) AS VARCHAR(4000)) + 'GB' ELSE CAST(CAST(((s.writes + s.reads) *8/1024.00) AS NUMERIC(9,2)) AS VARCHAR(4000)) + 'MB' END AS PhysicalIO,
       CASE WHEN (s.memory_usage) > 131072 THEN CAST(CAST(((s.memory_usage) *8/1024/1024.00) AS NUMERIC(9,2)) AS VARCHAR(4000)) + 'GB' ELSE CAST(CAST(((s.memory_usage) *8/1024.00) AS NUMERIC(9,2)) AS VARCHAR(4000)) + 'MB' END AS MemoryUsage,
       s.login_time AS LoginTime,
       s.last_request_start_time AS LastBatchStarted,
       s.[host_name] AS HostName,
       r.blocking_session_id AS BlockedBy,
       is_user_process AS IsUserProcess
FROM sys.dm_exec_sessions s LEFT OUTER JOIN sys.dm_exec_requests r
ON r.session_id = s.session_id");
                queryResult.Result = (await result).ToList();
            }
            catch (Exception ex)
            {
                queryResult.Errors.Add(ex.ToString());
            }

            return queryResult;
        }
    }
}