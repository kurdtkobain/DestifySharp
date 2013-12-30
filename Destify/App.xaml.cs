using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using STA.Settings;

namespace DestifySharp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private TaskbarIcon _notifyIcon;
        private HttpListener _listener;
        private Thread _listenerTh;
        private string _port, _theme, _cipher;
        private int _displaytime;
        bool _running = true;
        private readonly INIFile _settings = new INIFile("settings.ini");


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow();
            loadSettings();
            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            _listener = new HttpListener();
            string prefixes = String.Format(@"http://+:{0}/", _port);
            _listener.Prefixes.Add(prefixes);
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            try
            {
                _listener.Start();
                _listenerTh = new Thread(startListener) {Name = @"HTTPListener Thread"};
                _listenerTh.SetApartmentState(ApartmentState.STA);
                _listenerTh.Start();
                string text = String.Format("{0}:{1}", Utilities.localIpAddress(), _port);
                if (_notifyIcon != null) _notifyIcon.ShowBalloonTip(@"Server Started", text, _notifyIcon.Icon);
            }
            catch
            {
                if (_notifyIcon != null) _notifyIcon.ShowBalloonTip(@"Error", "Failed to start server, could not bind port.", BalloonIcon.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            terminateListenerTh();
            if (_listener.IsListening) _listener.Stop();
            base.OnExit(e);
        }

        /// <summary>
        /// Adds notification to history database.
        /// </summary>
        void addNotificationToHistory(string[] input)
        {
            var db = new SQLiteDatabase();
            db.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS history (title, subtitle, message, time);");
            string tmpinsert = String.Format(@"INSERT INTO history VALUES ('{0}', '{1}', '{2}', '{3}');", input[2], input[3], input[4], input[6]);
            db.ExecuteNonQuery(tmpinsert);

        }

        /// <summary>
        /// Formats and displays notification data.
        /// </summary>
        private void sendNotify(string[] input)
        {
            addNotificationToHistory(input);
            input[7] = Utilities.checkConvert(input[7]);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == "(null)") input[i] = "";
            }
            var alert = new NotificationCtrl(_theme);
            if (input[5] == "iMessage")
            {
                alert.title = String.Format(@"iMessage: {0}", input[2]);
                alert.topic = "";
            }
            else
            {
                alert.title = input[2];
                alert.topic = input[5];
            }

            alert.subtitle = input[3];
            alert.message = input[4];
            alert.time = input[6];
            alert.icon = input[7];

            _notifyIcon.ShowCustomBalloon(alert, PopupAnimation.Slide, _displaytime * 1000);
        }

        /// <summary>
        /// Loads settings.
        /// </summary>
        void loadSettings()
        {
            _port = _settings.GetValue("System", "Port", "3128");
            _displaytime = _settings.GetValue("System", "notifytime", 4);
            _cipher = _settings.GetValue("System", "cipher", "");
            _theme = _settings.GetValue("Theme", "folder", "default");
        }

        private void startListener(object s)
        {
            while (true)
            {
                if (!_running) break;
                processRequest();
            }
        }

        /// <summary>
        /// Shuts down listener thread.
        /// </summary>
        void terminateListenerTh()
        {
            _running = false;
        }

        private void processRequest()
        {
            var result = _listener.BeginGetContext(listenerCallback, _listener);
            result.AsyncWaitHandle.WaitOne();
        }

        private void listenerCallback(IAsyncResult result)
        {
            if (!_running) return;
            var context = _listener.EndGetContext(result);
            Thread.Sleep(1000);
            if (context.Request.HttpMethod == "POST")
            {
                string dataText = new StreamReader(context.Request.InputStream,
                                                    context.Request.ContentEncoding).ReadToEnd();
                bool shouldDecode = !dataText.Contains("Destify");
                if (_cipher == "") shouldDecode = false;
                string[] lines = Regex.Split(dataText, "\r\n");
                var values = new string[9];
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    string s = lines[i];
                    if (s != "\r\n")
                    {
                        string[] tmp = s.Split(Convert.ToChar("="));
                        if (shouldDecode && tmp[1] != "(null)")
                        {
                            if (tmp[0] != "time" || tmp[0] != "icon")
                            {
                                values[i] = Utilities.decode(tmp[1], _cipher);
                            }else
                            {
                                values[i] = tmp[1];
                            }
                        }
                        else
                        {
                            values[i] = tmp[1];
                        }
                    }
                }

                if (values[1] == "Destify")
                {
                    Dispatcher.Invoke((Action)(() => sendNotify(values)));
                }
                else
                {
                    _notifyIcon.ShowBalloonTip(@"Decoding failed!", @"Please check your Cipher Key!", _notifyIcon.Icon);
                }
            }
            else
            {
                const string responseString = "<html><head><title>DestifySharp by KAMY Studios</title></head><body><meta HTTP-EQUIV=\"REFRESH\" content=\"0; url=http://kamy.tk\"></body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

            }
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.Close();
        }
    }
}
