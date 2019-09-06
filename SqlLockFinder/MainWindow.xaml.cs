using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IConnectionContainer connectionContainer;
        private readonly IActivityMonitorQuery activityMonitorQuery;
        private readonly ISessionDrawer sessionDrawer;
        private List<SessionDto> sessions;
        private string databaseFilter;
        private string programNameFilter;
        private string DefaultFilter;

        public MainWindow(IConnectionContainer connectionContainer, IActivityMonitorQuery activityMonitorQuery, ISessionDrawer sessionDrawer)
        {
            this.connectionContainer = connectionContainer;
            this.activityMonitorQuery = activityMonitorQuery;
            this.sessionDrawer = sessionDrawer;
            this.DataContext = this;
            InitializeComponent();
            InitializeDefaults();
            InitializeCanvas();
        }

        public MainWindow()
        {
            this.connectionContainer = ConnectionContainer.Instance;
            this.activityMonitorQuery = new ActivityMonitorQuery(this.connectionContainer);
            this.DataContext = this;

            InitializeComponent();

            this.sessionDrawer = new SessionDrawer(new CanvasWrapper(SessionCanvas), SessionDetailControl);
            InitializeDefaults();
            InitializeCanvas();
        }

        private void InitializeCanvas()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (sender,e) => sessionDrawer.Move();
            timer.Start();
        }

        private string InitializeDefaults()
        {
            return ConnectionstringTextBox.Text =
                "Data Source=.;Initial Catalog=master;Integrated Security=SSPI;MultipleActiveResultSets=True;Application Name=SqlLockFinder;Connection Timeout=60;";
        }

        private void Clicked(object sender, RoutedEventArgs e)
        {
            connectionContainer.Create(ConnectionstringTextBox.Text);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (sender2, e2) => RetrieveSessions();
            timer.Start();
        }

        private void RetrieveSessions()
        {
            var queryResult = activityMonitorQuery.Execute();
            if (queryResult.HasValue)
            {
                Sessions = queryResult.Result;
            }
            else
            {
                sessionDrawer.Fault();
            }
        }

        private bool IsDefaultFilter(string filter)
        {
            return string.IsNullOrEmpty(filter) || filter == DefaultFilter;
        }

        public List<SessionDto> Sessions
        {
            get => sessions;
            private set
            {
                sessions = value;
                Databases = Sessions?.Select(x => x.DatabaseName)?.Distinct()?.ToList();
                DefaultFilter = "All";
                Databases?.Insert(0, DefaultFilter);

                ProgramNames = Sessions?.Select(x => x.ProgramName)?.Distinct()?.ToList();
                ProgramNames?.Insert(0, DefaultFilter);

                ProgramNameFilter ??= DefaultFilter;
                DatabaseFilter ??= DefaultFilter;

                sessionDrawer.Draw(SessionsFiltered);
                sessionDrawer.Move();
                OnPropertyChanged(nameof(Sessions), nameof(Databases), nameof(ProgramNames), nameof(SessionsFiltered), nameof(ProgramNameFilter), nameof(DatabaseFilter));
            }
        }

        public List<SessionDto> SessionsFiltered
        {
            get => sessions?.Where(x => 
                (IsDefaultFilter(DatabaseFilter) || x.DatabaseName == DatabaseFilter)
                && (IsDefaultFilter(ProgramNameFilter) || x.ProgramName == ProgramNameFilter)
            )?.ToList();
        }

        public string DatabaseFilter
        {
            get => databaseFilter;
            set
            {
                databaseFilter = value;
                sessionDrawer.Draw(SessionsFiltered);
                sessionDrawer.Move();
                OnPropertyChanged(nameof(DatabaseFilter), nameof(SessionsFiltered));
            }
        }
        public string ProgramNameFilter
        {
            get => programNameFilter;
            set
            {
                programNameFilter = value;
                sessionDrawer.Draw(SessionsFiltered);
                sessionDrawer.Move();
                OnPropertyChanged(nameof(ProgramNameFilter), nameof(SessionsFiltered));
            }
        }

        public List<string> Databases { get; set; }
        public List<string> ProgramNames { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}