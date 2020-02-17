using AppTrackerWin.Helper;
using AppTrackerWin.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace AppTrackerWin
{
    /// <summary>
    /// Interaction logic for ExcelDatabaseManagement.xaml
    /// </summary>
    public partial class ExcelDatabaseManagement : UserControl
    {
        StorageHelper _storage = new StorageHelper();
        public ObservableCollection<TrackedWindowStorage> allStoredData { get; set; }

        public ExcelDatabaseManagement()
        {
            InitializeComponent();
            _storage.CreateDatabaseFileIfNotExists();
            allStoredData = _storage.GetAllDatabaseEntriesObservable();
            listView.ItemsSource = allStoredData;
        }

        private void Delete_Entry_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure wou want to remove this item?", "Database", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if(result == MessageBoxResult.Yes)
            {
                TrackedWindowStorage rowEntity = ((Button)e.OriginalSource).DataContext as TrackedWindowStorage;
                allStoredData.Remove(rowEntity);
                var resultOfDelete = _storage.RemoveDatabaseEntry(rowEntity); 
                if (!resultOfDelete.isError)
                {
                    MessageBox.Show("Item removed", "Database", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error occured: " + resultOfDelete.Message, "Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btn_ClearDB_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure wou want to clear all the saved data?", "Database", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                result = MessageBox.Show("By continuing you will loose all your current data. Delete database?", "Database", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if(result == MessageBoxResult.Yes)
                {
                    var resultOfDelete = _storage.ClearAllData();
                    if (!resultOfDelete.isError)
                    {
                        allStoredData.Clear();
                        MessageBox.Show("Database cleared", "Database", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error occured: " + resultOfDelete.Message, "Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }   
            }
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            allStoredData = _storage.GetAllDatabaseEntriesObservable();
            listView.ItemsSource = allStoredData;
        }      
    }
}
