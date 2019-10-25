using System.Linq;
using System.Threading;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.Tests.SessionDetail.LockResource
{
    class GetLockResourcesBySpidQuery_when_execute_Key : DoubleConnection_TestBase
    {
        [Test]
        public void It_should_be_able_to_return_key_locks()
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction: transaction1).First();
            var spid2 = connection2.Query<int>("SELECT @@SPID", transaction: transaction2).First();

            connection1.Execute("USE Northwind", transaction: transaction1);
            connection2.Execute("USE Northwind", transaction: transaction2);

            connection1.ExecuteAsync(@"
                        UPDATE dbo.Customers
                        SET PostalCode = PostalCode
                        WHERE CustomerID = 'BLAUS'", transaction: transaction1);

            connection2.ExecuteAsync(@"
                        UPDATE dbo.Customers
                        SET PostalCode = PostalCode
                        WHERE CustomerID = 'BERGS'", transaction: transaction2);

            Thread.Sleep(600);

            connection2.ExecuteAsync(@"
                        UPDATE dbo.Customers
                        SET PostalCode = PostalCode
                        WHERE CustomerID = 'BLAUS'", transaction: transaction2);

            Thread.Sleep(600);

            var queryResult = new GetLockResourcesBySpidQuery(new TestConnectionContainer())
                .Execute(new[] {spid1, spid2}, "Northwind");

            queryResult.Result.Should().Contain(x =>
                x.IsKeyLock
                && x.Mode == "X"
                && x.Status == "GRANT"
                && x.SPID == spid1
                && x.FullObjectName == "dbo.Customers");
            queryResult.Result.Should().Contain(x =>
                x.IsKeyLock
                && x.Mode == "X"
                && x.Status == "GRANT"
                && x.SPID == spid1
                && x.FullObjectName == "dbo.Customers");
            queryResult.Result.Should().Contain(x =>
                x.IsKeyLock
                && x.Mode == "U"
                && x.Status == "WAIT"
                && x.SPID == spid2
                && x.FullObjectName == "dbo.Customers");
        }
    }
}