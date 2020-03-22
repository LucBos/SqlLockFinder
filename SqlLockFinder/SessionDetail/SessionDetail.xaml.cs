using System;
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
        ISessionCircleList SessionCircles { get; set; }
    }

    public partial class SessionDetail : UserControl, ISessionDetail, INotifyPropertyChanged
    {
        private readonly IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery;
        private readonly IGetLockSummaryFromSpidQuery getLockSummaryFromSpidQuery;
        private readonly ILockResourceBySpidFactory lockResourceBySpidFactory;
        private readonly IKillSessionQuery killSessionQuery;
        private readonly INotifyUser notifyUser;
        private ISessionCircle sessionCircle;
        private List<LockedResourceDto> lockedResourceDtos;
        private bool loadingDetailedLockResources;
        private bool loadingDLockSummary;
        private ISessionCircleList sessionCircles;
        private ISessionCircle lockCause;

        public SessionDetail() : this(
            new GetLockResourcesBySpidQuery(ConnectionContainer.Instance),
            new GetLockSummaryFromSpidQuery(ConnectionContainer.Instance),
            new LockResourceBySpidFactory(ConnectionContainer.Instance, new NotifyUser()),
            new KillSessionQuery(ConnectionContainer.Instance),
            new NotifyUser())
        {
        }

        public SessionDetail(
            IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery,
            IGetLockSummaryFromSpidQuery getLockSummaryFromSpidQuery,
            ILockResourceBySpidFactory lockResourceBySpidFactory,
            IKillSessionQuery killSessionQuery,
            INotifyUser notifyUser)
        {
            this.getLockResourcesBySpidQuery = getLockResourcesBySpidQuery;
            this.getLockSummaryFromSpidQuery = getLockSummaryFromSpidQuery;
            this.lockResourceBySpidFactory = lockResourceBySpidFactory;
            this.killSessionQuery = killSessionQuery;
            this.notifyUser = notifyUser;
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
                BlockedResourcesBySpidStackPanel.Children.Clear();
                lockedResourceDtos?.Clear();

                LockedSummaryApplications = new List<LockSummaryDto>();
                LockedSummaryTables = new List<LockSummaryDto>();
                LockedSummaryRows = new List<LockSummaryDto>();
                LockedSummaryRIDs = new List<LockSummaryDto>();
                LockedSummaryPages = new List<LockSummaryDto>();

                OnPropertyChanged(nameof(LockedSummaryApplications));
                OnPropertyChanged(nameof(LockedSummaryTables));
                OnPropertyChanged(nameof(LockedSummaryRows));
                OnPropertyChanged(nameof(LockedSummaryPages));
                OnPropertyChanged(nameof(LockedSummaryRIDs));

                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemWasSelected));
                OnPropertyChanged(nameof(Session));
                OnPropertyChanged(nameof(LockCause));
                OnPropertyChanged(nameof(LockedWith));
                OnPropertyChanged(nameof(TooManyResourcesLocked));
                RetrieveLockSummary();
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

        public bool LoadingDetailedLockResources
        {
            get => loadingDetailedLockResources;
            set
            {
                loadingDetailedLockResources = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TooManyResourcesLocked));
            }
        }

        public bool LoadingLockSummary
        {
            get => loadingDLockSummary;
            set
            {
                loadingDLockSummary = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LockSummaryDto> LockedSummaryRows { get; set; }
        public IEnumerable<LockSummaryDto> LockedSummaryTables { get; set; }

        public IEnumerable<LockSummaryDto> LockedSummaryRIDs { get; set; }

        public IEnumerable<LockSummaryDto> LockedSummaryPages { get; set; }

        public IEnumerable<LockSummaryDto> LockedSummaryApplications { get; set; }

        public bool HasLockedSummaryTables => LockedSummaryTables.Any();
        public bool HasLockedSummaryRows => LockedSummaryRows.Any();

        public bool HasLockedSummaryRIDs => LockedSummaryRIDs.Any();

        public bool HasLockedSummaryPages => LockedSummaryPages.Any();

        public bool HasLockedSummaryApplications => LockedSummaryApplications.Any();

        public bool TooManyResourcesLocked => !LoadingDetailedLockResources && lockedResourceDtos != null && lockedResourceDtos.Count > 5000;

        public SessionDto Session => SessionCircle?.Session;

        public IEnumerable<SessionDto> LockedWith => sessionCircles?.GetLockedWith(sessionCircle);

        public ISessionCircle LockCause => sessionCircles?.GetLockCause(sessionCircle);

        public bool ItemWasSelected => SessionCircle != null;

        private async void RetrieveLockSummary()
        {
            if (Session == null || LockedWith == null || LoadingDetailedLockResources) return;

            UI(() => LoadingLockSummary = true);

            var queryResult =
                await getLockSummaryFromSpidQuery.Execute(sessionCircle.Session.SPID, Session.DatabaseName);

            UI(() =>
            {
                LoadingLockSummary = false;
                if (queryResult.HasValue)
                {
                    LockedSummaryApplications = queryResult.Result.Where(x => x.IsApplicationLock).ToList();
                    LockedSummaryPages = queryResult.Result.Where(x => x.IsPageLock).ToList();
                    LockedSummaryRIDs = queryResult.Result.Where(x => x.IsRIDLock).ToList();
                    LockedSummaryRows = queryResult.Result.Where(x => x.IsKeyLock).ToList();
                    LockedSummaryTables = queryResult.Result.Where(x => x.IsTableLock).ToList();

                    OnPropertyChanged(nameof(LockedSummaryApplications));
                    OnPropertyChanged(nameof(LockedSummaryPages));
                    OnPropertyChanged(nameof(LockedSummaryRIDs));
                    OnPropertyChanged(nameof(LockedSummaryRows));
                    OnPropertyChanged(nameof(LockedSummaryTables));
                    OnPropertyChanged(nameof(HasLockedSummaryPages));
                    OnPropertyChanged(nameof(HasLockedSummaryApplications));
                    OnPropertyChanged(nameof(HasLockedSummaryRIDs));
                    OnPropertyChanged(nameof(HasLockedSummaryRows));
                    OnPropertyChanged(nameof(HasLockedSummaryTables));
                }
                else if (queryResult.Faulted)
                {
                    notifyUser.Notify(queryResult);
                }
            });
        }

        private async void RetrieveDetailedLocks(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Session == null || LockedWith == null || LoadingDetailedLockResources) return;

            UI(() => LoadingDetailedLockResources = true);

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
                LoadingDetailedLockResources = false;
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

        private void FocusLockCause(object sender, RoutedEventArgs routedEventArgs)
        {
            if (LockCause != null)
            {
                SessionCircle.Selected = false;
                LockCause.Selected = true;
                SessionCircle = LockCause;
            }
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