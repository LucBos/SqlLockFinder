using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.Tests.SessionDetail.LockSummary
{
    class LockSummary_when_GetByRID : LockSummary_TestBase
    {
        [Test]
        public void It_should_return_the_key_locksummary_grouped_by_object_and_mode()
        {
            var lockedResources = new List<LockedResourceDto>
            {
                CreateLockResource("dbo", "table1", "X", "RID"),
                CreateLockResource("dbo", "table1", "X", "RID"),
                CreateLockResource("dbo", "table2", "IX", "RID"),
                CreateLockResource("dbo", "table2", "IX", "RID"),
                CreateLockResource("dbo", "table1", "U", "RID"),
                CreateLockResource("dbo", "table1", "X", "PAGE"),
            };

            var result = new SqlLockFinder.SessionDetail.LockSummary.LockSummary().ByRIDLock(lockedResources);

            result.Should().HaveCount(3);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "X" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table2" && x.Mode == "IX" && x.Count == 2);
            result.Should().Contain(x => x.FullObjectName == "dbo.table1" && x.Mode == "U" && x.Count == 1);
        }

    }
}