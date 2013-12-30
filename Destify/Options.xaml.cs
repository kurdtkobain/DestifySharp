using System.Globalization;
using System.Windows;
using STA.Settings;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options
    {
        readonly INIFile _settings = new INIFile("settings.ini");
        private string _port, _theme,_cipher;
        private int _displaytime;
        public Options()
        {
            InitializeComponent();
            loadSettings();
            textBox1.Text = _port;
            textBox2.Text = _displaytime.ToString(CultureInfo.InvariantCulture);
            textBox3.Text = _theme;
            textBox4.Text = _cipher;
        }

        private void button1Click(object sender, RoutedEventArgs e)
        {
            _port = textBox1.Text;
            _displaytime = int.Parse(textBox2.Text);
            _theme = textBox3.Text;
            _cipher = textBox4.Text;
            saveSettings();
        }
        void loadSettings()
        {
            _port = _settings.GetValue("System", "Port", "3128");
            _displaytime = _settings.GetValue("System", "notifytime", 4);
            _cipher = _settings.GetValue("System", "cipher", "");
            _theme = _settings.GetValue("Theme", "folder", "default");
        }

        void saveSettings()
        {
            _settings.SetValue("System", "Port", _port);
            _settings.SetValue("System", "notifytime", _displaytime);
            _settings.SetValue("System", "cipher", _cipher);
            _settings.SetValue("Theme", "folder", _theme);
            _settings.Flush();
        }
    }
}
