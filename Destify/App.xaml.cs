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
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private HttpListener listener;
        private Thread listenerTH;
        private string port, theme, cipher;
        private int displaytime;
        bool running = true;
        private INIFile settings = new INIFile("settings.ini");


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.MainWindow = new MainWindow();
            loadSettings();
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            listener = new HttpListener();
            string prefixes = String.Format(@"http://+:{0}/", port);
            listener.Prefixes.Add(prefixes);
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            try
            {
                listener.Start();
                this.listenerTH = new Thread(new ParameterizedThreadStart(startListener));
                this.listenerTH.Name = @"HTTPListener Thread";
                this.listenerTH.SetApartmentState(ApartmentState.STA);
                this.listenerTH.Start();
                string text = String.Format("{0}:{1}", Utilities.localIPAddress(), port);
                notifyIcon.ShowBalloonTip(@"Server Started", text, notifyIcon.Icon);
            }
            catch
            {
                notifyIcon.ShowBalloonTip(@"Error", "Failed to start server, could not bind port.", BalloonIcon.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            terminateListenerTH();
            if (listener.IsListening) listener.Stop();
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
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == "(null)") input[i] = "";
            }
            NotificationCtrl alert = new NotificationCtrl(theme);
            if (input[5] == "iMessage")
            {
                alert.Title = String.Format(@"iMessage: {0}", input[2]);
                alert.Topic = "";
            }
            else
            {
                alert.Title = input[2];
                alert.Topic = input[5];
            }

            alert.Subtitle = input[3];
            alert.Message = input[4];
            alert.Time = input[6];

            notifyIcon.ShowCustomBalloon(alert, PopupAnimation.Slide, displaytime * 1000);
        }

        /// <summary>
        /// Loads settings.
        /// </summary>
        void loadSettings()
        {
            port = settings.GetValue("System", "Port", "3128");
            displaytime = settings.GetValue("System", "notifytime", 4);
            cipher = settings.GetValue("System", "cipher", "");
            theme = settings.GetValue("Theme", "folder", "default");
        }

        /// <summary>
        /// Saves settings.
        /// </summary>
        void saveSettings()
        {
            settings.SetValue("System", "Port", port);
            settings.SetValue("System", "notifytime", displaytime);
            settings.SetValue("System", "cipher", cipher);
            settings.SetValue("Theme", "folder", theme);
            settings.Flush();
        }

        private void startListener(object s)
        {
            while (true)
            {
                if (!running) break;
                processRequest();
            }
        }

        /// <summary>
        /// Shuts down listener thread.
        /// </summary>
        void terminateListenerTH()
        {
            running = false;
        }

        private void processRequest()
        {
            var result = listener.BeginGetContext(listenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();
        }

        private void listenerCallback(IAsyncResult result)
        {
            if (!running) return;
            var context = listener.EndGetContext(result);
            Thread.Sleep(1000);
            if (context.Request.HttpMethod == "POST")
            {
                string data_text = new StreamReader(context.Request.InputStream,
                                                    context.Request.ContentEncoding).ReadToEnd();

#if DEBUG
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Debug.txt", true))
                {
                    file.WriteLine(data_text);
                }
#endif
                bool shouldDecode = !data_text.Contains("Destify");
                if (cipher == "") shouldDecode = false;
                string[] lines = Regex.Split(data_text, "\r\n");
                string[] values = new string[9];
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    string s = lines[i];
                    if (s != "\r\n")
                    {
                        string[] tmp = s.Split(Convert.ToChar("="));
                        if (shouldDecode && values[i] != "(null)")
                        {
                            values[i] = Utilities.decode(tmp[1], cipher);
                        }
                        else
                        {
                            values[i] = tmp[1];
                        }
                    }
                }

                if (values[1] == "Destify")
                {
                    this.Dispatcher.Invoke((Action)(() => sendNotify(values)));
                }
                else
                {
                    notifyIcon.ShowBalloonTip(@"Decoding failed!", @"Please check your Cipher Key!", notifyIcon.Icon);
                }
            }
            else
            {
                string responseString = "<html><head><title>DestifySharp by KAMY Studios</title></head><body><meta HTTP-EQUIV=\"REFRESH\" content=\"0; url=http://kamy.tk\"></body></html>";
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
