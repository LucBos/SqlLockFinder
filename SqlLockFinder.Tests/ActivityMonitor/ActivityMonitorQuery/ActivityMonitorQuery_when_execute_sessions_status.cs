using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery;

namespace SqlLockFinder.Tests.ActivityMonitor
{
    class ActivityMonitorQuery_when_execute_check_for_sessions_status : ActivityMonitor_TestBase
    {
        [Test]
        public void It_should_return_all_background_sessions_from_all_databases()
        {
            var queryResult = new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "master"
                && x.Command.Trim() == "LOG WRITER"
                && x.Status.Trim() == "background");
        }

        [Test]
        public void It_should_return_all_sleeping_sessions_from_all_databases()
        {
            var queryResult = new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "master"
                && x.Status.Trim() == "sleeping");
        }

        [Test]
        public void It_should_return_all_runnable_sessions_from_all_databases()
        {
            PerformIntensiveSqlTask();

            var queryResult = new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Where(x => x.DatabaseName == "Northwind")
                .Should().Contain(x => x.Status.Trim() == "runnable" || x.Status.Trim() == "running");
        }

        [Test]
        public void It_should_return_all_blocked_sessions_from_all_databases()
        {
            connection1.Query("USE Northwind", transaction: transaction1);
            connection1.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ALFKI'", transaction: transaction1);

            connection2.Query("USE Northwind", transaction: transaction2);
            connection2.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ALFKI'", transaction: transaction2);

            var queryResult = new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "Northwind"
                && x.Status.Trim() == "suspended");
        }
    }
}