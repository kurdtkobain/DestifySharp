using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using STA.Settings;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for NotificationCtrl.xaml
    /// </summary>
    public partial class NotificationCtrl : UserControl
    {
        private readonly INIFile _styleini;
        private double _width, _height;
        private string _closeimgsrc, _bgimgsrc, _iconsrc;
        private readonly string _styledir;
        private bool _isClosing = false;

        public string title
        {
            set { titleLbl.Text = value; }
        }
        public string subtitle
        {
            set { subtitleLbl.Text = value; }
        }
        public string message
        {
            set { messageLbl.Text = value; }
        }
        public string topic
        {
            set {topicLbl.Text = value;}
        }
        public string time
        {
            set
            {
                var dateTime = Convert.ToDateTime(value);
                timeLbl.Text = dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
            }
        }
        public string icon
        {
            set { _iconsrc = value; }
        }

        public NotificationCtrl( string style)
        {
            _styledir = style;
            string tmp = String.Format(@"./{0}/style.ini", _styledir);
            _styleini = new INIFile(tmp);
            loadSettings();
            InitializeComponent();

            TaskbarIcon.AddBalloonClosingHandler(this, onNotificationClosing);
        }

        void loadSettings()
        {
            _width = _styleini.GetValue("Window", "width", Width);
            _height = _styleini.GetValue("Window", "height", Height);
            _bgimgsrc = _styleini.GetValue("Window", "bgimage", "BG.png");
            _closeimgsrc = _styleini.GetValue("Window", "closeimg", "Close.gif");


        }

        void setup()
        {
            Width = _width;
            Height = _height;
            string tmpbgsrc = String.Format(@"./{0}/{1}",_styledir,_bgimgsrc);
            grid1.Background = new ImageBrush(new BitmapImage(new Uri(tmpbgsrc, UriKind.Relative)));
            string tmpclosesrc = String.Format(@"./{0}/{1}", _styledir, _closeimgsrc);
            image1.Source = new BitmapImage(new Uri(tmpclosesrc, UriKind.Relative));
            if (_iconsrc == "(null)" || string.IsNullOrEmpty(_iconsrc))
            {
                _iconsrc = "Resources/Destify@2x.png";
            }
            image2.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_iconsrc);
        }

        private void onNotificationClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true; //suppresses the popup from being closed immediately
            _isClosing = true;
        }

        private void image1MouseDown(object sender, MouseButtonEventArgs e)
        {
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        private void gridMouseEnter(object sender, MouseEventArgs e)
        {
            if (_isClosing) return;

            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }

        private void onFadeOutCompleted(object sender, EventArgs e)
        {
            var pp = (Popup)Parent;
            pp.IsOpen = false;
        }

        private void userControlLoaded(object sender, RoutedEventArgs e)
        {
            setup();
        }
    }
}
