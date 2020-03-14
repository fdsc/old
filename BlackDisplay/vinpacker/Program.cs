using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace vinpacker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (args.Length > 0)
            {
                var create = args[0] == "1";
                var file   = args[1];   // ключевой файл
                var fi     = new FileInfo(args[2]);
                var pn     = args[3];   // Имя пакета

                Form1.MainPack(create, file, fi, pn, false);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)      // :логирование, :ошибки
        {
            var errorMessage = "\r\n{0}:\tvinpacker error\r\n\twith message '{1}'\r\n\r\n\tTrace:\r\n\t{2}\r\n\r\n\r\n";

            if (e.ExceptionObject is Exception)
            {
                var Exception = e.ExceptionObject as Exception;
                File.AppendAllText("error.log", String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
            else
                File.AppendAllText("error.log", String.Format(errorMessage, DateTime.Now, "unhandled undefined domain exception!", e.ExceptionObject.ToString().Replace("\n", "\n\t")));
        }
    }
}
