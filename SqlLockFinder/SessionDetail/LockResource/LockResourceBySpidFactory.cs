using System.Collections.Generic;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockResource
{
    public interface ILockResourceBySpidFactory
    {
        ILockResourceBySpid Create(int spid, List<LockedResourceDto> lockedResourceDtos, SessionDto session);
    }

    public class LockResourceBySpidFactory : ILockResourceBySpidFactory
    {
        private readonly IConnectionContainer connectionContainer;
        private readonly INotifyUser notifyUser;

        public LockResourceBySpidFactory(IConnectionContainer connectionContainer, INotifyUser notifyUser)
        {
            this.connectionContainer = connectionContainer;
            this.notifyUser = notifyUser;
        }

        public ILockResourceBySpid Create(int spid, List<LockedResourceDto> lockedResourceDtos, SessionDto session)
        {
            return new LockResourceBySpid(spid, lockedResourceDtos, session, new GetRowOfLockedResourceQuery(connectionContainer), notifyUser);
        }
    }
}