using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.ActivityMonitor
{
    public interface IActivityMonitorQuery
    {
        QueryResult<List<SessionDto>> Execute();
    }

    public class ActivityMonitorQuery : IActivityMonitorQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public ActivityMonitorQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public QueryResult<List<SessionDto>> Execute()
        {
            var queryResult = new QueryResult<List<SessionDto>>();
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase("master");
                var result = connection.Query<SessionDto>(@"
SELECT
    p.spid as SPID,
    CASE WHEN s.is_user_process = 1 THEN 0 ELSE 1 END AS IsUserProcess,
    p.loginame AS LoginName, 
    ISNULL(db_name(p.dbid),N'') AS DatabaseName,
    p.status AS Status,
    p.open_tran AS OpenTransactions,
    p.cmd AS Command,
    p.program_name AS ProgramName,
    p.waittime AS WaitTime,
    CASE WHEN p.waittype = 0 THEN N'' ELSE p.lastwaittype END AS WaitType,
    CASE WHEN p.waittype = 0 THEN N'' ELSE RTRIM(p.waitresource) END AS WaitResource,
    p.cpu AS CPU,
    p.physical_io AS PhysicalIO,
    p.memusage AS MemoryUsage,
    p.login_time AS LoginTime,
    p.last_batch AS LastBatch,
    p.hostname AS HostName,
    p.net_address AS NetAddress,
    p.blocked AS BlockedBy
FROM master.dbo.sysprocesses p, master.sys.dm_exec_sessions s
WHERE p.spid = s.session_id
ORDER BY p.spid").ToList();
                queryResult.Result = result;
            }
            catch (Exception ex)
            {
                queryResult.Errors.Add(ex.ToString());
            }

            return queryResult;
        }
    }
}