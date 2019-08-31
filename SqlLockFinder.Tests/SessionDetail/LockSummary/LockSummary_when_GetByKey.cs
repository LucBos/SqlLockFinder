using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.Tests.SessionDetail.LockSummary
{
    class LockSummary_when_GetByKey: LockSummary_TestBase
    {
        [Test]
        public void It_should_return_the_key_locksummary_grouped_by_object_and_mode()
        {
            var lockedResources = new List<LockedResourceDto>
            {
                CreateLockResource("dbo", "table1", "X", "KEY"),
                CreateLockResource("dbo", "table1", "X", "KEY"),
                CreateLockResource("dbo", "table2", "IX", "KEY"),
                CreateLockResource("dbo", "table2", "IX", "KEY"),
                CreateLockResource("dbo", "table1", "U", "KEY"),
                CreateLockResource("dbo", "table1", "X", "PAGE"),
            };

            var result = new SqlLockFinder.SessionDetail.LockSummary.LockSummary().ByKeyLock(lockedResources);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "X" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table2" && x.Mode == "IX" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "U" && x.Count == 1);
        }

    }
}
