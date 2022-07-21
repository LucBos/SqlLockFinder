using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockResource
{
    public interface IGetRowOfLockedResourceQuery
    {
        Task<QueryResult<dynamic>> Execute(string databaseName, string fullObjectName, string indexName,
            string lockres);
    }

    public class GetRowOfLockedResourceQuery : IGetRowOfLockedResourceQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public GetRowOfLockedResourceQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public async Task<QueryResult<dynamic>> Execute(string databaseName, string fullObjectName, string indexName,
            string lockres)
        {
            var connection = connectionContainer.GetConnection();
            connection.ChangeDatabase(databaseName);

            var queryResult = new QueryResult<dynamic>();

            try
            {
                var rows = await connection.QueryAsync<dynamic>($@"
SELECT *
FROM {fullObjectName} v2 WITH(INDEX={indexName}, NOLOCK)
WHERE %%lockres%% = @description", new { description = lockres });

                if (rows.Any())
                {
                    var result = rows.FirstOrDefault();
                    result.IndexName = indexName;
                    queryResult.Result = result;
                }
            }
            catch (SqlException exc)
            {
                if (exc.Number == 8622) // could not form queryplan because of filtered index
                {
                    queryResult.Warnings.Add(exc.ToString());
                }
                else
                {
                    queryResult.Errors.Add(exc.ToString());
                }

                connection.ChangeDatabase(databaseName);
            }

            return queryResult;
        }
    }
}