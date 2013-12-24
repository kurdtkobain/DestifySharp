using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DestifySharp
{
    class Utilities
    {
        /// <summary>
        /// Gets local IP address.
        /// </summary>
        public static string localIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        /// <summary>
        /// Decodes string.
        /// </summary>
        public static String decode(String NText, String code)
        {
            StringBuilder ctext = new StringBuilder();

            int a = 0;
            for (int i = 0; i < NText.Length; i++)
            {
                char key = code.ToUpper()[a % code.Length];
                char aCh = NText[i];
                char nCh = ' ';

                if (aCh >= 65 && aCh <= 90)
                {
                    aCh = NText[i];
                    nCh = (char)(aCh + 65 - key);
                    if ((int)nCh < 65) { nCh += (char)(26); }
                    if (NText[i] >= 97) { nCh += (char)(32); }
                    ctext.Append(nCh);
                    a++;
                }
                else if (aCh >= 97 && aCh <= 122)
                {
                    aCh = NText[i];
                    nCh = (char)(aCh + 65 - key);
                    if ((int)nCh < 97)
                    {
                        nCh += (char)(26);
                    }
                    ctext.Append(nCh);
                    a++;
                }
                else
                {
                    nCh = (char)(aCh);
                    ctext.Append(NText[i]);
                }
            }
            return ctext.ToString();
        }

        /// <summary>
        /// Encodes string.
        /// </summary>
        public static String encode(String text, String code)
        {
            StringBuilder ctext = new StringBuilder();
            for (int i = 0, a = 0; i < text.Length; i++)
            {
                int kiCh = (int)code.ToUpper()[a % code.Length];
                int aiCh = (int)text[i];
                int niCh = (int)' ';
                if (aiCh >= 65 && aiCh <= 90)
                {
                    aiCh = text[i];
                    niCh = (aiCh + kiCh - 65);
                    if (niCh > 90) { niCh -= 26; }
                    ctext.Append(niCh);
                    a++;
                }
                else if (aiCh >= 97 && aiCh <= 122)
                {
                    aiCh = text[i];
                    niCh = (aiCh + kiCh - 65);
                    if (niCh <= 97) { niCh += 26; }
                    if (niCh > 122) { niCh -= 26; }
                    ctext.Append(niCh);
                    a++;
                }
                else
                {
                    niCh = aiCh;
                    ctext.Append(niCh);
                }
            }
            return ctext.ToString();
        }
    }
}
