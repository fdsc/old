using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Tests;
using VinPlaning;
using System.Net.Sockets;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using options;
using keccak;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Random8.test();
            var sha3 = new SHA3(8192);

            string dir;
            if (args.Length == 1)
            {
                dir = args[0];
            }
            else
                dir = Directory.GetCurrentDirectory();
            /*
            var sc = new SmtpClient("localhost", 15025);
            var mm = new MailMessage("fdsc@yandex.ru", "fdsc@yandex.ru", "test", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            sc.Timeout = 5000;
            sc.Credentials = new NetworkCredential("free", "pass");

            sc.Send(mm);
            sc.Dispose();
            */
            /*
            var ep80 = new IPEndPoint(IPAddress.Any, 80);
            var loopback = new IPEndPoint(IPAddress.Loopback, 80);
            var ep81 = new IPEndPoint(IPAddress.Any, 81);
            var ep82 = new IPEndPoint(IPAddress.Any, 82);


            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.DontFragment = true;
            server.ReceiveTimeout = 1000;
            // ~28 байт - IP и UDP заголовки; непонятно, как влияет недостаточный размер буфера на потерю сообщений. Впечатление, что сообщения могут и не влезать в буфер. В то же время при малом буфере (меньше размера одного пакета данных без UDP/IP-заголовка в одновременно посылаемых двух сообщениях) сообщения теряются
            server.ReceiveBufferSize = 1024 * 1024;
            server.ExclusiveAddressUse = true;
            server.Bind(ep80);

            server.SendTo(new byte[1] {0}, loopback);

            UdpClientSendAndClose(ep81, loopback, "cnt\x00");
            UdpClientSendAndClose(ep82, loopback, "cnt\x00");

            var begin = DateTime.Now.Ticks;

            do
            {
                var regex = new Regex("[\\x00-\\x1F]", RegexOptions.Compiled | RegexOptions.Multiline);
                Console.WriteLine("available {0}", server.Available);
                try
                {
                    var b = new byte[8 * 1024];
                    EndPoint EP = new IPEndPoint(IPAddress.Loopback, 8080);
                    var s = server.ReceiveFrom(b, ref EP);

                    Console.WriteLine("{0} from {2}: {1}", s, regex.Replace(UTF8Encoding.UTF8.GetString(b, 0, s), " ", -1), EP.ToString());
                    Console.ReadLine();
                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0}", e.ErrorCode);
                }
            }
            while (server.Available > 0);

            Console.WriteLine("server {0} closed", server.LocalEndPoint.ToString());

            server.Close();
            Console.ReadLine();
            */

            
            var keccak = new keccak.SHA3(1024*1024);
            File.WriteAllBytes("D:/bytesout.txt",
                keccak.multiCryptLZMA(
                    File.ReadAllBytes("D:/bytes.txt"),
                    Encoding.GetEncoding("windows-1251").GetBytes("Метель ей пела песенку: спи ёлочка бай-бай"),
                    null
                    //Encoding.GetEncoding("windows-1251").GetBytes("Мороз снежком укутывал: смотри не замерзай"))
                    )
                );

            File.WriteAllBytes("D:/bytesdec.txt",
                keccak.multiDecryptLZMA(File.ReadAllBytes("D:/bytesout.txt"), Encoding.GetEncoding("windows-1251").GetBytes("Метель ей пела песенку: спи ёлочка бай-бай"))
            );

            var g = new Gost28147Modified();
            var gamma3 = g.getGOSTGamma(keccak.getHash384(keccak.getHash512("Жуткий ключ")), keccak.getHash256("Синхропосылка"), Gost28147Modified.ESbox_A, 1024*1024);
            File.WriteAllBytes("D:/bytegamma28147.txt", gamma3);

            /*var g = new Gost28147Modified();
            g.prepareGamma(UTF8Encoding.UTF8.GetBytes("Но вот багряною рукою Заря от утренних долин"), UTF8Encoding.UTF8.GetBytes("Выводит с солнцем"), Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProA);
            File.WriteAllBytes("D:/bytesout28147.txt", g.getGamma(1024*1024));*/
        }

        private static void UdpClientSendAndClose(IPEndPoint source, IPEndPoint target, string UTF8String)
        {
            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // client.Ttl = 128;
                client.DontFragment = true;
                client.ExclusiveAddressUse = true;
                client.Bind(source);

                var bb = new BytesBuilder();
                bb.add(UTF8String);

                client.SendTo(bb.getBytes(), target);
            }
        }

        public static string getTcpData(NetworkStream stream, TcpClient tcp, bool CRLF)
        {
            if (!tcp.Connected)
                return "no connect";

            using (var ms = new MemoryStream())
            {
                int i = tcp.Available;
                byte last = 0;
                while (i > 0 || (last != 10 && CRLF))
                {
                    if (i > 0)
                    {
                        byte[] buf = new byte[i];
                        stream.Read(buf, 0, i);
                        ms.Write(buf, 0, buf.Length);
                        last = buf[buf.Length - 1];
                    }

                    i = tcp.Available;
                }

                string result = UTF8Encoding.UTF8.GetString(ms.ToArray());

                Console.Write(result);

                return result;
            }
        }

        public static void setTcpData(NetworkStream stream, TcpClient tcp, string data, bool CRLF)
        {
            var RawData = data + (CRLF ? "\r\n" : "");
            Console.Write(RawData);
            byte[] buf = Encoding.UTF8.GetBytes(RawData);
            stream.Write(buf, 0, buf.Length);
        }

        private static void testExe(string fileName)
        {
            testDll(fileName);
        }

        private static void testDll(string fileName)
        {
            var asm     = Assembly.LoadFrom(fileName);
            var types   = asm.GetTypes();
            foreach (var type in types)
                testType(asm, type);
        }

        private static void testType(Assembly asm, Type type)
        {
            var methods = type.GetMethods();
            foreach (var m in methods)
            {
                if (!m.IsStatic)
                    continue;

                var att = m.GetCustomAttributes(typeof(TestAttribute), false);
                if (att.Length <= 0)
                    continue;

                TestMethodResult t = null;
                try
                {
                    t = (TestMethodResult) m.Invoke(null, null);
                }
                catch (Exception e)
                {
                    t = new TestMethodResult(TestMethodResult.generalTestResult.testError);
                    t.message = e.Message + " // " + asm.FullName + "->" + type.FullName + "->" + m.Name;
                    t.errorInfo = e.StackTrace;
                }
            }
        }
    }
}
