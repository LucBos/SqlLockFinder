using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        Task DisableCache();
        Task EnableCache();
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
        private Dictionary<int, QueryResult<List<LockSummaryDto>>> summaryCache = new Dictionary<int, QueryResult<List<LockSummaryDto>>>();
        private Dictionary<int, QueryResult<List<LockedResourceDto>>> detailsCache = new Dictionary<int, QueryResult<List<LockedResourceDto>>>();

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
            Context = Context.Instance;
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

        public async Task DisableCache()
        {
           summaryCache.Clear();
           detailsCache.Clear();
        }

        public async Task EnableCache()
        {
            foreach (var circle in SessionCircles)
            {
                var summary =
                    await getLockSummaryFromSpidQuery.Execute(circle.Session.SPID, circle.Session.DatabaseName);
                summaryCache.Add(circle.Session.SPID, summary);

                if (Context.AutoRetrieveDetailedLocks)
                {
                    var details = await GetLockResourcesBySpid(SessionCircles.GetLockedWith(circle), circle.Session);
                    detailsCache.Add(circle.Session.SPID, details);
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

        public bool HasLockedSummaryTables => LockedSummaryTables?.Any() ?? false;
        public bool HasLockedSummaryRows => LockedSummaryRows?.Any() ?? false;

        public bool HasLockedSummaryRIDs => LockedSummaryRIDs?.Any() ?? false;

        public bool HasLockedSummaryPages => LockedSummaryPages?.Any() ?? false;

        public bool HasLockedSummaryApplications => LockedSummaryApplications?.Any() ?? false;

        public bool TooManyResourcesLocked => !LoadingDetailedLockResources && lockedResourceDtos != null && lockedResourceDtos.Count > 5000;

        public SessionDto Session => SessionCircle?.Session;

        public IEnumerable<SessionDto> LockedWith => sessionCircles?.GetLockedWith(sessionCircle);

        public ISessionCircle LockCause => sessionCircles?.GetLockCause(sessionCircle);

        public bool ItemWasSelected => SessionCircle != null;
        public Context Context { get; set; }

        private async void RetrieveLockSummary()
        {
            if (Session == null || LockedWith == null || LoadingDetailedLockResources) return;

            UI(() => LoadingLockSummary = true);

            QueryResult<List<LockSummaryDto>> queryResult;
            if (summaryCache.ContainsKey(sessionCircle.Session.SPID))
            {
                queryResult = summaryCache[sessionCircle.Session.SPID];
            }
            else
            {
                queryResult = await getLockSummaryFromSpidQuery.Execute(sessionCircle.Session.SPID, Session.DatabaseName);
            }

            if (Context.AutoRetrieveDetailedLocks)
            {
                await RetrieveDetailedLocks();
            }

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

        private async void RetrieveDetailedLocks(object sender, RoutedEventArgs routedEventArgs) => await RetrieveDetailedLocks();
        private async Task RetrieveDetailedLocks()
        {
            if (Session == null || LockedWith == null || LoadingDetailedLockResources) return;

            UI(() => LoadingDetailedLockResources = true);

            QueryResult<List<LockedResourceDto>> queryResult;
            if (detailsCache.ContainsKey(Session.SPID))
            {
                queryResult = detailsCache[Session.SPID];
            }
            else
            {
                queryResult = await GetLockResourcesBySpid(LockedWith, Session);
            }

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

        private async Task<QueryResult<List<LockedResourceDto>>> GetLockResourcesBySpid(IEnumerable<SessionDto> lockedWith, SessionDto session)
        {
            var spids = lockedWith
                .Select(x => x.SPID)
                .Union(new[] { session.SPID })
                .ToArray();
            var queryResult = await getLockResourcesBySpidQuery.Execute(spids, session.DatabaseName);
            return queryResult;
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