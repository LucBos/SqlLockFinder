using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;

namespace SqlLockFinder.Tests.SessionDetail.LockResource.GetRowOfLockedResourceQuery
{
    public class GetRowOfLockedResourceQuery_when_execute: SingleConnection_TestBase
    {
        private SqlLockFinder.SessionDetail.LockResource.GetRowOfLockedResourceQuery getRowOfLockedResourceQuery;

        [SetUp]
        public void Setup()
        {
            getRowOfLockedResourceQuery =
                new SqlLockFinder.SessionDetail.LockResource.GetRowOfLockedResourceQuery(new TestConnectionContainer());
        }

        [Test]
        public void It_should_return_the_exact_row_by_clustered_index()
        {
            connection1.Execute("USE Northwind", transaction: transaction1);
            var lockres = connection1.Query<string>(@"
SELECT %%lockres%%
FROM dbo.Customers
WHERE CustomerID = 'GODOS'", transaction:transaction1).First();
            var record = connection1.Query<dynamic>(@"
SELECT CustomerID
      ,CompanyName
      ,ContactName
      ,ContactTitle
      ,Address
      ,City
      ,Region
      ,PostalCode
      ,Country
      ,Phone
      ,Fax
FROM dbo.Customers
WHERE CustomerID = 'GODOS'", transaction: transaction1).First();

            var result = getRowOfLockedResourceQuery.Execute("Northwind", "dbo.Customers", lockres);

            Assert.AreEqual(result.Result.CustomerID, record.CustomerID);
            Assert.AreEqual(result.Result.CompanyName, record.CompanyName);
            Assert.AreEqual(result.Result.ContactName, record.ContactName);
            Assert.AreEqual(result.Result.ContactTitle, record.ContactTitle);
            Assert.AreEqual(result.Result.Address, record.Address);
            Assert.AreEqual(result.Result.City, record.City);
            Assert.AreEqual(result.Result.Region, record.Region);
            Assert.AreEqual(result.Result.PostalCode, record.PostalCode);
            Assert.AreEqual(result.Result.Country, record.Country);
            Assert.AreEqual(result.Result.Phone, record.Phone);
            Assert.AreEqual(result.Result.Fax, record.Fax);
        }

        [Test]
        public void It_should_return_the_exact_row_by_nonclustered_index()
        {
            connection1.Execute("USE Northwind", transaction: transaction1);
            var lockres = connection1.Query<string>(@"
SELECT %%lockres%%
FROM dbo.Customers WITH(Index=PostalCode)
WHERE CustomerID = 'GODOS'", transaction: transaction1).First();
            var record = connection1.Query<dynamic>(@"
SELECT CustomerID
      ,CompanyName
      ,ContactName
      ,ContactTitle
      ,Address
      ,City
      ,Region
      ,PostalCode
      ,Country
      ,Phone
      ,Fax
FROM dbo.Customers
WHERE CustomerID = 'GODOS'", transaction: transaction1).First();

            var result = getRowOfLockedResourceQuery.Execute("Northwind", "dbo.Customers", lockres);

            Assert.AreEqual(result.Result.CustomerID, record.CustomerID);
            Assert.AreEqual(result.Result.CompanyName, record.CompanyName);
            Assert.AreEqual(result.Result.ContactName, record.ContactName);
            Assert.AreEqual(result.Result.ContactTitle, record.ContactTitle);
            Assert.AreEqual(result.Result.Address, record.Address);
            Assert.AreEqual(result.Result.City, record.City);
            Assert.AreEqual(result.Result.Region, record.Region);
            Assert.AreEqual(result.Result.PostalCode, record.PostalCode);
            Assert.AreEqual(result.Result.Country, record.Country);
            Assert.AreEqual(result.Result.Phone, record.Phone);
            Assert.AreEqual(result.Result.Fax, record.Fax);
        }
    }
}