using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Net;
using System.Threading;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace updator
{
    static class updator
    {
#if forLinux
        public static Boolean AllocConsole() {return false;}
        public static bool FreeConsole() {return false;}
#else
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
#endif
        static System.Threading.EventWaitHandle stopEvent = new System.Threading.EventWaitHandle(false, EventResetMode.ManualReset, "vs8.ru updateEvent");
        static System.Threading.Semaphore       s         = new System.Threading.Semaphore(50, 50, "vs8.ru updator semaphore");
        static System.Threading.Mutex           iniMutex  = new Mutex(false, "vs8.ru updator ini-mutex");

        static internal string version = "20170416";

        static string argsString;
        [MTAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

            toUpdateLog("started\r\n" + stringArrayToLogString(args, "\t"));
            if (!s.WaitOne(0))
            {
                Console.WriteLine("semaphore locked to umove.exe; exited");
                toUpdateLog("semaphore locked to umove.exe; exited");
                return 3;
            }

            var umove = false;
            try
            {
                argsString = getArgumentsFromArgArray(args);

                if (args.Length == 1 && args[0] == "-umove")
                {
                    var result = umoveRename();
                    toUpdateLog("-umove");
                    umove = true;
                }
                else
                    umoveRename();

                if (args.Length == 1 && args[0] == "-v")
                {
                    AllocConsole();
                    Console.WriteLine(version);
                    Console.ReadKey();
                    FreeConsole();
                    return 0;
                }

                createOrParseIni();

                var uuPath = downloadUpdate(Directory.GetCurrentDirectory(), opts["updatorDir", ""].Replace("$$$", "update"), "update", version, args);
                if (!String.IsNullOrEmpty(uuPath))
                {
                    try
                    {
                        File.WriteAllText("uup.flag", Path.GetFullPath(uuPath) + "\r\n");
                        Process.Start("umove.exe", "uup.flag \"vs8.ru updator semaphore\" 50 \"\"");
                    }
                    catch (Exception e)
                    {
                        toUpdateLog(e.Message + "\r\n" + e.StackTrace);
                    }
                }

                if (umove)
                {
                    return 51;
                }

                if (
                    args.Length > 3 || args.Length < 3
                    || (args.Length == 1 && (args[0] == "-?" || args[0] == "/?" || args[0] == "/help" || args[0].ToLower() == "--help"))
                    )
                {
                    AllocConsole();
                    Console.WriteLine("updatorvs8.exe updateName version path_to_dwnl");
                    Console.WriteLine("for example: updatorvs8.exe relaxtime 20110929 D:/rtbd/");
                    Console.WriteLine("warning: umove.exe must be place to same directory updatorvs8.exe");
                    Console.ReadKey();
                    FreeConsole();
                    return 1;
                }

                var dirName = args[2];
                if (!Directory.Exists(dirName))
                {
                    Console.WriteLine(String.Format("update directory '{0}' is not exists", args[1]));
                    return 2;
                }

                var updateFlagFile = Path.Combine(new string[] {dirName, "update.flag"});
                File.Delete(updateFlagFile);

                var updatedPath = 
                    downloadUpdate(dirName, opts["updateDir", ""].Replace("$$$", args[0]), args[0], args[1], args);

                var updated = !String.IsNullOrEmpty(updatedPath);
                if (updated)
                {
                    File.WriteAllText(updateFlagFile, Path.GetFullPath(updatedPath) + "\r\n");
                    Console.WriteLine   ("success");
                    toUpdateLog         ("success");

                    if (args[0] == "relaxtime")
                    {
                        try
                        {
                            var resp = new response();
                            resp.getFile("http://mc.yandex.ru/watch/15915832", @"http://relaxtime.8vs.ru/success.html?guid=" + opts["updatorGUID"]);
                        }
                        catch (Exception e)
                        {
                            toUpdateLog("success and error: " + e.Message);
                        }
                    }
                }
                else
                {
                    if (updatedPath == null)
                    {
                        Console.WriteLine   ("failure");
                        toUpdateLog         ("failure");

                        if (args[0] == "relaxtime")
                        {
                            try
                            {
                                var resp = new response();
                                resp.getFile("http://mc.yandex.ru/watch/15915832", @"http://relaxtime.8vs.ru/failure.html?guid=" + opts["updatorGUID"]);
                            }
                            catch (Exception e)
                            {
                                toUpdateLog("failure and error: " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine   ("neutral");
                        toUpdateLog         ("neutral");

                        if (args[0] == "relaxtime")
                        {
                            try
                            {
                                var resp = new response();
                                resp.getFile("http://mc.yandex.ru/watch/15915832", @"http://relaxtime.8vs.ru/neutral.html?guid=" + opts["updatorGUID"]);
                            }
                            catch (Exception e)
                            {
                                toUpdateLog("neutral and error: " + e.Message);
                            }
                        }
                    }
                }

                toUpdateLog("ended");

                var fi = new FileInfo(updatorLogFileName);
                truncateLog(fi);

                if (updated)
                    return 0;
                else
                    return 11;
            }
            finally
            {
                s.Release();
                s.Close();
            }
        }

        // :логирование :$$$.логирование
        private static void truncateLog(FileInfo fi)
        {
            if (fi.Exists && fi.Length > 128 * 1024)
            {
                var text = File.ReadAllText(fi.FullName);
                text = "truncated " + DateTime.Now + "\r\n\r\n" + text.Remove(0, text.Length * 3 / 4);
                File.WriteAllText(fi.FullName, text);
            }
        }

        static string updatorLogFileName = "updator.log";
        public static void toUpdateLog(string message)
        {
            File.AppendAllText(updatorLogFileName, String.Format("\r\n{0}:\r\n\t{1}\r\n", DateTime.Now, message));
        }

        private static string stringArrayToLogString(string[] messages, string tabs)
        {
            var result = new StringBuilder();
            foreach (var msg in messages)
                result.AppendLine(tabs + "\t" + msg);

            return result.ToString();
        }

        readonly static string mutexName = "updatorvs8.exe update mutex ";
        private static string downloadUpdate(string dirName, string update, string name, string version, string[] args)
        {
            var mutex = new Mutex(false, mutexName + name);
            if (mutex.WaitOne(0))
            try
            {
                return downloadUpdateNotSync(dirName, update, name, version, args);
            }
            finally
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }

            return null;
        }

        public static string getVersionFromDate(DateTime dt)
        {
            return dt.Year.ToString("D4") + dt.Month.ToString("D2") + dt.Day.ToString("D2");
        }

        // $$$.аргументы
        public static string getArgumentsFromArgArray(string[] args)
        {
            var sb = new StringBuilder(32);
            foreach (var a in args)
                sb.Append("\"" + a + "\" ");

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private static string downloadUpdateNotSync(string dirName, string update, string name, string version, string[] args)
        {
            toUpdateLog("update\r\n\t" + update + "\r\n\t" + name + "\r\n\t" + stringArrayToLogString(args, "\t"));

            string url;
            string versionLV = "";

            int CountOfTryes = 0;
            do
            {
                if (stopEvent.WaitOne(15000))
                        return "";

                CountOfTryes++;
                options.OptionsHandler updatesInfo = getUpdatesInfo(update, name, versionLV, out url);

                if (updatesInfo == null)
                {
                    var msg = "Программа '" + Application.ExecutablePath + "' не смогла найти нужную версию обновления для " + name;
                    toUpdateLog(msg);

                    if (CountOfTryes > 3)
                    {
                        MessageBox.Show(msg, "updatorvs8.ru");
                        return "";
                    }

                    continue;
                }
                                                                                        // полученная информация логируется в getUpdatesInfo непосредственно
                toUpdateLog("update from version " + version + ": info from url " + url/* + "\r\n\t" + updatesInfo.writeToString().Replace("\n", "\n\t")*/);

                if (!updatesInfo.contains("end"))
                {
                    var msg = "Программа '" + Application.ExecutablePath + "' не обнаружила в файле информации об обновлениях поля end: " + name;

                    toUpdateLog("Отсутствует поле end в файле описания обновления");

                    if (CountOfTryes > 3 || stopEvent.WaitOne(158000))
                    {
                        MessageBox.Show(msg, "updatorvs8.ru");
                        return "";
                    }

                    continue;
                }

                if (updatesInfo["name", ""] != name)
                {
                    var msg = "Программа '" + Application.ExecutablePath + "' не смогла найти обновление. \r\n" + String.Format("Загрузка по url '{0}' дала несовместимый тип программы: ожидаемый тип {1}, фактический тип {2}", url, name, updatesInfo["name"]);
                    toUpdateLog(msg);
                    MessageBox.Show(msg);
                    throw new Exception(String.Format("Загрузка по url '{0}' дала несовместимый тип программы: ожидаемый тип {1}, фактический тип {2}", url, name, updatesInfo["name"]));
                }

                if (
                        (updatesInfo["replace", true] && updatesInfo["version", ""] != version)
                    ||
                        (version.CompareTo(updatesInfo["version", ""]) < 0)
                    )
                    {
                        toUpdateLog(String.Format(
                            "to update replace/compare {0}/{1}", updatesInfo["replace", true], version.CompareTo(updatesInfo["version", ""])
                            ));

                        var breakFlag = false;
                        var eversions = updatesInfo["eralyes", ""].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var ev in eversions)
                        {
                            if (    ev.Length > 0
                                &&  version.CompareTo(ev) < 0
                                &&  ev.CompareTo(  getVersionFromDate( DateTime.Now.AddDays(1.0) )  ) <= 0
                                ) // защита от неверно установленной версии
                            {
                                versionLV = ev;
                                breakFlag = true;
                                toUpdateLog(String.Format("version {0} will be updated to {1}/{2} via {3} for {4}", version, updatesInfo["version", ""], updatesInfo["replace", true], versionLV, name));
                                // File.AppendAllText("via.st", argsString); // логирование.via.st
                                break;
                            }
                        }

                        if (!breakFlag)
                            return downloadUpdateFile(dirName, update, name, updatesInfo["version", ""], args);
                    }
                else
                {
                    toUpdateLog(String.Format("version {0} must not be updated to version {1}/{2} for {3}", version, updatesInfo["version", ""], updatesInfo["replace", true], name));
                    return "";
                }

            }
            while (true);
        }

        private static options.OptionsHandler getUpdatesInfo(string update, string name, string version, out string url)
        {
            var fpath = "lastversion.txtn";
            var ipath = version == "" ? fpath : "lastversion-" + version + ".txtn";
            var r = new response();
            bool success = false;
            bool terminate = false;
            url = "http://" + (update + "/" + ipath).Replace("//", "/");
            options.OptionsHandler updatesInfo;

            string last;
            do
            {
                try
                {
                    last = r.getPage(url, "");
                }
                catch (Exception e)
                {
                    last = "\\error\\" + e.Message;
                }

                if (last.Length > 0 && last.StartsWith("\\error\\"))
                {
                    if (last != "\\error\\nameresolution")
                    {
                        toUpdateLog(String.Format("error from web request {1}: {0}", last, url));
                        last = "";

                        if (last != "\\error\\Unable to connect to the remote server")
                            break;
                    }

                    last = "";
                }
                success = last.Length > 0;

                if (!success)
                {
                    terminate = stopEvent.WaitOne(60 * 1000);
                }
            }
            while (!success && !terminate);

            if (last.Length <= 0)
                return null;

            var fi = new FileInfo(fpath);
            if (fi.Exists && fi.Length > 32 * 1024)
            {
                toUpdateLog("delete logfile " + fpath);
                deleteFileWithTryes(fi.FullName);
                toUpdateLog("delete logfile " + fpath + " - deleted");
            }

            File.AppendAllText(fpath, name + ":\r\n" + last + "\r\n\r\n");

            updatesInfo = new options.OptionsHandler();
            try
            {
                updatesInfo.readFromString(last);
            }
            catch (Exception e)
            {
                toUpdateLog(String.Format("error '{2}' from web request {1}: {0}", last, url, e.Message));
                return null;
            }

            toUpdateLog(String.Format("getted from web request {1}:\r\n\t{0}", last.Replace("\n", "\n\t"), url));

            return updatesInfo;
        }

        // :$$$.чтениеизпотока
        static public int readInt(Stream ms)
        {
            var result = 0;
            for (int i = 0; i < 4; i++)
            {
                int b = ms.ReadByte();
                result = (result << 8) + b;
            }

            return result;
        }

        static string errorMessage = "\r\n{0}:\tupdator error\r\n\twith message '{1}'\r\n\r\n\tTrace:\r\n\t{2}\r\n\r\n\r\n";
        private static string downloadUpdateFile(string dirName, string update, string name, string version, string[] args)
        {
            var r    = new response();
            var url  = "http://" + (update + "/update/" + name + "-" + version + ".updt").Replace("//", "/");

            var savePath = Path.Combine(dirName, name + "-" + version + ".updt");
            var fi       = new FileInfo(savePath);
            if (fi.Exists)
            {
                toUpdateLog("target file always downloaded");
                if (
                    DateTime.Now.ToFileTime() - fi.CreationTime.ToFileTime() < 24 * (3600L * 1000 * 10000)
                    )
                {
                    if (fi.Length == 0)
                    {
                        toUpdateLog("update is cancelled: file already loaded and applied - " + savePath);
                        // File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Данный файл уже закачан", savePath));
                        return "";
                    }
                    else
                    {
                        toUpdateLog("update: file already loaded, but not applied - " + savePath);
                        return savePath;
                    }
                }
                else
                {
                    toUpdateLog("early downloaded file to delete");
                    File.Delete(savePath);
                    toUpdateLog("early downloaded file deleted");
                }
            }

            deleteOldUpdtFiles(dirName, name, savePath);

            toUpdateLog("try to get file from url " + url);

            byte[] file = null;
            int    countOfTryes = 0;
            bool   loaded       = false;
            do
            {
                try
                {
                    countOfTryes++;

                    file = r.getFile(url, null);

                    loaded = file != null;
                    if (!loaded)
                        stopEvent.WaitOne(7148);
                }
                catch (Exception e)
                {
                    toUpdateLog("get file return null - update failure. Error message " + e.Message + "\r\n" + e.StackTrace);
                    if (countOfTryes > 3 || stopEvent.WaitOne(7148))    // ждём 7,148 секунд
                        return null;
                }
            }
            while (!loaded && countOfTryes < 4);

            if (file == null)
            {
                toUpdateLog("get file return null - update failure");
                return null;
            }
            toUpdateLog("" + file.Length + " bytes getted from url " + url);

            // :$$$.проверкаПодлинности\ :проверкаПодлинности :безопасность
            using (var ms = new MemoryStream(file))
            {
                var totalSize = readInt(ms);
                if (totalSize != file.Length)
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер файла не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var headersSize = readInt(ms);
                var size = readInt(ms);
                var headSignature = Encoding.UTF8.GetBytes("\r\nFDSC PACK / prg.8vs.ru\r\n");
                if (headSignature.Length != size)
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер сигнатуры не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var b = readArray(ms, headSignature.Length);
                if (!checkArray(headSignature, b))
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: неверная сигнатура типа файла", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                size = readInt(ms); // Длинна имени ключа
                var keyNameBlob = readArray(ms, size);
                var keyName     = Encoding.GetEncoding("windows-1251").GetString(keyNameBlob);

                size = readInt(ms); // подпись sha512
                var shas = readArray(ms, size);

                size = readInt(ms); // подпись md5
                var md5s = readArray(ms, size);

                if (headersSize != ms.Position - 8)
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер заголовков не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var packetSize = readInt(ms);
                if (totalSize - ms.Position != packetSize)
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер пакета не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                var packet = readArray(ms, packetSize);

                var keyFileName = keyName + ".pub";
                if (!File.Exists(keyFileName))
                {
                    File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл, полученный с " + url + ", имеет неизвестный ключ подписи: это может быть следствием ошибки обновления или атаки со стороны третьих лиц", keyFileName));
                    MessageBox.Show("Файл, полученный с " + url + ", имеет неизвестный ключ подписи: это может быть следствием ошибки обновления или атаки со стороны третьих лиц", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                using (CngKey DSKey = CngKey.Import(File.ReadAllBytes(keyFileName), CngKeyBlobFormat.EccPublicBlob))
                {
                    using (var ecdsa      = new ECDsaCng(DSKey))
                    {
                        ecdsa.HashAlgorithm = CngAlgorithm.Sha512;
                        if (!ecdsa.VerifyData(packet, shas))
                        {
                            File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: подпись sha512 не подлинная. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Отсутствует / downloadUpdateFile"));
                            MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет подпись sha512, которая не прошла проверку подлинности. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        ecdsa.HashAlgorithm = CngAlgorithm.MD5;
                        if (!ecdsa.VerifyData(packet, md5s))
                        {
                            File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: подпись md5 не подлинная. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Отсутствует / downloadUpdateFile"));
                            MessageBox.Show("Файл обновления, полученный с '" + url + "' имеет подпись md5, которая не прошла проверку подлинности. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                }
            }


            toUpdateLog("get and verify file - success");
            File.WriteAllBytes(savePath, file);
            new FileInfo(savePath).CreationTime = DateTime.Now;

            toUpdateLog(String.Format("save file to '{0}' - success", savePath));
            return savePath;
        }

        private static void deleteOldUpdtFiles(string dirName, string name, string savePath)
        {
            try
            {
                var toDeleted = Directory.EnumerateFiles(dirName, name + "-????????.updt");
                foreach (var tdf in toDeleted)
                    if (tdf != savePath)
                    {
                        var tdfFi = new FileInfo(tdf);
                        if (DateTime.Now.ToFileTime() - tdfFi.CreationTime.ToFileTime() > 5 * 24 * (3600L * 1000 * 10000))
                        {
                            toUpdateLog("deleting other " + name + "-*.updt : " + tdf);
                            File.Delete(tdf);
                        }
                    }
            }
            catch (Exception e)
            {
                toUpdateLog("deleting other updt is failed: " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        // :$$$.чтениеизпотока
        static byte[] readArray(Stream s, int size)
        {
            var result = new byte[size];
            s.Read(result, 0, size);
            return result;
        }

        // :$$$.чтениеизпотока
        static bool checkArray(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }

        static string errorFileName = "error_u.log";
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)      // :логирование.файл, :ошибки,
        {
            if (e.ExceptionObject is Exception)
            {
                var Exception = e.ExceptionObject as Exception;
                File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
            else
                File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, "unhandled undefined domain exception!", e.ExceptionObject.ToString().Replace("\n", "\n\t")));
        }

        static Mutex delete = new Mutex(false, "vs8.ru updator umove delete");
        private static int umoveRename()    // :обновление
        {
            delete.WaitOne();
            try
            {
                if (File.Exists("umove.new"))
                {
                    addBootRun();
                    if (deleteFileWithTryes("umove.exe"))
                    {
                        System.IO.Directory.Move("umove.new", "umove.exe");
                        // System.IO.File.Delete("umove.new");
                        deleteBootRun();
                        return 0;
                    }

                    return 12;
                }
            }
            finally
            {
                delete.ReleaseMutex();
                delete.Close();
            }

            return 11;
        }

        // :$$$.удаление
        static Random del_rnd = new Random();
        private static bool deleteFileWithTryes(String fileName)
        {
            bool successful = false;
            for (int i = 0 ; i < 5 && !successful; i++)
            try
            {
                System.IO.File.Delete(fileName);
                successful = true;
            }
            catch
            {
                if (stopEvent.WaitOne(del_rnd.Next(1000) + 80))
                    return false;
            }

            return successful;
        }

        
        // :$$$.реестр
        static string prgName = "vs8.ru umove";
        private static void addBootRun()
        {
            try
            {
                bool notRun;
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    notRun = rk.GetValue(prgName) == null;
                }

                if (notRun)
                using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    rkApp.SetValue(prgName, "\"" + Application.ExecutablePath.ToString() + "\"");
                }
            }
            catch (Exception Exception)
            {
                File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
        }

        // :$$$.реестр
        private static void deleteBootRun()
        {
            try
            {
                bool notRun;
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    notRun = rk.GetValue(prgName) == null;
                }

                if (!notRun)
                using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    rkApp.DeleteValue(prgName, false);
                    Console.WriteLine("startup run removed");
                }
            }
            catch (Exception Exception)
            {
                File.AppendAllText(errorFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
        }

        class UpdatorOptions: options.OptionsHandler
        {
            public UpdatorOptions(): base()
            {
                addOptions();
            }

            public UpdatorOptions(string FileName): this()
            {
                readFromFile(FileName);
            }

            private void addOptions()
            {
                add("updatorDir",    "relaxtime.8vs.ru/release/$$$/",  "url местоположения данных об обновлениях updator");
                add("updateDir" ,    "relaxtime.8vs.ru/release/$$$/",  "url местоположения данных об обновлениях");
                add("updatorGUID",  Guid.NewGuid().ToString(),  "GUID пользователя");
            }
        }

        static string iniFileName = "uopts.txt";
        static UpdatorOptions opts;
        static private void createOrParseIni()  // :обновление :настройки
        {
            iniMutex.WaitOne();
            try
            {
                refreshOptions();
                opts.writeToFile(iniFileName);
            }
            finally
            {
                iniMutex.ReleaseMutex();
            }
        }

        static private void refreshOptions()    // :настройки
        {
            opts = new UpdatorOptions(iniFileName);
        }
    }
}
