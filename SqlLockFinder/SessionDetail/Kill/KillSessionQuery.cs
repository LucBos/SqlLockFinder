using System;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.Kill
{
    public interface IKillSessionQuery
    {
        QueryResult Execute(int spid);
    }

    public class KillSessionQuery : IKillSessionQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public KillSessionQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public QueryResult Execute(int spid)
        {
            var queryResult = new QueryResult();
           
            try
            {
                var connection = connectionContainer.GetConnection();
                connection.ChangeDatabase("master");
                connection.Execute($"KILL {spid}");
            }
            catch (Exception ex)
            {
                queryResult.Errors.Add(ex.ToString());
            }

            return queryResult;
        }
    }
}
