using System.Data;
using System.Windows;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Window
    {
        public History()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase();
            DataTable result = db.GetDataTable("SELECT * FROM history;");
            this.dataGrid1.ItemsSource = result.DefaultView;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase();
            db.ClearTable("history");
            DataTable result = db.GetDataTable("SELECT * FROM history;");
            this.dataGrid1.ItemsSource = result.DefaultView;
        }
    }
}
