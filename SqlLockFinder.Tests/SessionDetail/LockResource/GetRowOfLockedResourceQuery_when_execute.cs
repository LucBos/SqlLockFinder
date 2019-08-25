using NUnit.Framework;

namespace SqlLockFinder.Tests.SessionDetail.LockResource.GetRowOfLockedResourceQuery
{
    public class GetRowOfLockedResourceQuery_when_execute: SingleConnection_TestBase
    {
        [Test]
        public void It_should_return_the_exact_row_by_clustered_index()
        {
            Assert.Fail();
        }

        [Test]
        public void It_should_return_the_exact_row_by_nonclustered_index()
        {
            Assert.Fail();
        }
    }
}