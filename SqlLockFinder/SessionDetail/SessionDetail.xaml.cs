using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
        ISessionCircleList SessionCircles { get; set; }
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
        private bool loadingLockResources;
        private ISessionCircleList sessionCircles;
        private ISessionCircle lockCause;

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
                OnPropertyChanged(nameof(LockCause));
                OnPropertyChanged(nameof(LockedWith));
                RetrieveLocks();
            }
        }


        public ISessionCircleList SessionCircles
        {
            get => sessionCircles;
            set
            {
                sessionCircles = value;
                if (sessionCircle != null)
                {
                    OnPropertyChanged(nameof(LockCause));
                    OnPropertyChanged(nameof(LockedWith));
                }
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
                OnPropertyChanged(nameof(LockedSummaryRIDs));
                OnPropertyChanged(nameof(LockedSummaryPages));
                OnPropertyChanged(nameof(LockedSummaryApplications));
                OnPropertyChanged(nameof(HasLockedSummaryRows));
                OnPropertyChanged(nameof(HasLockedSummaryRIDs));
                OnPropertyChanged(nameof(HasLockedSummaryPages));
                OnPropertyChanged(nameof(HasLockedSummaryApplications));
                OnPropertyChanged(nameof(TooManyResourcesLocked));
            }
        }

        public bool LoadingLockResources
        {
            get => loadingLockResources;
            set
            {
                loadingLockResources = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TooManyResourcesLocked));
            }
        }

        public IEnumerable<LockSummaryDto> LockedSummaryRows => lockSummary.ByKeyLock(lockedResourceDtos);

        public IEnumerable<LockSummaryDto> LockedSummaryRIDs => lockSummary.ByRIDLock(lockedResourceDtos);

        public IEnumerable<LockSummaryDto> LockedSummaryPages => lockSummary.ByPageLock(lockedResourceDtos);
        public IEnumerable<LockSummaryDto> LockedSummaryApplications => lockSummary.ByApplications(lockedResourceDtos);

        public bool HasLockedSummaryRows => LockedSummaryRows.Any();

        public bool HasLockedSummaryRIDs => LockedSummaryRIDs.Any();

        public bool HasLockedSummaryPages => LockedSummaryPages.Any();
        public bool HasLockedSummaryApplications => LockedSummaryApplications.Any();
        public bool TooManyResourcesLocked => !LoadingLockResources && lockedResourceDtos.Count > 5000;

        public SessionDto Session => SessionCircle?.Session;

        public IEnumerable<SessionDto> LockedWith => sessionCircles?.GetLockedWith(sessionCircle);

        public ISessionCircle LockCause => sessionCircles?.GetLockCause(sessionCircle);

        public bool ItemWasSelected => SessionCircle != null;

        private async void RetrieveLocks()
        {
            if (Session == null || LockedWith == null || LoadingLockResources) return;

            UI(() => LoadingLockResources = true);

            var spids = LockedWith
                .Select(x => x.SPID)
                .Union(new[] {Session.SPID})
                .ToArray();
            var queryResult = await getLockResourcesBySpidQuery.Execute(spids, Session.DatabaseName);

            Dictionary<int, List<LockedResourceDto>> lockedPerSpid = null;
            if (queryResult.HasValue)
            {
                lockedPerSpid = CreateLockResourcesBySPID(queryResult.Result);
            }

            UI(() =>
            {
                LoadingLockResources = false;
                if (queryResult.HasValue && lockedPerSpid != null)
                {
                    BlockedResourcesBySpidStackPanel.Children.Clear();

                    foreach (var spidLocks in lockedPerSpid)
                    {
                        var lockedResourceBySpid =
                            lockResourceBySpidFactory.Create(spidLocks.Key, spidLocks.Value, Session);
                        BlockedResourcesBySpidStackPanel.Children.Add(lockedResourceBySpid as UIElement);
                    }
                }
                else if (queryResult.Faulted)
                {
                    notifyUser.Notify(queryResult);
                }
            });
        }

        private Dictionary<int, List<LockedResourceDto>> CreateLockResourcesBySPID(
            List<LockedResourceDto> lockedResources)
        {
            var result = new Dictionary<int, List<LockedResourceDto>>();
            LockedResourceDtos = lockedResources.Where(x => x.SPID == Session.SPID).ToList();
            if (lockedResourceDtos.Count > 1000) return result;
            var otherSessionsGrouped = lockedResources.Where(x => x.SPID != Session.SPID).GroupBy(x => x.SPID);
            foreach (var otherResources in otherSessionsGrouped)
            {
                var bothLockedResources =
                    otherResources.Where(x => LockedResourceDtos.Any(y => y.SameLockAs(x))).ToList();
                result.Add(otherResources.Key, bothLockedResources);
            }

            return result;
        }

        public async void Kill()
        {
            if (Session == null) return;

            if (Session.IsUserProcess
                && MessageBox.Show(
                    $"Are you certain you want to kill session with spid {Session?.SPID}?",
                    "Kill session?",
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            if (!Session.IsUserProcess
                && MessageBox.Show(
                    $"Are you certain you want to kill session with spid {Session?.SPID}?\nKilling a system session can have unforeseen consequences!",
                    "Kill session?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation) == MessageBoxResult.No)
            {
                return;
            }

            var queryResult = await killSessionQuery.Execute(Session.SPID);
            UI(() =>
            {
                if (queryResult.Faulted)
                {
                    notifyUser.Notify(queryResult);
                }
                else
                {
                    SessionCircle = null;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UI(Action action)
        {
            Dispatcher.Invoke(action);
        }
    }
}