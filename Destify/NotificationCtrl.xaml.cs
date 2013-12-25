using System;
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
        private INIFile styleini;
        private double width, height;
        private string closeimgsrc, bgimgsrc, styledir, iconsrc;
        private bool isClosing = false;

        public string Title
        {
            set { titleLbl.Text = value; }
        }
        public string Subtitle
        {
            set { subtitleLbl.Text = value; }
        }
        public string Message
        {
            set { messageLbl.Text = value; }
        }
        public string Topic
        {
            set { topicLbl.Text = value; }
        }
        public string Time
        {
            set { timeLbl.Text = value; }
        }
        public string Icon
        {
            set { iconsrc = value; }
        }

        public NotificationCtrl( string style,string icon)
        {
            styledir = style;
            if (icon == "(null)" || string.IsNullOrEmpty(icon))
            {
                iconsrc = "Resources/Destify@2x.png";
            }
            else
            {
                iconsrc = icon;
            }
            string tmp = String.Format(@"./{0}/style.ini", styledir);
            styleini = new INIFile(tmp);
            loadSettings();
            InitializeComponent();
            setup();
            

            TaskbarIcon.AddBalloonClosingHandler(this, OnNotificationClosing);
        }

        void loadSettings()
        {
            width = styleini.GetValue("Window", "width", this.Width);
            height = styleini.GetValue("Window", "height", this.Height);
            bgimgsrc = styleini.GetValue("Window", "bgimage", "BG.png");
            closeimgsrc = styleini.GetValue("Window", "closeimg", "Close.gif");


        }

        void saveSettings()
        {
            styleini.SetValue("Window", "width", width);
            styleini.SetValue("Window", "height", height);
            styleini.SetValue("Window", "bgimage", bgimgsrc);
            styleini.SetValue("Window", "closeimg", closeimgsrc);
            styleini.Flush();

        }

        void setup()
        {
            this.Width = width;
            this.Height = height;
            string tmpbgsrc = String.Format(@"./{0}/{1}",styledir,bgimgsrc);
            grid1.Background = new ImageBrush(new BitmapImage(new Uri(tmpbgsrc, UriKind.Relative))); ;
            string tmpclosesrc = String.Format(@"./{0}/{1}", styledir, closeimgsrc);
            image1.Source = new BitmapImage(new Uri(tmpclosesrc, UriKind.Relative));
            //image2.Source = new BitmapImage(new Uri(iconsrc,UriKind.Relative));
            image2.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(iconsrc);
        }

        private void OnNotificationClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true; //suppresses the popup from being closed immediately
            isClosing = true;
        }

        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isClosing) return;

            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }

        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Popup pp = (Popup)Parent;
            pp.IsOpen = false;
        }
    }
}
