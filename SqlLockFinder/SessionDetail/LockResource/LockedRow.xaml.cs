using System.Windows.Controls;

namespace SqlLockFinder.SessionDetail.LockResource
{
    /// <summary>
    /// Interaction logic for LockedRow.xaml
    /// </summary>
    public partial class LockedRow : UserControl
    {
        public LockedRow(dynamic row)
        {
            Row = row;
            DataContext = this;
            InitializeComponent();
        }

        public dynamic Row { get; set; }
    }
}