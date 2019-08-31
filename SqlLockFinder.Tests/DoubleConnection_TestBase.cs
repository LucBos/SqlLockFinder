using System;
using System.Data.SqlClient;
using System.Threading;
using NUnit.Framework;

namespace SqlLockFinder.Tests
{
    public class DoubleConnection_TestBase
    {
        protected SqlConnection connection1;
        protected SqlConnection connection2;
        protected SqlTransaction transaction1;
        protected SqlTransaction transaction2;
        protected CancellationTokenSource cancellationTokenSource;

        [SetUp]
        public void SetupConnections()
        {
            connection1 =
                new SqlConnection(
                    $"Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=False;Application Name=SqlLockFinder{Guid.NewGuid()};Connection Timeout=30;");
            connection2 =
                new SqlConnection(
                    $"Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=False;Application Name=SqlLockFinder{Guid.NewGuid()};Connection Timeout=30;");
            connection1.Open();
            connection2.Open();

            transaction1 = connection1.BeginTransaction();
            transaction2 = connection2.BeginTransaction();
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TeardownConnections()
        {
            cancellationTokenSource?.Cancel();
            connection1?.Close();
            connection2?.Close();
        }

    }

    public class SingleConnection_TestBase
    {
        protected SqlConnection connection1;
        protected SqlTransaction transaction1;
        protected CancellationTokenSource cancellationTokenSource;

        [SetUp]
        public void SetupConnections()
        {
            connection1 =
                new SqlConnection(
                    $"Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=False;Application Name=SqlLockFinder{Guid.NewGuid()};Connection Timeout=30;");
            connection1.Open();

            transaction1 = connection1.BeginTransaction();
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TeardownConnections()
        {
            cancellationTokenSource?.Cancel();
            transaction1?.Rollback();
            connection1?.Close();
        }

    }
}