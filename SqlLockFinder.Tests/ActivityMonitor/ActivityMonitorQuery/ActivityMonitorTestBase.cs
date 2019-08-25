using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    public class ActivityMonitorTestBase : DoubleConnection_TestBase
    {
        protected CancellationTokenSource cancellationTokenSource;
        private bool intensiveTaskBusy;

        [SetUp]
        public void BaseSetup()
        {
            intensiveTaskBusy = false;
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void BaseTeardown()
        {
            cancellationTokenSource?.Cancel();
        }

        protected void PerformIntensiveSqlTask()
        {
            IntensiveSqlTask().Wait(500);
        }

        private async Task IntensiveSqlTask()
        {
            connection1.Query("USE Northwind", transaction: transaction1);

            await connection1.ExecuteAsync(@"
                    CREATE TABLE SplitThrash
                    (
                     id UNIQUEIDENTIFIER default newid(),
                     parent_id UNIQUEIDENTIFIER default newid(),
                     name VARCHAR(50) default cast(newid() as varchar(50))
                    );", transaction: transaction1);

            for (int i = 0; i < 10000; i++)
            {
                await connection1.ExecuteAsync(@"
                    SET NOCOUNT ON;
                    INSERT INTO SplitThrash DEFAULT VALUES;", transaction: transaction1);
            }

            await connection1.ExecuteAsync(@"
                    CREATE CLUSTERED INDEX [ClusteredSplitThrash] ON [dbo].[SplitThrash]
                    (
                     [id] ASC,
                     [parent_id] ASC
                    );", transaction: transaction1);

            while (true)
            {
                await connection1.ExecuteAsync(@"
                    UPDATE SplitThrash
                    SET parent_id = newid(), id = newid();", transaction: transaction1);
            }
        }
    }
}