using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlLockFinder.Infrastructure
{
    public class Context : INotifyPropertyChanged
    {
        private static Context instance;
        private bool autoRetrieveDetailedLocks;

        protected Context()
        {
        }

        public static Context Instance => instance ??= new Context();

        public bool AutoRetrieveDetailedLocks
        {
            get => autoRetrieveDetailedLocks;
            set
            {
                autoRetrieveDetailedLocks = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}