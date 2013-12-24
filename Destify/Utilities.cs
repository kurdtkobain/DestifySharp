using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using PNGDecrush;

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

        /// <summary>
        /// C# port of Destify's hexStringToByteArray
        /// </summary>
        public static byte[] hexStringToByteArray(String s)
        {
            s = s.Substring(1, s.Length - 1).Replace(" ", "");

            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte)((getIntegerValue(s[i], 16) << 4) + getIntegerValue(s[i + 1], 16));
            }

            return data;
        }

        /// <summary>
        /// Native C# version of hexStringToByteArray (not tested)
        /// </summary>
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Like Java's Character.digit (only works for 0-9, a-z, A-Z)
        /// </summary>
        static int getIntegerValue(char c, int radix)
        {
            int val = -1;
            if (char.IsDigit(c))
                val = (int)(c - '0');
            else if (char.IsLower(c))
                val = (int)(c - 'a') + 10;
            else if (char.IsUpper(c))
                val = (int)(c - 'A') + 10;
            if (val >= radix)
                val = -1;
            return val;
        }

        /// <summary>
        /// C# port of Destify's MD5
        /// </summary>
        public static String MD5(String md5S)
        {
            try
            {
                MD5 md = new MD5CryptoServiceProvider();
                byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(md5S));
                StringBuilder  sb = new StringBuilder ();

                for (int i = 0; i < array.Length; ++i)
                {
                    sb.Append(array[i].ToString("x2"));
                }
                return  sb.ToString();
            }
            catch (Exception e) { }
            return null;
        }

        /// <summary>
        /// Checks if the program should decrush the png data
        /// </summary>
        public static string checkDecrush(string input)
        {
            if (input != "(null)" && input != "")
            {
                string fHash = Utilities.MD5(input);
                byte[] bytes = Utilities.hexStringToByteArray(input);
                string outpng = String.Format("Resources/{0}.png", fHash);
                using (FileStream uncFile = File.Create(outpng))
                {
                    MemoryStream cStream = new MemoryStream();
                    cStream.Write(bytes,0,bytes.Length);
                    try
                    {
                        PNGDecrusher.Decrush(cStream, uncFile);
                        cStream.Close();
                        return outpng;
                    }
                    catch (InvalidDataException)
                    {
                        // decrushing failed, either an invalid PNG or it wasn't crushed
                        cStream.Close();
                        return "";
                    }
                }
            }
        }
    }
}
