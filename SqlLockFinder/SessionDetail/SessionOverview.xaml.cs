using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionDetail
{
    /// <summary>
    /// Interaction logic for SessionOverview.xaml
    /// </summary>
    public partial class SessionOverview : UserControl, INotifyPropertyChanged
    {
        private SessionDto session;

        public SessionOverview()
        {
            DataContext = this;

            InitializeComponent();
        }

        public SessionOverview(SessionDto session) : this()
        {
            Session = session;
        }

        public SessionDto Session
        {
            get => session;
            set { session = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
