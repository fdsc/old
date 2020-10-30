using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using options;
using System.IO;
// using vbAccelerator.Components.Shell;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BlackDisplay
{
    static class Program
    {
        #if forLinux
            public static Boolean AllocConsole() {return false;}
            public static Boolean FreeConsole() {return false;}
            public static int GetVersion() {return -1;}
        #else

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern int GetVersion();
        #endif

        public static int WinVersion = GetVersion() & 0xFF;

        static public string version = "20200603";

        public readonly static string MainMutexName; // = "relaxtime.8vs.ru|" + BitConverter.ToString(System.Text.Encoding.Unicode.GetBytes(Application.StartupPath)).Replace("-", "");

        static Program()
        {
            var sha = new keccak.SHA3(1024);
            MainMutexName = "relaxtime.8vs.ru|" + BitConverter.ToString(sha.getHash224(System.Text.Encoding.Unicode.GetBytes(AppDomain.CurrentDomain.BaseDirectory))).Replace("-", "");
            m     = new System.Threading.Semaphore(1, 1, MainMutexName);
        }

        readonly static System.Threading.Semaphore m;

        static internal Form1 mainForm = null;
        [MTAThread]
        static int Main(string[] args)
        {
            AppDomain CurrentDomain = AppDomain.CurrentDomain;
            CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

#if DEBUG
            DbgLog.dbg.Disabled = false;
            DbgLog.dbg.setLogRegime(null, -1);
            DbgLog.dbg.setLogRegime(DbgLog.ERROR, 0);
            DbgLog.dbg.setLogRegime(DbgLog.MESSAGE, 0);
#else
            DbgLog.dbg.Disabled = false;
            DbgLog.dbg.setLogRegime(null, -1);
            DbgLog.dbg.setLogRegime(DbgLog.ERROR, 0);
#endif

            if (args.Length == 1 && args[0] == "-v")
            {
                AllocConsole();
                Console.WriteLine(version);
                Console.ReadKey();
                FreeConsole();
                DbgLog.allLogsDispose();
                return 0;
            }

            Directory.SetCurrentDirectory(Application.StartupPath);

            // анинсталляция
            if (args.Length > 0)
            {
                if (args[0] == "uninstall")
                {
                    Uninstall(false);
                }
                else
                {
                    AllocConsole();
                    Console.WriteLine("no correct parameters:\r\n\t uninstall - doing uninstallation");
                    
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();

                    FreeConsole();
                }

                DbgLog.allLogsDispose();
                return 0;
            }

            if (!m.WaitOne(10))
            {
                // Не выводим сообщение, чтобы освободить exe-файл для обновления как можно быстрее
                // MessageBox.Show("Невозможно запустить две программы 'relax time black display' одновременно", "Недопустимо", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ToLogFile("semaphore locked - program exited");
                DbgLog.allLogsDispose();
                return 3;
            }
            // toLogFileMessage("semaphore is green - program started");

            try
            {
                DbgLog.dbg.messageToLog("", "semaphore is green - program started");
                /*if (File.Exists("update.flag"))
                {
                    toLogFile("update.flag exists - exit program");
                    Form1.updateProcessStart();
                    return 31;
                }
                */
                /*
                try
                {
                    var fs = new FileSecurity(Path.GetFullPath(Application.StartupPath), AccessControlSections.Access);
                    fs.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories | FileSystemRights.ChangePermissions | FileSystemRights.AppendData | FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.ListDirectory | FileSystemRights.Modify | FileSystemRights.ReadAndExecute | FileSystemRights.Write, AccessControlType.Allow));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }*/

                //File.SetAccessControl(Path.GetDirectoryName(Application.StartupPath), fs);

                // setUninstall();
                TruncateLog(new FileInfo(Application.StartupPath + errorLogFileName));

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                mainForm = new Form1();
                Application.Run(mainForm);
            }
            finally
            {
                m.Release();
                m.Close();
            }
            DbgLog.allLogsDispose();
            return 0;
        }

        public static void Uninstall(bool isApplicationExists)
        {
            var msg = DeleteBootRun();
            if (isApplicationExists)
            {
                new Uninstall(msg).Show();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Uninstall(  msg  ));
            }
        }

        // :логирование :$$$.логирование
        private static void TruncateLog(FileInfo fi)
        {
            if (fi.Exists && fi.Length > 128 * 1024)
            {
                var text = File.ReadAllText(fi.FullName);
                text = "truncated " + DateTime.Now + "\r\n\r\n" + text.Remove(0, text.Length * 3 / 4);
                File.WriteAllText(fi.FullName, text);
            }
        }

        // :$$$.реестр
        private static string DeleteBootRun()
        {
            try
            {
                bool notRun;
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    notRun = rk.GetValue(Form1.prgName) == null;
                }

                if (!notRun)
                    using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        rkApp.DeleteValue(Form1.prgName, false);
                        //Console.WriteLine("startup run removed");
                    }

                //Console.WriteLine(":) uninstalled successful - remove program directory manually");

                return null;
            }
            catch (Exception e)
            {
                //Console.WriteLine(":( error during uninstall");
                return e.Message;
            }
        }

        // анинсталляция сейчас сделана по другому - этот код не используется
        /*static public void setUninstall()
        {
            var ep = Application.ExecutablePath;
            var wd = Path.GetDirectoryName(ep);
            var ln = wd + "\\" + "uninstall.exe.lnk";

            if ( !File.Exists(ln) )
            using (ShellLink shortcut = new ShellLink())
            {
                shortcut.Arguments          = "uninstall";
                shortcut.Target             = ep;
                shortcut.WorkingDirectory   = wd;
                shortcut.Description        = "Запуск BlackDisplay.exe в режиме удаления";
                shortcut.DisplayMode        = ShellLink.LinkDisplayMode.edmNormal;
                shortcut.Save(ln);
            }
        }*/

        public static string errorLogFileName = "/error_rtbd.log";                                               // :логирование.файл
        public static string errorMessage = "\r\n{0}:\trelax time error\r\n\twith message '{1}'\r\n\r\n\tTrace:\r\n\t{2}\r\n\r\n\r\n";
        public static string   logMessage = "\r\n{0}:\trelax time message\r\n\twith message '{1}'\r\n\r\n";
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)      // :логирование, :ошибки
        {
            if (e.ExceptionObject is Exception)
            {
                var Exception = e.ExceptionObject as Exception;
                File.AppendAllText(Application.StartupPath + errorLogFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
            else
                File.AppendAllText(Application.StartupPath + errorLogFileName, String.Format(errorMessage, DateTime.Now, "unhandled undefined domain exception!", e.ExceptionObject == null ? "null" : e.ExceptionObject.ToString().Replace("\n", "\n\t")));

            Form1.logTime(Form1.TimeRecord.ended);
            Form1.toWndLog();
        }

        public static void ToLogFile(string msg)
        {
            var s = String.Format(errorMessage, DateTime.Now, msg, "none");
            File.AppendAllText(Application.StartupPath + errorLogFileName, s);
            DbgLog.dbg.errorToLog("ProgramLogFile", s);
        }

        public static void ToLogFileMessage(string msg)
        {
            var s = String.Format(logMessage, DateTime.Now, msg, "none");
            File.AppendAllText(Application.StartupPath + errorLogFileName, s);
            DbgLog.dbg.messageToLog("ProgramLogFile", s);
        }
    }
}
