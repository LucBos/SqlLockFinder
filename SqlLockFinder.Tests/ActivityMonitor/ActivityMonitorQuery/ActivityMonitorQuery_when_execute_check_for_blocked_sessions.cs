using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Dapper;
using FluentAssertions;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    public class ActivityMonitorQuery_when_execute_check_for_blocked_sessions: ActivityMonitorTestBase
    {
        [Test]
        public void It_should_return_all_blocked_sessions_from_all_databases()
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction:transaction1).First();
            var spid2 = connection2.Query<int>("SELECT @@SPID", transaction: transaction2).First();

            connection1.Query("USE Northwind", transaction: transaction1);
            connection1.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ALFKI'", transaction: transaction1);

            connection2.Query("USE Northwind", transaction: transaction2);
            connection2.ExecuteAsync(@"UPDATE dbo.Customers
                                    SET PostalCode = PostalCode
                                    WHERE CustomerID = 'ALFKI'", transaction: transaction2);

            Thread.Sleep(1100); // emulate wait time

            var queryResult = new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer()).Execute();
            queryResult.Result.Should().Contain(x => 
                x.DatabaseName == "Northwind" 
                && x.SPID == spid1 
                && x.Status != "suspended");
            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "Northwind"
                && x.SPID == spid2
                && x.Status.Trim() == "suspended"
                && x.BlockedBy == spid1
                && x.WaitTimeMs > 1000);
        }
    }
}