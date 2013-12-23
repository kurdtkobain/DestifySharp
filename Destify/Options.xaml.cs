using System.Windows;
using STA.Settings;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        INIFile settings = new INIFile("settings.ini");
        private string port, theme,cipher;
        private int displaytime;
        public Options()
        {
            InitializeComponent();
            loadSettings();
            this.textBox1.Text = port;
            this.textBox2.Text = displaytime.ToString();
            this.textBox3.Text = theme;
            this.textBox4.Text = cipher;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            port = this.textBox1.Text;
            displaytime = int.Parse(textBox2.Text);
            theme = this.textBox3.Text;
            cipher = this.textBox4.Text;
            saveSettings();
        }
        void loadSettings()
        {
            port = settings.GetValue("System", "Port", "3128");
            displaytime = settings.GetValue("System", "notifytime", 4);
            cipher = settings.GetValue("System", "cipher", "");
            theme = settings.GetValue("Theme", "folder", "default");
        }

        void saveSettings()
        {
            settings.SetValue("System", "Port", port);
            settings.SetValue("System", "notifytime", displaytime);
            settings.SetValue("System", "cipher", cipher);
            settings.SetValue("Theme", "folder", theme);
            settings.Flush();
        }
    }
}
