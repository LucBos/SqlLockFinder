using System.Data.SqlClient;
using System.Linq;
using Dapper;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockResource
{
    public interface IGetRowOfLockedResourceQuery
    {
        QueryResult<dynamic> Execute(SessionDto sessionDto, LockedResourceDto lockedResourceDto);
    }

    class GetRowOfLockedResourceQuery : IGetRowOfLockedResourceQuery
    {
        private readonly IConnectionContainer connectionContainer;

        public GetRowOfLockedResourceQuery(IConnectionContainer connectionContainer)
        {
            this.connectionContainer = connectionContainer;
        }

        public QueryResult<dynamic> Execute(SessionDto sessionDto, LockedResourceDto lockedResourceDto)
        {
            var connection = connectionContainer.GetConnection();
            connection.ChangeDatabase(sessionDto.DatabaseName);

            var indexes = connection.Query<SpHelpIndexResult>("EXEC sp_helpindex @objectname",
                new {objectname = lockedResourceDto.FullObjectName});
            var queryResult = new QueryResult<dynamic>();

            foreach (var index in indexes)
            {
                try
                {
                    var rows = connection.Query<dynamic>($@"
SELECT *
FROM {lockedResourceDto.FullObjectName} v2 WITH(INDEX={index.index_name})
WHERE %%lockres%% = @description", new {description = lockedResourceDto.Description});

                    if (rows.Any())
                    {
                        var result = rows.FirstOrDefault();
                        result.IndexName = index.index_name;
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

                    connection.ChangeDatabase(sessionDto.DatabaseName);
                }
            }

            return queryResult;
        }
    }
}