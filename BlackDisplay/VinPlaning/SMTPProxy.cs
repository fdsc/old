using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;

namespace VinPlaning
{
    public partial class SMTPProxy : Form
    {
        public SMTPProxy()
        {
            InitializeComponent();
        }

        public class tcpResult
        {
            string result;
            string error;

            public tcpResult(string str, string err)
            {
                result = str;
                error  = err;
            }

            public static implicit operator tcpResult(string str)
            {
                return new tcpResult(str, null);
            }

            public static tcpResult tcpError(string str)
            {
                return new tcpResult(null, str);
            }
        }

        public static void sendMail(Socket client, string server, int port = 25, int timeOut = 25000)
        {
            using (TcpClient mailServer = new TcpClient(server, port))
            {
                mailServer.SendTimeout = timeOut;

                using (var stream = mailServer.GetStream())
                using (var tls    = new SslStream(stream, false, null, null, EncryptionPolicy.RequireEncryption))
                {
                    tls.AuthenticateAsClient(server, null, System.Security.Authentication.SslProtocols.Tls, true);
                    getTcpData(tls, mailServer, true);
                }
            }
        }

        private static string GetGmtRfcTime()
        {
            return DateTime.Now.ToUniversalTime().ToString("R");
        }

        private static string getHumanRfcTime()
        {
            var dtfi = new System.Globalization.CultureInfo("RU-ru");

            return DateTime.Now.ToString("ddd, dd MMM yyyy HH':'mm':'ss K", dtfi);
        }

        public static tcpResult getTcpData(SslStream stream, TcpClient tcp, bool CRLF, long timeout = 25L * 1000L * 10000L)
        {
            if (!tcp.Connected)
                return tcpResult.tcpError("no connect");

            using (var ms = new MemoryStream())
            {
                int  i = tcp.Available;
                byte last = 0;
                var  now = DateTime.Now.Ticks;
                while (i > 0 || (CRLF && last != 10))
                {
                    if (i > 0)
                    {
                        byte[] buf = new byte[i];
                        stream.Read(buf, 0, i);
                        ms.Write(buf, 0, buf.Length);
                        last = buf[buf.Length - 1];
                        now = DateTime.Now.Ticks;
                    }
                    else
                        if (DateTime.Now.Ticks - now > timeout)   //25 секунд
                            throw new TimeoutException();

                    i = tcp.Available;
                }

                string result = UTF8Encoding.UTF8.GetString(ms.ToArray());

                File.AppendAllText("sendmail.log", result);

                return result;
            }
        }

        public static void setTcpData(NetworkStream stream, TcpClient tcp, string data, bool CRLF)
        {
            var RawData = data + (CRLF ? "\r\n" : "");

            byte[] buf = Encoding.UTF8.GetBytes(RawData);
            stream.Write(buf, 0, buf.Length);
        }
    }
}
