using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.SessionDetail.LockSummary
{
    public interface ILockSummary
    {
        IEnumerable<LockSummaryDto> ByKeyLock(IEnumerable<LockedResourceDto> lockedResources);
        IEnumerable<LockSummaryDto> ByRIDLock(IEnumerable<LockedResourceDto> lockedResources);
        IEnumerable<LockSummaryDto> ByPageLock(IEnumerable<LockedResourceDto> lockedResources);
        IEnumerable<LockSummaryDto> ByApplications(IEnumerable<LockedResourceDto> lockedResources);
    }

    public class LockSummary : ILockSummary
    {
        public IEnumerable<LockSummaryDto> ByKeyLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsKeyLock), x => x.FullObjectName);
        }

        public IEnumerable<LockSummaryDto> ByRIDLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsRIDLock), x => x.FullObjectName);
        }

        public IEnumerable<LockSummaryDto> ByPageLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsPageLock), x => x.FullObjectName);
        }
        public IEnumerable<LockSummaryDto> ByApplications(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsApplicationLock), x => x.Description);
        }

        private static IEnumerable<LockSummaryDto> GetLockSummary(IEnumerable<LockedResourceDto> lockedResources, Func<LockedResourceDto, string> grouper)
        {
            foreach (var lockedResourcesByObject in lockedResources.GroupBy(grouper))
            {
                foreach (var g in lockedResourcesByObject.GroupBy(x => x.Mode))
                {
                    yield return new LockSummaryDto
                        {FullObjectName = lockedResourcesByObject.Key, Count = g.Count(), Mode = g.Key};
                }
            }
        }
    }
}