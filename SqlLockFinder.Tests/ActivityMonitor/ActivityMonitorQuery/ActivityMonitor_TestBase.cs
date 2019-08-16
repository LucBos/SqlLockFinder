using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    public class ActivityMonitor_TestBase
    {
        protected SqlConnection connection1;
        protected SqlConnection connection2;
        protected SqlTransaction transaction1;
        protected SqlTransaction transaction2;
        protected CancellationTokenSource cancellationTokenSource;

        [SetUp]
        public void BaseSetup()
        {
            connection1 =
                new SqlConnection(
                    "Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=True;Application Name=SqlLockFinder;Connection Timeout=30;");
            connection2 =
                new SqlConnection(
                    "Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=True;Application Name=SqlLockFinder;Connection Timeout=30;");
            connection1.Open();
            connection2.Open();

            transaction1 = connection1.BeginTransaction();
            transaction2 = connection2.BeginTransaction();
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void BaseTeardown()
        {
            cancellationTokenSource?.Cancel();

            transaction1?.Rollback();
            transaction2?.Rollback();

            connection1?.Close();
            connection2?.Close();
        }

        protected void PerformIntensiveSqlTask()
        {
            var task = Task.Run(IntensiveSqlTask, cancellationTokenSource.Token);
            while (!task.Status.Equals(TaskStatus.Running))
            {
                Thread.Sleep(100);
            }
        }

        private void IntensiveSqlTask()
        {
            connection1.Query("USE Northwind", transaction: transaction1);

            connection1.ExecuteAsync(@"
                    CREATE TABLE SplitThrash
                    (
                     id UNIQUEIDENTIFIER default newid(),
                     parent_id UNIQUEIDENTIFIER default newid(),
                     name VARCHAR(50) default cast(newid() as varchar(50))
                    );");

            connection1.ExecuteAsync(@"
                    SET NOCOUNT ON;
                    INSERT INTO SplitThrash DEFAULT VALUES;
                    GO  1000000");

            connection1.ExecuteAsync(@"
                    CREATE CLUSTERED INDEX [ClusteredSplitThrash] ON [dbo].[SplitThrash]
                    (
                     [id] ASC,
                     [parent_id] ASC
                    );");

            connection1.ExecuteAsync(@"
                    UPDATE SplitThrash
                    SET parent_id = newid(), id = newid();
                    GO 10000");

            while (true)
            {
                connection1.ExecuteAsync(@"
                        UPDATE dbo.Customers
                        SET PostalCode = PostalCode
                        WHERE CustomerID = 'ALFKI'", transaction: transaction1);
            }
        }
    }
}
