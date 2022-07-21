using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail.LockResource
{
    public interface ILockResourceBySpid
    {
        SessionDto Session { get; set; }
        int LockingSPID { get; set; }
        List<LockedResourceDto> LockedResourceDtos { get; set; }
    }

    /// <summary>
    /// Interaction logic for LockResourceBySpid.xaml
    /// </summary>
    public partial class LockResourceBySpid : UserControl, ILockResourceBySpid, INotifyPropertyChanged
    {
        private readonly IGetRowOfLockedResourceQuery getRowOfLockedResourceQuery;
        private readonly INotifyUser notifyUser;
        private bool allowQuery;
        private const int MaxResults = 100;

        public LockResourceBySpid(
            int lockingSpid,
            List<LockedResourceDto> lockedResourceDtos,
            SessionDto session,
            IGetRowOfLockedResourceQuery getRowOfLockedResourceQuery,
            INotifyUser notifyUser)
        {
            this.getRowOfLockedResourceQuery = getRowOfLockedResourceQuery;
            this.notifyUser = notifyUser;
            LockingSPID = lockingSpid;
            TooManyResult = lockedResourceDtos.Count > MaxResults;
            LockedResourceDtos = lockedResourceDtos.Take(MaxResults).ToList();
            Session = session;
            DataContext = this;
            AllowQuery = true;

            InitializeComponent();
        }

        public bool TooManyResult { get; set; }
        public SessionDto Session { get; set; }
        public int LockingSPID { get; set; }
        public List<LockedResourceDto> LockedResourceDtos { get; set; }

        public bool AllowQuery
        {
            get => allowQuery;
            set
            {
                allowQuery = value;
                OnPropertyChanged();
            }
        }

        private async void ShowLockedRow(object sender, RoutedEventArgs e)
        {
            if (!(LockedResourceGrid.SelectedItem is LockedResourceDto selectedItem && AllowQuery)) return;

            ShowLockedRow(selectedItem);
        }

        private async Task ShowLockedRow(LockedResourceDto selectedItem)
        {
            UI(() => AllowQuery = false);

            var queryResult = await getRowOfLockedResourceQuery.Execute(Session.DatabaseName, selectedItem.FullObjectName, selectedItem.IndexName,
                selectedItem.Description);

            UI(() =>
            {
                AllowQuery = true;
                if (queryResult.HasValue)
                {
                    var window = new Window();
                    window.Content = new LockedRow(queryResult.Result);
                    window.Show();
                }
                else
                {
                    notifyUser.Notify(queryResult);
                }
            });
        }

        private void UI(Action action)
        {
            Dispatcher.Invoke(action);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}