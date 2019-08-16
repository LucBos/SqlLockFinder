using System;
using System.Data;
using System.Data.SqlClient;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.Tests
{
    public class TestConnectionContainer : IConnectionContainer
    {
        public void Create(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDbConnection GetConnection()
        {
            var connection = new SqlConnection(
                "Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=True;Application Name=SqlLockFinder;Connection Timeout=30;");
            connection.Open();
            return connection;
        }
    }
}