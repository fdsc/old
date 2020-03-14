using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace options
{
    public class SMTP_YandexMX
    {
        public static bool send(string message, string msgId, bool nonLogTextMessage, int timeOut = 25000)
        {
            if (timeOut < 5000)
                timeOut = 5000;

            bool result = false;
            string currentDT = GetGmtRfcTime();
            string humanTime = getHumanRfcTime();
            File.AppendAllText("sendmail.log", "\r\n\r\nNew connect " + humanTime + DateTime.Now.Kind + "\r\n");

            message = humanTime + " / " + msgId + "\r\n" + message;
            if (!nonLogTextMessage)
                File.WriteAllText("sendmail.txt", message);

            string host = "noauthenticate.8vs.ru"; //"mx.yandex.ru";
            var msgFrom = "FFB337CC2BEF43B89ACD65186DCAA454@noauthenticate.8vs.ru";

            using (TcpClient client = new TcpClient("mx.yandex.ru", 25))
            {
                client.SendTimeout = timeOut;

                using (var stream = client.GetStream())
                {
                    getTcpData(stream, client, true);
                    setTcpData(stream, client, "HELO " + host, true);
                    getTcpData(stream, client, true);
                    setTcpData(stream, client, "MAIL FROM:" + msgFrom, true);
                    getTcpData(stream, client, true);
                    setTcpData(stream, client, "RCPT TO:prg@8vs.ru", true);
                    getTcpData(stream, client, true);
                    setTcpData(stream, client, "DATA", true);
                    getTcpData(stream, client, true);

                    var headers = "MIME-Version: 1.0\r\n" +
                    "Content-Disposition: inline\r\n" +
                    "Content-Type: text/plain; charset=\"UTF-8\"\r\n"+
                    "Date: " + currentDT + "\r\n" +
                    "Message-Id: " + msgId;

                    setTcpData(stream, client, headers, true);
                    setTcpData(stream, client, "from: " + msgFrom, true);
                    setTcpData(stream, client, "to: prg@8vs.ru", true);
                    setTcpData(stream, client, "subject: авт. уведомление", true);
                    setTcpData(stream, client, "", true);
                    setTcpData(stream, client, message.Replace("\r\n.\r\n", "\r\n. \r\n"), true);
                    setTcpData(stream, client, ".", true);
                    var msg = getTcpData(stream, client, true);
                    if (msg.StartsWith("250 2.0.0"))
                        result = true;

                    setTcpData(stream, client, "QUIT", true);
                    getTcpData(stream, client, true);
                }

                // client.Close();
            }
            //Console.ReadLine();

            return result;
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

        public static string getTcpData(NetworkStream stream, TcpClient tcp, bool CRLF)
        {
            if (!tcp.Connected)
                return "no connect";

            using (var ms = new MemoryStream())
            {
                int  i = tcp.Available;
                byte last = 0;
                var  now = DateTime.Now.Ticks;
                while (i > 0 || (last != 10 && CRLF))
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
                        if (DateTime.Now.Ticks - now > 25L * 1000L * 10000L)   //25 секунд
                            throw new TimeoutException();

                    i = tcp.Available;
                }

                string result = UTF8Encoding.UTF8.GetString(ms.ToArray());

                // Console.Write(result);
                File.AppendAllText("sendmail.log", result);

                return result;
            }
        }

        public static void setTcpData(NetworkStream stream, TcpClient tcp, string data, bool CRLF)
        {
            var RawData = data + (CRLF ? "\r\n" : "");
            // File.AppendAllText("sendmail.log", RawData);
            // Console.Write(RawData);

            byte[] buf = Encoding.UTF8.GetBytes(RawData);
            stream.Write(buf, 0, buf.Length);
        }
    }
}
