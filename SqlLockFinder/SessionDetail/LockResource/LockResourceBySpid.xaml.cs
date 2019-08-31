using System;
using System.Collections.Generic;
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
    public partial class LockResourceBySpid : UserControl, ILockResourceBySpid
    {
        private readonly IGetRowOfLockedResourceQuery getRowOfLockedResourceQuery;
        private readonly INotifyUser notifyUser;

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
            LockedResourceDtos = lockedResourceDtos;
            Session = session;
            DataContext = this;

            InitializeComponent();
        }

        public SessionDto Session { get; set; }
        public int LockingSPID { get; set; }
        public List<LockedResourceDto> LockedResourceDtos { get; set; }

        private void ShowLockedRow(object sender, RoutedEventArgs e)
        {
            if (LockedResourceGrid.SelectedItem is LockedResourceDto selectedItem)
            {
                var queryResult = getRowOfLockedResourceQuery.Execute(Session.DatabaseName, selectedItem.FullObjectName, selectedItem.Description);

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
            }
        }
    }
}