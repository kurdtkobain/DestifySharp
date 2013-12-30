using System.Data;
using System.Windows;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History
    {
        public History()
        {
            InitializeComponent();
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase();
            DataTable result = db.GetDataTable("SELECT * FROM history;");
            dataGrid1.ItemsSource = result.DefaultView;
        }

        private void menuItemClick(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase();
            db.ClearTable("history");
            DataTable result = db.GetDataTable("SELECT * FROM history;");
            dataGrid1.ItemsSource = result.DefaultView;
        }
    }
}
