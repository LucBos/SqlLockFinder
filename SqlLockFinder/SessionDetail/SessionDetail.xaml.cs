using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;
using SqlLockFinder.SessionCanvas;
using SqlLockFinder.SessionDetail.Kill;
using SqlLockFinder.SessionDetail.LockResource;
using SqlLockFinder.SessionDetail.LockSummary;

namespace SqlLockFinder.SessionDetail
{
    public interface ISessionDetail
    {
        ISessionCircle SessionCircle { get; set; }
        IEnumerable<SessionDto> LockedWith { get; set; }
    }

    public partial class SessionDetail : UserControl, ISessionDetail, INotifyPropertyChanged
    {
        private readonly IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery;
        private readonly ILockResourceBySpidFactory lockResourceBySpidFactory;
        private readonly IKillSessionQuery killSessionQuery;
        private readonly INotifyUser notifyUser;
        private readonly ILockSummary lockSummary;
        private ISessionCircle sessionCircle;
        private List<LockedResourceDto> lockedResourceDtos;

        public SessionDetail() : this(
            new GetLockResourcesBySpidQuery(ConnectionContainer.Instance),
            new LockResourceBySpidFactory(ConnectionContainer.Instance, new NotifyUser()),
            new KillSessionQuery(ConnectionContainer.Instance),
            new NotifyUser(),
            new LockSummary.LockSummary())
        {
        }

        public SessionDetail(
            IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery,
            ILockResourceBySpidFactory lockResourceBySpidFactory,
            IKillSessionQuery killSessionQuery,
            INotifyUser notifyUser,
            ILockSummary lockSummary)
        {
            this.getLockResourcesBySpidQuery = getLockResourcesBySpidQuery;
            this.lockResourceBySpidFactory = lockResourceBySpidFactory;
            this.killSessionQuery = killSessionQuery;
            this.notifyUser = notifyUser;
            this.lockSummary = lockSummary;
            this.DataContext = this;
            InitializeComponent();
        }

        public ISessionCircle SessionCircle
        {
            get => sessionCircle;
            set
            {
                sessionCircle = value;
                SessionOVerviewControl.Session = sessionCircle?.Session;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemWasSelected));
                OnPropertyChanged(nameof(Session));
                RetrieveLocks();
            }
        }

        public List<LockedResourceDto> LockedResourceDtos
        {
            get => lockedResourceDtos;
            set
            {
                lockedResourceDtos = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LockedSummaryRows));
                OnPropertyChanged(nameof(LockedSummaryPages));
            }
        }

        public IEnumerable<LockSummaryDto> LockedSummaryRows => lockSummary.ByKeyLock(lockedResourceDtos);

        public IEnumerable<LockSummaryDto> LockedSummaryPages => lockSummary.ByPageLock(lockedResourceDtos);

        public SessionDto Session => SessionCircle?.Session;

        public IEnumerable<SessionDto> LockedWith { get; set; }

        public bool ItemWasSelected => SessionCircle != null;

        private void RetrieveLocks()
        {
            if (Session == null) return;

            var spids = LockedWith
                .Select(x => x.SPID)
                .Union(new[] {Session.SPID})
                .ToArray();
            var queryResult = getLockResourcesBySpidQuery.Execute(spids, Session.DatabaseName);
            if (queryResult.HasValue)
            {
                CreateLockResourcesBySPID(queryResult.Result);
            }
            else if (queryResult.Faulted)
            {
                notifyUser.Notify(queryResult);
            }
        }

        private void CreateLockResourcesBySPID(List<LockedResourceDto> lockedResources)
        {
            BlockedResourcesBySpidStackPanel.Children.Clear();
            LockedResourceDtos = lockedResources.Where(x => x.SPID == Session.SPID).ToList();
            var otherSessionsGrouped = lockedResources.Where(x => x.SPID != Session.SPID).GroupBy(x => x.SPID);
            foreach (var otherResources in otherSessionsGrouped)
            {
                var bothLockedResources =
                    otherResources.Where(x => LockedResourceDtos.Any(y => y.SameLockAs(x))).ToList();
                var lockedResourceBySpid =
                    lockResourceBySpidFactory.Create(otherResources.Key, bothLockedResources, Session);
                BlockedResourcesBySpidStackPanel.Children.Add(lockedResourceBySpid as UIElement);
            }
        }

        private void KillSession(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show(
                    $"Are you certain you want to kill session with spid {Session?.SPID}?", 
                    "Kill session?",
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var queryResult = killSessionQuery.Execute(Session.SPID);
            if (queryResult.Faulted)
            {
                notifyUser.Notify(queryResult);
            }
            else
            {
                SessionCircle = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}