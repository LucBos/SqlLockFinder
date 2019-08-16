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
using SqlLockFinder.SessionDetail.LockResource;

namespace SqlLockFinder.SessionDetail
{
    public interface ISessionDetail
    {
        SessionCircle SessionCircle { get; set; }
        IEnumerable<SessionDto> LockedWith { get; set; }
    }

    /// <summary>
    /// Interaction logic for SessionDetail.xaml
    /// </summary>
    public partial class SessionDetail : UserControl, ISessionDetail, INotifyPropertyChanged
    {
        private readonly IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery;
        private readonly ILockResourceBySpidFactory lockResourceBySpidFactory;
        private readonly INotifyUser notifyUser;
        private SessionCircle sessionCircle;
        private List<LockedResourceDto> lockedResourceDtos;

        public SessionDetail() : this(
            new GetLockResourcesBySpidQuery(ConnectionContainer.Instance),
            new LockResourceBySpidFactory(ConnectionContainer.Instance, new NotifyUser()),
            new NotifyUser())
        {
        }

        public SessionDetail(
            IGetLockResourcesBySpidQuery getLockResourcesBySpidQuery,
            ILockResourceBySpidFactory lockResourceBySpidFactory,
            INotifyUser notifyUser)
        {
            this.getLockResourcesBySpidQuery = getLockResourcesBySpidQuery;
            this.lockResourceBySpidFactory = lockResourceBySpidFactory;
            this.notifyUser = notifyUser;
            this.DataContext = this;
            InitializeComponent();
        }

        public SessionCircle SessionCircle
        {
            get => sessionCircle;
            set
            {
                sessionCircle = value;
                SessionOVerviewControl.Session = sessionCircle.Session;
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

        public List<string> LockedSummaryRows => LockedResourceDtos
                                                     ?.Where(x => x.IsKeyLock)
                                                     ?.GroupBy(x => x.FullObjectName)
                                                     ?.Select(x => $"{x.Key}: {x.Count()} rows ({MakeSummaryMode(x)})")
                                                     ?.ToList()
                                                 ?? new List<string>();

        public List<string> LockedSummaryPages => LockedResourceDtos
                                                      ?.Where(x => x.IsPageLock)
                                                      ?.GroupBy(x => x.FullObjectName)
                                                      ?.Select(x => $"{x.Key}: {x.Count()} pages ({MakeSummaryMode(x)})")
                                                      ?.ToList()
                                                  ?? new List<string>();

        public SessionDto Session => SessionCircle?.Session;

        public IEnumerable<SessionDto> LockedWith { get; set; }

        private void RetrieveLocks()
        {
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

        private string MakeSummaryMode(IEnumerable<LockedResourceDto> x)
        {
            return x.GroupBy(e => e.Mode).Aggregate(string.Empty, (a, g) => $"{a}{g.Key}: {g.Count()}");
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

        public bool ItemWasSelected => SessionCircle != null;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}