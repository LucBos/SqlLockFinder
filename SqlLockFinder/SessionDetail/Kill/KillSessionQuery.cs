using System;
using System.Threading.Tasks;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.Kill
{
    public interface IKillSessionQuery
    {
        Task<QueryResult> Execute(int spid);
    }

    public class KillSessionQuery : IKillSessionQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public KillSessionQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public async Task<QueryResult> Execute(int spid)
        {
            var queryResult = new QueryResult();
           
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase("master");
                await connection.ExecuteAsync($"KILL {spid}");
            }
            catch (Exception ex)
            {
                queryResult.Errors.Add(ex.ToString());
            }

            return queryResult;
        }
    }
}
