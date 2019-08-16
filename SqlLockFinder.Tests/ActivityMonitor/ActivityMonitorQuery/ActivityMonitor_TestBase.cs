using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    public class ActivityMonitor_TestBase: DoubleConnectionBaseTest
    {
        protected CancellationTokenSource cancellationTokenSource;

        [SetUp]
        public void BaseSetup()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void BaseTeardown()
        {
            cancellationTokenSource?.Cancel();
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
