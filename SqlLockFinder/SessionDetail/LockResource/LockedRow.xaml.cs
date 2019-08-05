using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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