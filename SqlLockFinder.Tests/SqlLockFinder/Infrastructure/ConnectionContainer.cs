using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlLockFinder.Infrastructure
{
    public interface IConnectionContainer
    {
        void Create(string connectionString);
        IDbConnection GetConnection();
    }

    public class ConnectionContainer : IConnectionContainer, IDisposable
    {
        private static IConnectionContainer instance;

        private SqlConnection connection;

        private ConnectionContainer()
        { }

        public void Create(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public IDbConnection GetConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

        public void Dispose()
        {
            connection?.Dispose();
        }

        public static IConnectionContainer Instance => instance ?? (instance = new ConnectionContainer());
    }
}