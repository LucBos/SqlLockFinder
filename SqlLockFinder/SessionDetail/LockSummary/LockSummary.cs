using System.Collections.Generic;
using System.Linq;
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.SessionDetail.LockSummary
{
    public interface ILockSummary
    {
        IEnumerable<LockSummaryDto> ByKeyLock(IEnumerable<LockedResourceDto> lockedResources);
        IEnumerable<LockSummaryDto> ByRIDLock(IEnumerable<LockedResourceDto> lockedResources);
        IEnumerable<LockSummaryDto> ByPageLock(IEnumerable<LockedResourceDto> lockedResources);
    }

    public class LockSummary : ILockSummary
    {
        public IEnumerable<LockSummaryDto> ByKeyLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsKeyLock));
        }

        public IEnumerable<LockSummaryDto> ByRIDLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsRIDLock));
        }

        public IEnumerable<LockSummaryDto> ByPageLock(IEnumerable<LockedResourceDto> lockedResources)
        {
            if (lockedResources == null)
            {
                return new List<LockSummaryDto>();
            }

            return GetLockSummary(lockedResources.Where(x => x.IsPageLock));
        }

        private static IEnumerable<LockSummaryDto> GetLockSummary(IEnumerable<LockedResourceDto> lockedResources)
        {
            foreach (var lockedResourcesByObject in lockedResources.GroupBy(x => x.FullObjectName))
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