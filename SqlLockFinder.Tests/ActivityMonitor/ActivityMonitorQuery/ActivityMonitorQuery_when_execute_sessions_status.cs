using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    class ActivityMonitorQuery_when_execute_check_for_sessions_status : ActivityMonitorTestBase
    {
        [Test]
        public async Task It_should_return_all_background_sessions_from_all_databases()
        {
            var queryResult = await new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "master"
                && x.Command.Trim() == "LOG WRITER"
                && x.Status.Trim() == "background");
        }

        [Test]
        public async Task It_should_return_all_sleeping_sessions_from_all_databases()
        {
            var queryResult = await new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "master"
                && x.Status.Trim() == "sleeping");
        }

        [Test]
        public async Task It_should_return_all_running_sessions_from_all_databases()
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction:transaction1).First();
            connection1.ExecuteAsync(
                @"SELECT SUM(CAST(message_id AS BIGINT)), SUM(CAST(object_id AS BIGINT)) from sys.messages CROSS JOIN sys.objects OPTION (MAXDOP  1)",
                transaction1);

            var queryResult = await new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Where(x => x.DatabaseName == "master")
                .Should().Contain(x => x.Status.Trim() == "runnable" || x.Status.Trim() == "running");
        }

        [Test]
        public async Task It_should_return_all_blocked_sessions_from_all_databases()
        {
            connection1.Query("USE Northwind", transaction: transaction1);
            connection1.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ANTON'", transaction: transaction1);

            connection2.Query("USE Northwind", transaction: transaction2);
            connection2.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ANTON'", transaction: transaction2);

            var queryResult = await new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "Northwind"
                && x.Status.Trim() == "suspended");
        }
    }
}