using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.Tests.SessionDetail.LockSummary
{
    class LockSummary_when_GetByPage : LockSummary_TestBase
    {
        [Test]
        public void It_should_return_the_page_locksummary_grouped_by_object_and_mode()
        {
            var lockedResources = new List<LockedResourceDto>
            {
                CreateLockResource("dbo", "table1", "X", "PAGE"),
                CreateLockResource("dbo", "table1", "X", "PAGE"),
                CreateLockResource("dbo", "table2", "IX", "PAGE"),
                CreateLockResource("dbo", "table2", "IX", "PAGE"),
                CreateLockResource("dbo", "table1", "U", "PAGE"),
                CreateLockResource("dbo", "table1", "X", "KEY"),
            };

            var result = new SqlLockFinder.SessionDetail.LockSummary().ByPageLock(lockedResources);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "X" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table2" && x.Mode == "IX" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "U" && x.Count == 1);
        }
    }
}