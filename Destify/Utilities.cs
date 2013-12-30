using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using IPNGConverter;

namespace DestifySharp
{
    class Utilities
    {
        /// <summary>
        /// Gets local IP address.
        /// </summary>
        public static string localIpAddress()
        {
            string localIp = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }

        /// <summary>
        /// Decodes string.
        /// </summary>
        public static String decode(String nText, String code)
        {
            var ctext = new StringBuilder();

            int a = 0;
            foreach (char t in nText)
            {
                char key = code.ToUpper()[a % code.Length];
                char aCh = t;
                char nCh;

                if (aCh >= 65 && aCh <= 90)
                {
                    aCh = t;
                    nCh = (char)(aCh + 65 - key);
                    if (nCh < 65) { nCh += (char)(26); }
                    if (t >= 97) { nCh += (char)(32); }
                    ctext.Append(nCh);
                    a++;
                }
                else if (aCh >= 97 && aCh <= 122)
                {
                    aCh = t;
                    nCh = (char)(aCh + 65 - key);
                    if (nCh < 97)
                    {
                        nCh += (char)(26);
                    }
                    ctext.Append(nCh);
                    a++;
                }
                else
                {
                    ctext.Append(t);
                }
            }
            return ctext.ToString();
        }

        /// <summary>
        /// Encodes string.
        /// </summary>
        public static String encode(String text, String code)
        {
            var ctext = new StringBuilder();
            for (int i = 0, a = 0; i < text.Length; i++)
            {
                var kiCh = (int)code.ToUpper()[a % code.Length];
                var aiCh = (int)text[i];
                int niCh;
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
        /// Native C# version of hexStringToByteArray
        /// </summary>
        public static byte[] stringToByteArray(String hex)
        {
            hex = hex.Substring(1, hex.Length - 1).Replace(" ", "");
            int numberChars = hex.Length / 2;
            var bytes = new byte[numberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < numberChars; i++)
                    bytes[i] = Convert.ToByte(new string(new[] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }

        /// <summary>
        /// C# port of Destify's MD5
        /// </summary>
        public static String md5(String md5S)
        {
            try
            {
                MD5 md = new MD5CryptoServiceProvider();
                byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(md5S));
                var  sb = new StringBuilder ();

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
        public static string checkConvert(string input)
        {
            if (input != "(null)" || input != "")
            {
                string fHash = md5(input);
                byte[] bytes = stringToByteArray(input);
                string inpng = String.Format("./Resources/{0}.png", fHash);
                string outpng = String.Format("./Resources/{0}-new.png", fHash);
                if(File.Exists(outpng))
                {
                    return outpng;
                }
                var png = new FileStream(inpng, FileMode.OpenOrCreate);
                png.Write(bytes, 0, bytes.Length);
                png.Close();
                if(!PngConverter.convert(inpng))
				{
				return inpng;
				}
                return outpng;
            }
            return input;
        }
    }
}
