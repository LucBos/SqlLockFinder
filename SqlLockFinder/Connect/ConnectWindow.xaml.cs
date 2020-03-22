using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.Connect
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window, INotifyPropertyChanged
    {
        private const string WindowsAuthentication = "Windows Authentication";
        private const string SqlServerAuthentication = "SQL Server Authentication";
        private readonly IConnectionContainer connectionContainer;
        private ConnectBy connectBy;
        private string fileName;
        private string connectionstring;
        private OpenFileDialog openFileDialog;
        private bool enableCustomAuthentication;
        private string selectedAuthentication;
        private string dataSource;
        private string username;
        private string password;

        public ConnectWindow()
            : this(ConnectionContainer.Instance)
        {
        }

        public ConnectWindow(IConnectionContainer connectionContainer)
        {
            SelectedAuthentication = WindowsAuthentication;
            this.connectionContainer = connectionContainer;
            ConnectBy = ConnectBy.Properties;
            this.DataContext = this;
            InitializeComponent();
            DataSourceTextBox.Focus();
        }

        public ConnectBy ConnectBy
        {
            get => connectBy;
            set
            {
                connectBy = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public string Connectionstring
        {
            get => connectionstring;
            set
            {
                connectionstring = value;
                OnPropertyChanged();
            }
        }

        public string DataSource
        {
            get => dataSource;
            set
            {
                dataSource = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }

        public string[] AuthenticationModes => new[] {WindowsAuthentication, SqlServerAuthentication};

        public string SelectedAuthentication
        {
            get => selectedAuthentication;
            set
            {
                selectedAuthentication = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableCustomAuthentication));
            }
        }

        public bool EnableCustomAuthentication => SelectedAuthentication == SqlServerAuthentication;

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenUDL(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UDL file (*.udl)|*.udl";
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.FileName;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (ConnectBy)
                {
                    case ConnectBy.Properties:
                        var security = SelectedAuthentication == WindowsAuthentication
                            ? "Integrated Security=SSPI"
                            : $"User=\"{Username}\";Password=\"{PasswordBox.Password}\"";
                        connectionContainer.Create($"Data Source={DataSource};{security};Application Name=SqlLockFinder;Connection Timeout=15;MultipleActiveResultSets=true;");
                        break;
                    case ConnectBy.Connectionstring:
                        connectionContainer.Create(Connectionstring);
                        break;
                    case ConnectBy.UDL:
                        if (string.IsNullOrEmpty(openFileDialog.FileName))
                        {
                            MessageBox.Show("Please select a valid udl file");
                            return;
                        }

                        connectionContainer.Create(new UdlParser().ParseConnectionString(openFileDialog.OpenFile()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Could not connect to the database.");
            }
        }

        private void LoginOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Connect(null, null);

        }
    }
}