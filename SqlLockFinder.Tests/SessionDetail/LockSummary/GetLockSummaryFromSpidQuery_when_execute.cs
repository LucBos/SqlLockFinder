using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.LockSummary;

namespace SqlLockFinder.Tests.SessionDetail.LockSummary
{
    class GetLockSummaryFromSpidQuery_when_execute: DoubleConnection_TestBase
    {
        private int SetupDataWithLock(string lockType)
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction: transaction1).First();
            var spid2 = connection2.Query<int>("SELECT @@SPID", transaction: transaction2).First();

            connection1.Execute("USE Northwind", transaction: transaction1);
            connection2.Execute("USE Northwind", transaction: transaction2);

            connection1.Execute($@"
                        UPDATE dbo.Shippers WITH ({lockType})
                        SET CompanyName =  CompanyName + '2'", transaction: transaction1);

            connection1.Execute($@"
                        UPDATE dbo.[Order Details] WITH ({lockType})
                        SET UnitPrice = UnitPrice + 1", transaction: transaction1);

            connection2.Execute($@"
                        UPDATE dbo.Region WITH ({lockType})
                        SET RegionDescription = substring(RegionDescription,0,1)", transaction: transaction2);
            return spid1;
        }

        [Test]
        public async Task It_should_return_a_grouped_summary_of_page_locks()
        {
            var spid1 = SetupDataWithLock("PAGLOCK");

            var queryResult = await new GetLockSummaryFromSpidQuery(new TestConnectionContainer()).Execute(spid1, "Northwind");

            queryResult.Faulted.Should().BeFalse();
            queryResult.Result.Should()
                .Contain(x => x.IsPageLock && x.Count == 9 && x.FullObjectName == "dbo.Order Details" && x.Mode == "X");
            queryResult.Result.Should()
                .Contain(x => x.IsPageLock && x.Count == 1 && x.FullObjectName == "dbo.Shippers" && x.Mode == "X");
        }

        [Test]
        public async Task It_should_return_a_grouped_summary_of_table_locks()
        {
            var spid1 = SetupDataWithLock("TABLOCK");

            var queryResult = await new GetLockSummaryFromSpidQuery(new TestConnectionContainer()).Execute(spid1, "Northwind");

            queryResult.Faulted.Should().BeFalse();
            queryResult.Result.Should()
                .Contain(x => x.IsTableLock && x.Count == 1 && x.FullObjectName == "dbo.Order Details" && x.Mode == "X");
            queryResult.Result.Should()
                .Contain(x => x.IsTableLock && x.Count == 1 && x.FullObjectName == "dbo.Shippers" && x.Mode == "X");
        }

        [Test]
        public async Task It_should_return_a_grouped_summary_of_key_locks()
        {
            var spid1 = SetupDataWithLock("ROWLOCK");

            var queryResult = await new GetLockSummaryFromSpidQuery(new TestConnectionContainer()).Execute(spid1, "Northwind");

            queryResult.Faulted.Should().BeFalse();
            queryResult.Result.Should()
                .Contain(x => x.IsKeyLock && x.Count == 2155 && x.FullObjectName == "dbo.Order Details" && x.Mode == "X");
            queryResult.Result.Should()
                .Contain(x => x.IsKeyLock && x.Count == 3 && x.FullObjectName == "dbo.Shippers" && x.Mode == "X");
        }

        [Test]
        public async Task It_should_return_a_grouped_summary_of_rid_locks()
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction: transaction1).First();

            connection1.Execute("USE Northwind", transaction: transaction1);

            connection1.Execute(@"
                        UPDATE dbo.Territories
                        SET TerritoryDescription = TerritoryDescription
                        WHERE TerritoryDescription = 'Philadelphia'
                        ", transaction: transaction1);

            var queryResult = await new GetLockSummaryFromSpidQuery(new TestConnectionContainer()).Execute(spid1, "Northwind");

            queryResult.Faulted.Should().BeFalse();
            queryResult.Result.Should()
                .Contain(x => x.IsRIDLock && x.Count == 1 && x.FullObjectName == "dbo.Territories");
        }
    }
}
