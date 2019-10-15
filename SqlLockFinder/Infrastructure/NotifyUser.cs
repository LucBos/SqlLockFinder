using System;
using System.Linq;
using System.Windows;

namespace SqlLockFinder.Infrastructure
{
    public interface INotifyUser
    {
        void Notify(QueryResult queryResult);
    }

    public class NotifyUser : INotifyUser
    {
        public void Notify(QueryResult queryResult)
        {
            if (queryResult.Errors.Any())
            {
                MessageBox.Show(String.Join("\n",queryResult.Errors), "", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else if (queryResult.Warnings.Any())
            {
                MessageBox.Show(String.Join("\n", queryResult.Warnings), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
