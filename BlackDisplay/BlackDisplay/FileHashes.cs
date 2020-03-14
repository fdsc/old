using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BlackDisplay
{
    public partial class FileHashes : Form
    {
        protected FileHashes()
        {
            InitializeComponent();
            hashBox.SelectedIndex = 3;
        }

        protected static volatile FileHashes form = null;
        public static FileHashes @new()
        {
            if (form != null)
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form.Activate();
                return form;
            }

            form = new FileHashes();
            return form;
        }

        public struct FileHash
        {
            public FileHash(string FileName, string hash)
            {
                this.FileName = FileName;
                this.hash     = hash;
            }

            public readonly string FileName;
            public readonly string hash;
        }

        struct fileNameAndStream
        {
            public string fileName;
            public string fileStream;
            public long   size;

            public fileNameAndStream(string fn, string fs, long size)
            {
                this.fileName   = fn;
                this.fileStream = fs;
                this.size       = size;
            }
        }

        long calcCount  = 0;
        long calcCountS = 0;
        long calcCountE = 0;
        long toCalc     = 0;
        long allFTime   = 0;
        long allCTime   = 0;
        long addErrors  = 0;

        volatile object sync = new Object();
        volatile string fName = null;
        volatile string dirName = null;
        volatile string hashType = null;
        volatile List<fileNameAndStream> list = null;
        volatile List<FileHash> lfh = null;
        private void button1_Click(object sender, EventArgs e)
        {
            if (list != null)
                return;

            toCalc = 0;
            addErrors  = 0;

            timer1.Enabled = true;

            list = new List<fileNameAndStream>(1024 * 1024);
            lfh  = new List<FileHash>(1024 * 1024);
            logBox.Text = "Добавление имён файлов в список к вычислению";
            logBox.AppendText("\r\n\t" + DateTime.Now.ToLongTimeString());
            var now = DateTime.Now;
            var prefix = this.prefixBox.Text + "-";
            if (prefix == "-")
                prefix = "hash-";
            prefix = Path.Combine(saveDirBox.Text, prefix + now.Year + now.Month.ToString("D2") + now.Day.ToString("D2") + "-" + now.ToFileTimeUtc());
            fName = new FileInfo(prefix).FullName;

            var dn = this.fileNameBox.Text;
            if (dn.Trim() == "" || dn.Trim() == "*")
                dn = "*";

            if (dn != "*")
                dirName = new DirectoryInfo(dn).FullName;
            else
                dirName = "*";

            hashType = hashBox.Text;

            File.AppendAllText(fName, now.Ticks + ":" + hashType + ": " + now.ToLocalTime() + "\r\n" + dirName + "\r\n");

            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    ProcessFileHashes();
                }
            );
        }

        void subDirsAddToList(string path)
        {
            if (toCalc == -1)
                return;

            try
            {
                // list.Add(path);
                AddFileStreams(path);

                var a = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);

                foreach (var f in a)
                {
                    AddFileStreams(f);
                }

                var ad = Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
                foreach (var d in ad)
                {
                    subDirsAddToList(d);
                }
            }
            catch (Exception ex)
            {
                addErrors++;
                File.AppendAllText(fName, path + "\r\n" + "error " + ex.Message + "\r\n");
                /*
                this.Invoke
                (
                    new VOID
                    (
                        delegate
                        {
                            logBox.AppendText("\r\nОшибка при построении списка " + ex.Message);
                            logBox.AppendText("\r\n" + path);
                        }
                    )
                );*/
            }
        }

        byte[] fileStreamInfo = new byte[1024*1024];
        Encoding utf16 =  Encoding.GetEncoding("utf-16");
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364404(v=vs.85).aspx
        private unsafe void AddFileStreams(string FileName)
        {
            try
            {
                var fnb = utf16.GetBytes(FileName);
                uint da = 0;
                if (Directory.Exists(FileName))
                    da = 0x02000000; // FILE_LIST_DIRECTORY = 1 FILE_FLAG_BACKUP_SEMANTICS = 0x02000000

                int bin = Form1.CreateFileW(fnb, /*0x80000000*/ 0, 0x00000002 | 0x00000001 | 0x00000004, 0, 3, da, 0); // GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE, OPEN_EXISTING = 3
                if (bin <= 0)
                {
                    throw new Exception("Error in AddFileStreams: CreateFileW GetLast error " + Form1.GetLastError() + " for file " + FileName);
                }

                try
                {
                    var success = false;
                    do
                    {
                        fixed (byte * fsi = fileStreamInfo)
                        {
                            if (Form1.GetFileInformationByHandleEx(bin, 7 /* FileStreamInfo */, (Form1.FILE_STREAM_INFO *) fsi, fileStreamInfo.Length) == 0)
                            {
                                var gle = Form1.GetLastError();

                                if (gle == 0x26 || gle == 87) // ERROR_HANDLE_EOF или "параметр задан неверно"
                                {
                                    if (da != 0x02000000)
                                        list.Add(new fileNameAndStream(FileName, "", 0));

                                    return;
                                }

                                if (gle == 24)
                                {
                                    if (fileStreamInfo.Length > 16*1024*1024)
                                    {
                                        
                                        throw new Exception("Error in AddFileStreams: GetFileInformationByHandleExW an alternate streams count is very large");
                                    }

                                    fileStreamInfo = new byte[fileStreamInfo.Length << 1];
                                }
                                else
                                    throw new Exception("Error in AddFileStreams: GetFileInformationByHandleExW GetLast error " + gle + "for file " + FileName);
                            }
                            else
                            {
                                Form1.FILE_STREAM_INFO * last = (Form1.FILE_STREAM_INFO *) fsi;
                                int offset = 0;
                                do
                                {
                                    var name = utf16.GetString(fileStreamInfo, offset + sizeof(Form1.FILE_STREAM_INFO), last->StreamNameLength);
                                    list.Add(new fileNameAndStream(FileName, name, last->StreamSize));

                                    /*if (name != "::$DATA")
                                    {
                                        fnb = utf16.GetBytes(FileName + name);
                                        int bin2 = Form1.CreateFileW(fnb, 0x80000000, 0, 0, 3, 0, 0);
                                        var gle = Form1.GetLastError();
                                        File.AppendAllText(fName, ":" + gle + "\r\n");
                                        Form1.CloseHandle(bin2);
                                    }*/

                                    offset = last->NextEntryOffset;
                                    last = (Form1.FILE_STREAM_INFO *) (fsi + offset);
                                }
                                while (offset > 0);

                                success = true;
                            }
                        }
                    }
                    while (!success);
                }
                finally
                {
                    Form1.CloseHandle(bin);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(fName, FileName + "\r\n" + "error " + ex.Message + "\r\n");
            }
        }

        private void ProcessFileHashes()
        {
            try
            {
                if (dirName == "*" || dirName.Trim().Length <= 0)
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        subDirsAddToList(drive.RootDirectory.FullName);
                    }
                }
                else
                {
                    subDirsAddToList(dirName);
                }
            }
            catch (Exception ex)
            {
                this.Invoke
                (
                    new VOID
                    (
                        delegate
                        {
                            lock (sync)
                            {
                                logBox.AppendText("\r\n");
                                logBox.AppendText("\r\nОшибка " + ex.Message);
                                logBox.AppendText("\r\n\t" + DateTime.Now.ToLongTimeString());
                            }
                        }
                    )
                );

                MessageBox.Show(ex.Message);

                toCalc = 0;
                list = null;

                return;
            }

            if (toCalc == -1)
                return;

            CalcFileHashes(list);
        }

        delegate void VOID();

        private void CalcFileHashes(List<fileNameAndStream> list)
        {
            toCalc = list.Count;

            this.Invoke
            (
                new VOID
                (
                    delegate
                    {
                        lock (sync)
                        {
                            logBox.AppendText("\r\nВсего добавлено " + list.Count);
                            if (addErrors > 0)
                                logBox.AppendText("\r\nОшибок при добавлении " + addErrors);

                            logBox.AppendText("\r\n\t" + DateTime.Now.ToLongTimeString());

                            logBox.AppendText("\r\n");
                            logBox.AppendText("\r\nНачинаются вычисления. Результат добавляется в файл " + fName);
                        }
                    }
                )
            );

            loopCnt = 0;
            int procCount = 0;

            for (var i = 0; i < 1/*Environment.ProcessorCount + 1*/; i++)
            {
                var procNum = i;
                System.Threading.ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        Interlocked.Increment(ref procCount);
                        try
                        {
                            int len;
                            if (hashType == "512")
                            {
                                len = keccak.SHA3.rNumbers[3] >> 3;
                            }
                            else
                            if (hashType == "384")
                                len = keccak.SHA3.rNumbers[2] >> 3;
                            else
                            if (hashType == "256")
                                len = keccak.SHA3.rNumbers[1] >> 3;
                            else
                                len = keccak.SHA3.rNumbers[0] >> 3;

                            var a = new keccak.SHA3(len + 1024);

                            string FileName = "";
                            fileNameAndStream fns;
                            int    lastNum = 0;
                            // FileInfo fi = null;
                            byte[] buff;
                            /*if (procNum > 0)
                                buff = new byte[64*1024*1024];
                            else*/
                            buff = new byte[len];
                            byte[] hash = new byte[64];

                            Start:
                            FileName = "";
                            long aft = 0, act = 0;
                            long n1;
                            try
                            {
                                long fl;
                                lock (sync)
                                {
                                    lastNum = list.Count - 1;

                                    /*File:
                                    fsi = null;
                                    fi  = null;
                                    di  = null;*/
                                    if (toCalc <= 0 || (lastNum < 0 && procNum > 0))
                                        return;

                                    //if (lastNum >= list.Count)
                                    if (list.Count <= 0)
                                    {
                                        while (procCount > 1)
                                            Monitor.Wait(sync);

                                        if (procNum == 0)
                                        {
                                            foreach (var afh in lfh)
                                            {
                                                File.AppendAllText(fName, afh.FileName + "\r\n" + afh.hash + "\r\n");
                                            }

                                            lfh.Clear();
                                        }
                                        return;
                                    }

                                    fns = list[lastNum];
                                    FileName = fns.fileName + fns.fileStream;
                                    n1 = DateTime.Now.Ticks;

                                    fl = fns.size;
                                    /*fi = new FileInfo(fns.fileName);

                                    fl = 0;
                                    if (fi.Exists)
                                    {
                                        fl = fi.Length;
                                        fsi = fi;
                                    }
                                    else
                                    {
                                        di = new DirectoryInfo(FileName);
                                        if (di.Exists)
                                        {
                                            fsi = di;
                                        }
                                    }*/
                                    aft += DateTime.Now.Ticks - n1;

                                    /*if (fl > buff.Length)
                                    // if (procNum > 0 || fl > 1024 * 1024 * 256)
                                    {
                                        // if (fl > 1024 * 1024 * 256)
                                        //lock (sync)
                                        {
                                            n1 = DateTime.Now.Ticks;
                                            File.AppendAllText(fName, FileName + "\r\n" + "skipped " + fl + "\r\n");
                                            aft += DateTime.Now.Ticks - n1;

                                            n1 = DateTime.Now.Ticks;
                                            list.RemoveAt(lastNum);
                                            act += DateTime.Now.Ticks - n1;
                                            calcCountS++;
                                        }

                                        lastNum--;
                                        goto File;
                                    }*/

                                    n1 = DateTime.Now.Ticks;
                                    list.RemoveAt(lastNum);
                                    act += DateTime.Now.Ticks - n1;
                                    /*
                                    if (fsi == null)
                                        goto Start;*/
                                }
                                /*
                                if (di != null)
                                {
                                    var dac = di.GetAccessControl();

                                    var sb  = new StringBuilder();

                                    // new FileIOPermission(FileIOPermissionAccess.Read, di.FullName).Demand();

                                    var dars = dac.GetAccessRules(true, false, WindowsIdentity.GetCurrent().GetType());
                                    foreach (var dar in dars)
                                    {
                                        
                                        sb.AppendLine();
                                    }

                                    di = null;
                                    goto Start;
                                }
                                */
                                // var fac = fi.GetAccessControl();

                                bool isInit = false;
                                int tmp = 0;
                                // byte[] fc = null;
                                // Эта блокировка нужна только для того, чтобы к диску потоки обращались последовательно
                                try
                                {
                                    lock (list)
                                    {
                                        // fc = File.ReadAllBytes(FileName);

                                        n1 = DateTime.Now.Ticks;

                                        var fnb = utf16.GetBytes(FileName);
                                        int bin = Form1.CreateFileW(fnb, 0x80000000, 0x00000002 | 0x00000001, 0, 3, 0x20000000 | 0x80000000, 0); // FILE_FLAG_NO_BUFFERING FILE_FLAG_WRITE_THROUGH
                                        if (bin <= 0)
                                            throw new Exception("CreateFileW in readFile GetLast error " + Form1.GetLastError() + " for file " + FileName);

                                        try
                                        {
                                            aft += DateTime.Now.Ticks - n1;

                                            do
                                            {
                                                n1 = DateTime.Now.Ticks;
                                                FormCrypt.ReadFile(bin, buff, buff.Length, out tmp, 0);
                                                if (hash == null && tmp <= 0)
                                                    throw new Exception("ReadFile in readFile GetLast error " + Form1.GetLastError() + " for file " + FileName);

                                                aft += DateTime.Now.Ticks - n1;


                                                n1 = DateTime.Now.Ticks;
                                                if (hashType == "512")
                                                {
                                                    hash = a.getHash512(buff, tmp, isInit, tmp != buff.Length, hash);
                                                }
                                                else
                                                if (hashType == "384")
                                                    hash = a.getHash384(buff, tmp, isInit, tmp != buff.Length, hash);
                                                else
                                                if (hashType == "256")
                                                    hash = a.getHash256(buff, tmp, isInit, tmp != buff.Length, hash);
                                                else
                                                    hash = a.getHash224(buff, tmp, isInit, tmp != buff.Length, hash);

                                                isInit = true;
                                                act += DateTime.Now.Ticks - n1;
                                            }
                                            while (tmp > 0 && tmp == buff.Length);
                                        }
                                        finally
                                        {
                                            Form1.CloseHandle(bin);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    lock (sync)
                                    {
                                        calcCountE++;
                                    }

                                    lock (sync)
                                    {
                                        try
                                        {
                                            File.AppendAllText(fName, FileName + "\r\n" + "error " + ex.Message + "\r\n");
                                        }
                                        catch
                                        {}
                                    }

                                    goto Start;
                                }

                                n1 = DateTime.Now.Ticks;
                                var fh = new FileHash(FileName, Convert.ToBase64String(hash));

                                lock (sync)
                                {
                                    allFTime += aft;

                                    lfh.Add(fh);
                                    act += DateTime.Now.Ticks - n1;
                                    allCTime += act;

                                    calcCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (sync)
                                {
                                    calcCountE++;
                                }

                                lock (sync)
                                {
                                    try
                                    {
                                        File.AppendAllText(fName, FileName + "\r\n" + "error " + ex.Message + "\r\n");
                                    }
                                    catch
                                    {}
                                }

                                try
                                {
                                    this.Invoke
                                    (
                                        new VOID
                                        (
                                            delegate
                                            {
                                                lock (sync)
                                                {
                                                    logBox.AppendText("\r\nОшибка " + ex.Message);
                                                    logBox.AppendText("\r\nОшибка " + FileName);
                                                }
                                            }
                                        )
                                    );
                                }
                                catch
                                {}
                            }

                            goto Start;
                        }
                        finally
                        {
                            Interlocked.Decrement(ref procCount);
                            lock (sync)
                                Monitor.Pulse(sync);
                        }
                    }
                );
            }
        }

        int loopCnt = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (list == null)
                return;

            loopCnt++;
            lock (sync)
            {
                if (toCalc == 0 && loopCnt > 60)
                {
                    logBox.AppendText("\r\nПока добавлено файлов в список " + list.Count);
                    logBox.AppendText("\r\n\t" + DateTime.Now.ToLongTimeString());

                    loopCnt = 0;
                    return;
                }
                else
                if (toCalc == 0)
                    return;

                if (toCalc <= 0)
                {
                    if (toCalc == -1)
                        logBox.AppendText("\r\n\r\nЗавершено пользователем (возможно, некоторые потоки ещё в работе)");

                    timer1.Enabled = false;
                    list = null;

                    loopCnt = 0;
                    calcCount  = 0;
                    calcCountS = 0;
                    calcCountE = 0;
                    toCalc     = -1;

                    return;
                }

                if (calcCount + calcCountE + calcCountS >= toCalc || calcCount + calcCountE + calcCountS < toCalc && loopCnt > 60)
                {
                    logBox.AppendText("\r\n");
                    logBox.AppendText("Вычислено " + calcCount);
                    if (calcCountE > 0)
                        logBox.AppendText("\r\nОшибок " + calcCountE);
                    if (calcCountS > 0)
                        logBox.AppendText("\r\nПропущено больших (>256 Mb) файлов " + calcCountS);

                    if (allFTime > 0)
                    logBox.AppendText("\r\nВремя вычислений/диска, мс (кратность) " + (allCTime/10000) + " / " + (allFTime/10000) + " (" + ((allCTime/10000*100) / (allFTime/10000)) + "%)");

                    logBox.AppendText("\r\n");
                    logBox.AppendText("Всего " + ((calcCount + calcCountE + calcCountS)*100 / toCalc) + "%");


                    logBox.AppendText("\r\n\t" + DateTime.Now.ToLongTimeString());

                    loopCnt = 0;
                }

                if (calcCount + calcCountE + calcCountS >= toCalc)
                {
                    timer1.Enabled = false;
                    list = null;
                    loopCnt = 0;
                    calcCount  = 0;
                    calcCountS = 0;
                    calcCountE = 0;
                    toCalc     = 0;

                    logBox.AppendText("\r\n");
                    logBox.AppendText("\r\n");
                    logBox.AppendText("Вычисления закончены. Результат в файле\r\n");
                    logBox.AppendText(fName);

                    fName = null;
                }
            }
        }

        private void hashBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            toCalc = -1;
        }

        private void FileHashes_FormClosed(object sender, FormClosedEventArgs e)
        {
            form = null;
        }
    }
}
