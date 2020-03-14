using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO.Compression;

namespace move
{
    class move
    {
        static System.Threading.EventWaitHandle main = new System.Threading.EventWaitHandle(false, EventResetMode.ManualReset, "vs8.ru updateEvent");
        static Mutex mainMutexForLog = new Mutex(false, "vs8.ru umove mainMutexForLog");

        private static void writeHelp()
        {
            Console.WriteLine("umove.exe path_to_unarc semaphore_name s_count execute_path");
            Console.WriteLine("for example: umove.exe D:/rtbd/update.flag \"Relax Time Black Display Main Mutex\" 1 D:/rtbd/BlackDisplay.exe \"opt_params\"");
            Console.WriteLine("for example: umove.exe D:/rtbd/uup.flag \"vs8.ru updator semaphore\" 50 \"\"");
        }

        static int Main(string[] args)  // :обновление
        {
            AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

            repairUpdateExe();
            var repair = args.Length == 0 || (args.Length == 1 && args[0] == "-repair");
            if (Repair(repair) && repair)
            {
                Console.WriteLine("repair started - succes");
                return 40;
            }

            if (  args.Length > 5 || args.Length < 4  )
            {
                writeHelp();
                return 1;
            }

            mainMutexForLog.WaitOne();
            addBootRun();

            Semaphore s                  = null; //new System.Threading.Semaphore(50, 50, "vs8.ru updator semaphore");
            Mutex     syncSemaphoreMutex = null;
            int       sCount = 0, rCount = 0, mCount = 0;
            bool      updatorUpdate = false;
            try
            {
                if (args[1] == "vs8.ru updator semaphore")
                {
                    main.Set();
                    sCount = 50;
                    updatorUpdate = true;
                }
                else
                    sCount = Int32.Parse(args[2]);


                if (args[1].Length > 0)
                {
                    s                  = new Semaphore(sCount, sCount, args[1]);
                    syncSemaphoreMutex = new Mutex    (false, args[1] + " umove sync mutex");
                }

                if (syncSemaphoreMutex != null)
                {
                    if (!syncSemaphoreMutex.WaitOne(0))
                        return 32;

                    mCount = 1;
                }

                var RepairInfo = setRepairInfo(args);
                if (s != null)
                {
                    rCount = blockSemaphore(s, sCount);
                }

                var returned = umoveProcess(args);

                if (returned == 0)
                    Console.WriteLine("success");
                else
                    Console.WriteLine("failure with code " + returned);
                File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Исполнено с кодом " + returned, getArgumentsFromArgArray(args)));

                if (updatorUpdate)
                {
                    File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Стартуем ./updatorvs8.exe -umove", getArgumentsFromArgArray(args)));
                    Process.Start("updatorvs8.exe", "-umove");
                }

                if (args[3].Length > 0)
                if (args.Length == 4)
                {
                    File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Стартуем " + args[3], getArgumentsFromArgArray(args)));
                    Process.Start(args[3]);
                }
                else
                {
                    File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Стартуем " + args[3] + " " + args[4], getArgumentsFromArgArray(args)));
                    Process.Start(args[3], args[4]);
                }
                ClearRepairInfo(args, RepairInfo);

                deleteBootRun();
                File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Исполнено и удалено из аварийного восстановления", getArgumentsFromArgArray(args)));

                truncateLog(new FileInfo(errorLogFileName));

                return returned;
            }
            finally
            {
                if (s != null)
                {
                    releaseSemaphore(s, rCount);
                    s.Close();
                }

                if (syncSemaphoreMutex != null)
                {
                    if (mCount > 0)
                        syncSemaphoreMutex.ReleaseMutex();
                    syncSemaphoreMutex.Close();
                }

                main.Reset();
                main.Close();
                mainMutexForLog.ReleaseMutex();
                mainMutexForLog.Close();
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

        static Mutex repairFileMutex = null; //new Mutex(false, "updatorvs8.ru repairFileMutex");
        static string repairFile = "repair.st";
        private static void ClearRepairInfo(string[] args, string repairInfo)
        {
            string text;
            repairFileMutex = new Mutex(false, "updatorvs8.ru repairFileMutex");
            repairFileMutex.WaitOne();
            try
            {
                text = File.ReadAllText(repairFile);

                int count = 0;
                int k;
                do
                {
                    k = text.IndexOf(repairInfo);
                    if (k < 0)
                    {
                        if (count == 0)
                            File.AppendAllText( errorLogFileName, 
                                                String.Format("\r\n{0}:\tumove error\r\n\tВ файле repair.st не был найден ключ {1}\r\n", DateTime.Now, repairInfo)
                                              );
                    }
                    else
                    {
                        text = text.Remove(k, repairInfo.Length + 2);
                    }
                    count++;
                }
                while (k >= 0);

                File.WriteAllText(repairFile, text);
            }
            finally
            {
                repairFileMutex.ReleaseMutex();
                repairFileMutex.Close();
            }
        }


        private static string setRepairInfo(string[] args)
        {
            repairFileMutex = new Mutex(false, "updatorvs8.ru repairFileMutex");
            repairFileMutex.WaitOne();

            string result    = "";
            string arguments = "";
            try
            {
                arguments = getArgumentsFromArgArray(args);

                File.AppendAllText(repairFile, arguments + "\r\n");
                result = arguments;
            }
            finally
            {
                repairFileMutex.ReleaseMutex();
                repairFileMutex.Close();
            }

            File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Записано в аварийное восстановление на случай сбоя", arguments));

            return result;
        }

        // $$$.аргументы
        public static string getArgumentsFromArgArray(string[] args)
        {
            var sb = new StringBuilder(32);
            foreach (var a in args)
                sb.Append("\"" + a + "\" ");

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private static bool Repair(bool repair)
        {
            bool result = false;
            if (repair)
            {
                var fi = new FileInfo(repairFile);
                if (fi.Exists && fi.Length > 0)
                {
                    result = true;
                    
                    repairFileMutex = new Mutex(false, "updatorvs8.ru repairFileMutex");
                    repairFileMutex.WaitOne();
                    
                    string [] operationsStrings;
                    try
                    {
                        operationsStrings = File.ReadAllLines(repairFile);
                    }
                    finally
                    {
                        repairFileMutex.ReleaseMutex();
                        repairFileMutex.Close();
                    }

                    var operations        = new List<String>(operationsStrings);
                    operations.Sort();

                    var lastStr = "";
                    for (int i = 0; i < operations.Count; i++)
                    {
                        var curStr = operations[i];

                        if (curStr == lastStr)
                            continue;

                        try
                        {
                            Process.Start("umove.exe", curStr);
                        }
                        catch (Exception e)
                        {
                            File.AppendAllText( errorLogFileName, 
                                                String.Format("\r\n{0}:\tumove error\r\n\tОшибка при запуске команды {0} из repair.st: {1}\r\n{2}\r\n\r\n",
                                                                DateTime.Now, curStr, e.Message, e.StackTrace)
                                              );
                        }

                        lastStr = curStr;
                    }
                }
            }

            return result;
        }


        static Mutex deleteMutex; // = new Mutex(false, "vs8.ru repair updator mutex");
        private static void repairUpdateExe()
        {
            deleteMutex = new Mutex(false, "vs8.ru repair updator mutex");
            deleteMutex.WaitOne();
            main.Set();
            try
            {
                if (File.Exists("updatorvs8.new"))
                {
                    if (File.Exists("updatorvs8.exe"))
                    {
                        if (!deleteFileWithTryes("updatorvs8.exe"))
                            return;
                    }

                    File.Move("updatorvs8.new", "updatorvs8.exe");
                    // File.Delete("updatorvs8.new");
                }
            }
            finally
            {
                // deleteMutex.ReleaseMutex();
                main.Reset();

                deleteMutex.ReleaseMutex();
                deleteMutex.Close();
            }
        }

        // :$$$.удаление\
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
                System.Threading.Thread.Sleep(del_rnd.Next(1000) + 80);
            }

            return successful;
        }

        // :$$$.семафор
        private static int blockSemaphore(System.Threading.Semaphore s, int p)
        {
            int i = 0;
            try
            {
                // int count = p;
                for (; i < p; i++)
                {
                    // Console.WriteLine("wait");
                    s.WaitOne();
                    // Console.WriteLine("waited " + --count);
                }
                return i;
            }
            catch
            {
                return i;
            }
        }

        // :$$$.отдатьсемафор
        private static bool releaseSemaphore(System.Threading.Semaphore s, int p)
        {
            for (int i = 0; i < p; i++)
            {
                s.Release();
            }
            return true;
        }

        static string errorLogFileName = "error_um.log";
        static string errorMessage     = "\r\n{0}:\tumove error\r\n\twith message '{1}'\r\n\r\n\tTrace:\r\n\t{2}\r\n\r\n\r\n";
        static string   msgMessage     = "\r\n{0}:\tumove message\r\n\twith message '{1}'\r\n\r\n\tData:\r\n\t{2}\r\n\r\n\r\n";
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)  // :логирование.файл, :ошибки
        {
            if (e.ExceptionObject is Exception)
            {
                var Exception = e.ExceptionObject as Exception;
                File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
            else
                File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "unhandled undefined domain exception!", e.ExceptionObject.ToString().Replace("\n", "\n\t")));
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
                File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
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
                File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, Exception.Message, Exception.StackTrace.Replace("\n", "\n\t")));
            }
        }

        private static int umoveProcess(string[] args)
        {
            string ufName = args[0];
            string pfName = null;

            if (!File.Exists(ufName))
                    return 41;

            var fName = File.ReadAllLines(ufName);
            try
            {
                if (fName.Length < 1)
                    return 42;

                if (!File.Exists(fName[0]))
                    return 43;

                var fi = new FileInfo(fName[0]);
                if (fi.Length == 0)
                {
                    return 44;
                }

                pfName = fName[0];
                byte[] packet = verify(pfName);
                if (packet == null)
                {
                    // File.Delete(fName[0]);
                    File.WriteAllText(pfName, "");    // Сбрасываем плохое содержимое, но не удаляем файл, иначе произойдёт новая попытка докачки
                    return 51;
                }

                return unpack(packet, Path.GetDirectoryName(pfName));
            }
            finally
            {
                if (pfName != null)
                    // File.Delete(pfName);
                    File.WriteAllText(pfName, "");  // Сбрасываем содержимое, но не удаляем файл, чтобы было ясно, что обновление получено

                File.Delete(ufName);
            }
        }

        class NewFile: IComparable<NewFile>
        {
            public readonly string error;
            public readonly int    errorCode;
            public readonly bool   toDelete;
            public readonly bool   isKey;
            public readonly int    Priority = -1;
            public readonly byte[] data;
            public readonly string Name;
            public readonly string exeRenamed;
            public NewFile(MemoryStream ms, string dirName)
            {
                int zero = readInt(ms);
                if (zero != 0)
                {
                    error       = "Неверный формат: отсутствует ноль в разархивированном объявлении файла";
                    errorCode   = 1;
                    return;
                }

                int readedSize; string errMsg;
                var fileName = readArray(ms, out readedSize, out errMsg);
                if (fileName == null || fileName.Length <= 0)
                {
                    error       = "Неверный формат. Нет имени файла: " + errMsg;
                    errorCode   = 2;
                    return;
                }

                data = readArray(ms, out readedSize, out errMsg);

                if (data == null)
                {
                    error       = "Неверный формат. Не удалось прочитать данные файла: " + errMsg;
                    errorCode   = 3;
                    return;
                }
                toDelete = data.Length == 0;

                var hash = readArray(ms, out readedSize, out errMsg);
                var chh  = MD160(data);
                if (hash == null)
                {
                    error       = "Не удалось прочитать данные контрольной суммы MD160 файла: " + errMsg;
                    errorCode   = 4;
                    return;
                }

                if (!checkArray(hash, chh))
                {
                    error       = "Нарушенная контрольная сумма MD160 внутреннего файла.";
                    errorCode   = 11;
                    return;
                }

                Name = Encoding.GetEncoding("windows-1251").GetString(fileName);
                Name = Path.Combine(dirName, Name);
                if (Name.EndsWith(".exe"))
                {
                    exeRenamed = Name.Substring(0, Name.Length - 4) + ".new";
                    Priority   = 0;
                }
                else
                    exeRenamed = null;

                if (Priority == -1)
                    Priority = 256;

                if (Name.EndsWith(".pub"))
                {
                    Priority = Int32.MaxValue;
                    isKey    = true;
                }

                if (Name.EndsWith(".private"))
                {
                    data = new byte[0];             // Не позволяем записывать приватные ключи, если они по ошибке попали в архив
                                                    // При этом сам файл мы записываем (чтобы было видно, что приватный ключ есть в архиве [для того, чтобы можно было быстро это понять])
                }
            }

            public int CompareTo(NewFile y)
            {
                if (Priority > y.Priority)
                    return 1;

                if (Priority < y.Priority)
                    return -1;

                return Name.CompareTo(y.Name);
            }
        }

        private static int unpack(byte[] packet, string dirName)
        {
            byte[] zipPacket;
            byte[] ipacket = null;
            // Формат пакета см. addFileToMs и иже с ними
            // Суммы SHA512, MD160, сам зипованный пакет
            using (var ms = new MemoryStream(packet))
            {
                int     ReadedSize;
                string  error;

                var SHA512summ = readArray(ms, out ReadedSize, out error);
                if (SHA512summ == null || SHA512summ.Length <= 0)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: отсутствует сумма SHA512", error));
                    MessageBox.Show("Файл обновления имеет неверный формат: отсутствует сумма SHA512", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 52;
                }

                var MD160summ = readArray(ms, out ReadedSize, out error);
                if (MD160summ == null || MD160summ.Length <= 0)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: отсутствует сумма MD160", error));
                    MessageBox.Show("Файл обновления имеет неверный формат: отсутствует сумма MD160", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 52;
                }

                int dataSize    = readInt(ms);
                    zipPacket   = readArray(ms, (int) (ms.Length - ms.Position));

                if ( zipPacket != null && zipPacket.Length > 0 )
                using ( var zip = new MemoryStream(zipPacket) )
                {
                    using (var zs = new GZipStream(zip, CompressionMode.Decompress))
                    {
                        if (dataSize <= 0)
                        {
                            File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер заархивированных данных неположителен", "dataSize = " + dataSize));
                            MessageBox.Show("Файл обновления имеет неверный формат: размер заархивированных данных неположителен", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return 53;
                        }

                        ipacket = readArray(zs, dataSize);
                    }
                }


                if (ipacket == null || ipacket.Length <= 0)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: отсутствует содержимое пакета", error));
                    MessageBox.Show("Файл обновления имеет неверный формат: отсутствует содержимое пакета", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 52;
                }

                var SHA512check = sha512(zipPacket);
                var MD160check  = MD160 (zipPacket);

                if (!checkArray(SHA512check, SHA512summ))
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Нарушена контрольная сумма SHA512 архива", error));
                    MessageBox.Show("Файл обновления имеет неверный формат или подделан неизвестным злоумышленником: нарушена контрольная сумма SHA512 архива", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 53;
                }

                if (!checkArray(MD160check, MD160summ))
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Нарушена контрольная сумма MD160 архива", error));
                    MessageBox.Show("Файл обновления имеет неверный формат или подделан неизвестным злоумышленником: нарушена контрольная сумма MD160 архиваа", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 53;
                }

                File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Проверка целостности контрольных сумм архива пройдена", ""));
            }


            List<NewFile> files = new List<NewFile>(32);
            using (var ms = new MemoryStream(ipacket))
            {
                do
                {
                    var newFile = new NewFile(ms, dirName);
                    if (newFile.error != null)
                    {
                        File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл обновления имеет неверный формат: не удалось считать архивированный файл", newFile.error));
                        MessageBox.Show("Файл обновления имеет неверный формат: не удалось считать архивированный файл - " + newFile.error, "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 53;
                    }

                    files.Add(newFile);
                }
                while (ms.Length > ms.Position);
            }

            files.Sort();

            errorsCount = 0;
            deleteOldFiles(dirName, files);
            createNewFiles(dirName, files);
            var upflag = renameNewFiles(dirName);

            if (upflag)
                repairUpdateExe();

            if (errorsCount > 0)
                MessageBox.Show("Во время обновления " + dirName + " произошли ошибки (игнорированы при обновлении).\r\nСмотрите " + Path.GetFullPath(errorLogFileName));

            return 0;
        }

        static int errorsCount = 0;
        private static bool renameNewFiles(string dirName)
        {
            // Переименовываем все exe-файлы обратно
            var exefiles = Directory.EnumerateFiles(dirName, "*.new");
            var upflag = false;
            foreach (var exef in exefiles)
            {
                var fsName = Path.GetFileName(exef);
                if (fsName == "updatorvs8.new" || fsName == "umove.new")
                {
                    upflag = true;
                    continue;
                }

                if (exef.EndsWith(".new"))
                {
                    var newFName = exef.Substring(0, exef.Length - 4) + ".exe";
                    try
                    {
                        File.Delete(newFName);          // На всяк пожарный удаляем файл, в который переименовываем, если он вдруг остался с предыдущих операций
                        File.Copy(exef, newFName);
                        deleteFileWithTryes(exef);
                    }
                    catch (Exception e)
                    {
                        errorsCount++;
                        File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Не удалось переименовать файл " + newFName + " в *.exe", e.Message));
                    }
                }
                else
                    throw new Exception("Фатальная ошибка: среди new-файлов оказался не new-файл");
            }
            return upflag;
        }

        private static void createNewFiles(string dirName, List<NewFile> files)
        {
            // Создаём новые файлы
            foreach (var f in files)
            {
                if (f.toDelete)
                    continue;

                var fName = String.IsNullOrEmpty(f.exeRenamed) ? f.Name : f.exeRenamed;
                var fi = new FileInfo(Path.Combine(dirName, fName));

                try
                {
                    if (!fi.Directory.Exists)
                    {
                        try
                        {
                            createNewDirPath(fi);
                        }
                        catch (Exception e)
                        {
                            errorsCount++;
                            File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Не удалось создать новую директорию: " + fi.FullName, e.Message + "\r\n" + e.StackTrace));
                        }
                    }

                    if (f.isKey)
                    {
                        if (!File.Exists(f.Name))
                            File.WriteAllBytes(fName, f.data);
                        else
                            File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Ключевой файл штатно пропущен для записи", f.Name));
                    }
                    else
                    {
                        File.WriteAllBytes(fName, f.data);
                    }
                }
                catch (Exception e)
                {
                    errorsCount++;
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Не удалось создать файл " + fName, e.Message));
                }
            }
        }

        private static void createNewDirPath(FileInfo fi)
        {
            var l = new List<DirectoryInfo>(2);
            var c = fi.Directory;
            do
            {
                l.Add(c);
                c = c.Parent;
            }
            while (!c.Exists);

            for (int i = l.Count - 1; i >= 0; i--)
                l[i].Create();
        }

        private static void deleteOldFiles(string dirName, List<NewFile> files)
        {
            // Временно переименовываем все exe-файлы, чтобы препятствовать их запуску
            var exefiles = Directory.EnumerateFiles(dirName, "*.exe");
            foreach (var exef in exefiles)
            {
                var fsName = Path.GetFileName(exef);
                if (fsName == "updatorvs8.new" || fsName == "umove.new")
                    continue;

                if (exef.EndsWith(".exe"))
                {
                    try
                    {
                        var newFName = exef.Substring(0, exef.Length - 4) + ".new";

                        File.Delete(newFName);          // На всяк пожарный удаляем файл, в который переименовываем, если он вдруг остался с предыдущих операций
                        File.Copy(exef, newFName);
                        deleteFileWithTryes(exef);
                    }
                    catch (Exception e)
                    {
                        errorsCount++;
                        File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Не удалось переименовать файл " + exef + " в *.new", e.Message));
                    }
                }
                else
                    throw new Exception("Фатальная ошибка: среди exe-файлов оказался не exe-файл");
            }

            // Удаляем файлы, подлежащие обновлению
            foreach (var f in files)
            {
                var fName = String.IsNullOrEmpty(f.exeRenamed) ? f.Name : f.exeRenamed;
                try
                {
                    var fi = new FileInfo(Path.Combine(dirName, fName));
                    if (f.isKey)
                    {
                        if (f.toDelete && fi.Exists)
                            deleteFileWithTryes(fi.FullName);
                    }
                    else
                    {
                        if (fi.Exists)
                            fi.Delete();    // проверка на существование делается, т.к. иначе если path не существует, будет exception
                    }
                }
                catch (Exception e)
                {
                    errorsCount++;
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Не удалось удалить файл " + fName, e.Message + "\r\n" + e.StackTrace));
                }
            }
        }

        public static byte[] verify(string fName)
        {
            var    file   = File.ReadAllBytes(fName);
            byte[] packet = null;

            // :$$$.проверкаПодлинности\ :проверкаПодлинности :безопасность
            using (var ms = new MemoryStream(file))
            {
                var totalSize = readInt(ms);
                if (totalSize != file.Length)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер файла не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var headersSize = readInt(ms);
                var size = readInt(ms);
                var headSignature = Encoding.UTF8.GetBytes("\r\nFDSC PACK / prg.8vs.ru\r\n");
                if (headSignature.Length != size)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер сигнатуры не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var b = readArray(ms, headSignature.Length);
                if (!checkArray(headSignature, b))
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: неверная сигнатура типа файла", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер заголовков не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var packetSize = readInt(ms);
                if (totalSize - ms.Position != packetSize)
                {
                    File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: размер пакета не совпадает с заявленным", "Отсутствует / downloadUpdateFile"));
                    MessageBox.Show("Файл обновления имеет неверный формат.", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                packet = readArray(ms, packetSize);

                using (CngKey DSKey = CngKey.Import(File.ReadAllBytes(keyName + ".pub"), CngKeyBlobFormat.EccPublicBlob))
                {
                    using (var ecdsa      = new ECDsaCng(DSKey))
                    {
                        ecdsa.HashAlgorithm = CngAlgorithm.Sha512;
                        if (!ecdsa.VerifyData(packet, shas))
                        {
                            File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: подпись sha512 не подлинная. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Отсутствует / downloadUpdateFile"));
                            MessageBox.Show("Файл обновления имеет подпись sha512, которая не прошла проверку подлинности. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }

                        ecdsa.HashAlgorithm = CngAlgorithm.MD5;
                        if (!ecdsa.VerifyData(packet, md5s))
                        {
                            File.AppendAllText(errorLogFileName, String.Format(errorMessage, DateTime.Now, "Файл имеет неверный формат: подпись md5 не подлинная. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Отсутствует / downloadUpdateFile"));
                            MessageBox.Show("Файл обновления имеет подпись md5, которая не прошла проверку подлинности. Это может быть ошибка обновления или неуспешная попытка атаки на обновляемый компьютер со стороны третьих лиц", "Ошибка обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                }
            }

            File.AppendAllText(errorLogFileName, String.Format(msgMessage, DateTime.Now, "Проверка подлинности пройдена", "packet/file length: " + packet.Length + "/" + file.Length));

            return packet;
        }

        // :$$$.контрольнаясумма
        public static byte[] MD160(byte[] toHash)
        {
            using (RIPEMD160 myRIPEMD160 = RIPEMD160Managed.Create())
            {
                return myRIPEMD160.ComputeHash(toHash);
            }
        }

        // :$$$.контрольнаясумма
        public static byte[] sha512(byte[] toHash)
        {
            using (SHA512 sha = new SHA512Managed())
            {
                return sha.ComputeHash(toHash);
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

        // :$$$.чтениеизпотока
        static byte[] readArray(Stream s, out int readedSize, out string error)
        {
            error = "нет ошибки";
            int size = readInt(s);
            readedSize = 4;
            if (size < 0)
            {
                error      = "Количество байт к чтению отрицательно";
                return null;
            }

            if (size == 0)
            {
                return new byte[0];
            }

            try
            {
                var result  = new byte[size];
                readedSize += s.Read(result, 0, size);
                return result;
            }
            catch (Exception e)
            {
                error = e.Message + "\r\n" + e.StackTrace;
                return null;
            }
        }
    }
}
