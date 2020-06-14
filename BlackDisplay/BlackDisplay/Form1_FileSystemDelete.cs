using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Principal;
using System.IO;
using System.Security.Permissions;
using Tests;
using options;
using keccak;
using System.Threading;
using System.Collections.Concurrent;

namespace BlackDisplay
{
    partial class Form1
    {
        public void удалитьДиректорию(int regime, string dirName, DoDataSanitizationObjectPreStep ddso)
        {
            if (regime == 4)
            {
                var formDelete = new FormDelete(ddso);
                formDelete.ShowDialog();
            }

            bool flag = false;
            try
            {
                DoDataSanitizationObjectPreStep v = new DoDataSanitizationObjectPreStep();
                v.doNotPreStep = true;
                v.doNotDelete  = true;

                удалитьДиректориюПодметод(dirName, v, 2, false, 65536);

                ddso.success = false;
                ddso.doNotPreStep = regime == 2;
                ddso.preStepPassed = false;

                var dnd = ddso.doNotDelete;
                ddso.doNotDelete = ddso.doNotPreStep ? dnd : true;
                var t1 = DateTime.Now;
                удалитьДиректориюПодметод(dirName, ddso, 2, regime == 2);
                var ts = DateTime.Now.Subtract(t1);
                ddso.ts = (float) ts.TotalSeconds;
                if (regime == 1)
                    ddso.ts *= 3;
                if (regime == 0)
                    ddso.ts *= (int) (3 + 35 + 8 * 3.5);
                if (regime == 3)
                    ddso.ts *= 512;
                if (regime == 4)
                    ddso.ts *= ddso.countToWrite;

                if (!ddso.preSuccess || ddso.doTerminate)
                {
                    ddso.success = false;
                    ddso.exited  = true;
                    if (ddso.terminatedByUser)
                        MessageBox.Show("Удаление прекращено пользователем" + ddso.errorMessage, "Удаление прекращено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Удаление не удалось.\r\nВозможно какая-то папка открыта в проводнике или какой-то файл используется другой программой." + ddso.errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                flag = ddso.success;

                ddso.success = false;
                ddso.doNotDelete = dnd;
                ddso.preStepPassed = true;
                ddso.prepercent = 100f;

                if (regime != 2)
                    удалитьДиректориюПодметод(dirName, ddso, regime, true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при удалении\r\n" + e.Message + ddso.errorMessage, "Ошибка при удалении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ddso.success = false;
            }

            if (!ddso.success)
            {
                if (flag)
                    MessageBox.Show("Удаление не удалось. Перезатирание и переименование файлов выполнено по крайней мере один раз для каждого файла.\r\nВозможно, некоторые файлы были удалены с полным выполнением цикла удаления (включая удаление из файловой системы). Для остальных запустите процедуру повторно.\r\nЕсли ошибка повторяется, попробуйте отключить сканирование по доступу антивирусного средства либо добавить удаляемую папку в исключения антивируса" + ddso.errorMessage);
                else
                    MessageBox.Show("Удаление не удалось" + ddso.errorMessage);
                return;
            }
        }

        private static void удалитьДиректориюПодметод(string dirName, DoDataSanitizationObject ddso, int regime, bool endOfProcess, int onlyFirstBytes = -1)
        {
            var sha = new SHA3(1024);
            var initVector = sha.CreateInitVector(0, 512, regime);
            sha.getDuplex(initVector);
            BytesBuilder.ToNull(initVector);

            AddFilesToList:

            var e = Directory.EnumerateFiles(dirName, "*", SearchOption.AllDirectories);
            var d = Directory.EnumerateDirectories(dirName, "*", SearchOption.AllDirectories);
            var list1 = new List<FileInfo>();
            var list2 = new List<string>(d);
            float FSize = 0, FSizeF = 0;
            bool notSuccess = false;

            foreach (var f in d)
            {
                try
                {
                    var fd = new DirectoryInfo(f);
                    if (fd.Attributes != FileAttributes.Directory)
                        fd.Attributes = FileAttributes.Directory;
                }
                catch (Exception ex)
                {
                    notSuccess = true;
                    ddso.errorMessage += "\r\n" + "error for directory (add in list) " + f + "\r\n" + ex.Message;
                }
            }

            var filesCount = 0;
            foreach (var f in e)
            {
                filesCount++;

                FileInfo fi;
                try
                {
                    lock (list1)
                    {
                        fi = new FileInfo(f);
                        FSize += fi.Length;
                        FSizeF += fi.Length;

                        list1.Add(fi);
                    }

                    sha.getDuplex(new ASCIIEncoding().GetBytes(fi.FullName), true);
                }
                catch (PathTooLongException)
                {
                    // Бывает такое, что путь ровно 260 символов. Но .NET придирается, что это уже слишком длинно. Приходится извращаться.
                    // Пытаемся переименовать директорию, в которой содержится файл
                    var lp  = f.LastIndexOf(Path.DirectorySeparatorChar);
                    var dir = f.Substring(0, lp);
                    lp  = dir.LastIndexOf(Path.DirectorySeparatorChar);
                    var fn  = dir.Substring(lp+1);
                    var dir2 = dir.Substring(0, lp);

                    var di  = new DriveInfo(Path.GetPathRoot(dir));
                    var fl = fn.Length - 1;

                    var newf = NewRandomFileName(dir, sha, dir2, ref fl, di, 0, 0);
                    Directory.Move(dir, newf);

                    goto AddFilesToList;
                }
                catch (Exception ex)
                {
                    notSuccess = true;
                    ddso.errorMessage += "\r\n" + "error for file (add in list) " + f + "\r\n" + ex.Message;
                }
            }

            // Почему-то диск работает на полную катушку именно при 4-х
            // Не учтена возможность того, что один файл будет переименовываться в другой, и их названия совпадут при переименовании, но после проверки (где они ещё не совпадали)
            int count = 4;
            for (int i = 0; i < count; i++)
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    try
                    {
                        while (true)
                        {
                            FileInfo fi;
                            lock (list1)
                            {
                                if (list1.Count <= 0)
                                    break;

                                fi = list1[list1.Count - 1];
                                list1.RemoveAt(list1.Count - 1);
                            }

                            if (ddso.doTerminate)
                            {
                                lock (list1)
                                {
                                    notSuccess = true;
                                    list1.Clear();
                                    // Monitor.Pulse(list1);
                                }
                                return;
                            }

                            var cddso = new DoDataSanitizationObject();
                            cddso.countToWrite = ddso.countToWrite;
                            cddso.complex = ddso.complex;
                            try
                            {
                                cddso.parent = ddso;
                                cddso.doNotDelete = ddso.doNotDelete;
                                DoDataSanitization(fi.FullName, cddso, regime, 0, onlyFirstBytes);
                            }
                            catch (Exception ex)
                            {
                                cddso.errorMessage += "\r\n" + "error for file " + fi.FullName + "\r\n" + ex.Message;
                            }
                            finally
                            {
                                lock (list1)
                                {
                                    if (!cddso.success)
                                        notSuccess = true;

                                    if (!String.IsNullOrEmpty(cddso.errorMessage))
                                        ddso.errorMessage += "\r\n" + "error for file " + fi.FullName + "\r\n" + cddso.errorMessage;

                                    if (!cddso.doTerminate)
                                        FSize -= fi.Length;

                                    //list1.Remove(fi);     // нельзя в foreach
                                    Monitor.Pulse(list1);
                                }
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref count);
                        lock (list1)
                        {
                            //list1.Clear();
                            Monitor.Pulse(list1);
                        }
                    }
                }
            );

            //var FileAverageSize = FSizeF / filesCount;

            ddso.MX = FSizeF + 1 + (filesCount << 16); //(FSizeF * 2)+1;
            lock (list1)
            {
                while (list1.Count > 0 || count > 0)
                {
                    Monitor.Wait(list1);
                    ddso.stage = (FSizeF - FSize) + ((filesCount-list1.Count) << 16);
                }
            }

            if (notSuccess)
            {
                ddso.success = false;
                return;
            }

            list2.Sort();
            list2.Reverse();
            foreach (var directoryName in list2)
            {
                if (ddso.doTerminate)
                {
                    notSuccess = true;
                    break;
                }

                try
                {
                    if (onlyFirstBytes <= 0)
                    RenameDirectory(ddso, regime, sha, directoryName);
                }
                catch (Exception ex)
                {
                    notSuccess = true;
                    ddso.errorMessage += "\r\n" + "error for directory " + directoryName + "\r\n" + ex.Message;
                }
            }

            if (endOfProcess && !notSuccess)
                RenameDirectory(ddso, regime, sha, dirName);

            ddso.success = !notSuccess;
        }

        private static void RenameDirectory(DoDataSanitizationObject ddso, int regime, SHA3 sha, string dirName)
        {
            var fd = new DirectoryInfo(dirName);/*
                    if (fd.Attributes != FileAttributes.Directory)
                        fd.Attributes = FileAttributes.Directory;
                    */
            int fl = fd.Name.Length;

            var di = new DriveInfo(fd.Root.FullName);
            string renamed = NewRandomFileName(fd.FullName, sha, fd.Parent.FullName, ref fl, di, 0, 0);

            Directory.Move(fd.FullName, renamed);

            if (regime != 2)
            {
                var cnt = regime == 0 ? 35 + 32 : 1;
                if (regime != 2)
                    cnt += 3;
                if (regime == 3 || regime == 4)
                    cnt += 512;

                for (int k = 0; k < 2; k++)
                {
                    for (int i = 0; i < cnt; i++)
                    {
                        var cDirName = renamed;
                        renamed = NewRandomFileName(cDirName, sha, fd.Parent.FullName, ref fl, di, k, i);

                        Directory.Move(cDirName, renamed);
                    }
                    // fl = 245/* - fd.Parent.FullName.Length - 1*/;
                }
            }

            if (!ddso.doNotDelete)
                Directory.Delete(renamed, false);
        }

        private void удалитьФайлМногопроходноToolStripMenuItem_Click(object sender, EventArgs e)
        {
            удалитьФайл(0);
        }

        private void удалитьФайлТремяПроходамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            удалитьФайл(1);
        }

        private void удалитьФайлОднимПроходомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            удалитьФайл(2);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            удалитьФайл(3);
        }

        
        private void удалитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            удалитьФайл(4);
        }

        class OFDC: BlackDisplay.Form1.OpenFileDialogContext
        {
            public int regime = 0;

            public OFDC(Form parentForm = null, OpenFileDialog initedDialog = null): base(parentForm, initedDialog)
            {
            }
        }
        
        private void удалитьФайлToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }

        private void удалитьФайл(int regime)
        {
            var cnt = new OFDC(this);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = true;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.Title = "Выберите файл к уничтожению";
            cnt.regime = regime;

            cnt.closedEvent += new Form1.OpenFileDialogContext.closed(processDelete);
            cnt.show();
        }

        private void processDelete(Form1.OpenFileDialogContext cnt, bool isOk)
        {
            var context = cnt as OFDC;
            if (isOk)
            {
                var fns   = context.dialog.FileNames;
                var r     = context.regime;
                var fn    = "";

                // 20 ещё и ниже
                float all = fns.Length << 20, completed = 0;
                foreach (var fna in fns)
                {
                    fn += fna + "\r\n";

                    if (fns.Length > 1)
                        all += new FileInfo(fna).Length;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить следующие файлы?\r\n" + fn, "Запрос на удаление файла", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                    return;

                var ddsoMany = new DoDataSanitizationObject();

                if (r == 4)
                {
                    var formDelete = new FormDelete(ddsoMany);
                    formDelete.ShowDialog();
                }

                if (fns.Length > 1)
                {
                    ddsoMany.MX = all;
                    ddsoMany.prepercent = 100f;

                    var fm = new DataSanitizationProgressForm(ddsoMany);
                    fm.Show();
                    fm.Focus();
                }

                var i = 0;
                addNewDelegate addNew = delegate (string FileName, DoDataSanitizationObject.exitedCallback func)
                {
                    var ddso = new DoDataSanitizationObject();
                    ddso.complex = ddsoMany.complex;
                    ddso.countToWrite = ddsoMany.countToWrite;
                    DataSanitizationProgressForm f = null;
                    if (fns.Length == 1)
                    {
                        f = new DataSanitizationProgressForm(ddso);
                        f.Show();
                        f.Focus();
                    }
                    else
                    {
                        var fi = new FileInfo(FileName);
                        ddso.workLength = fi.Length;
                        ddso.fileName   = fi.Name;
                        ddso.ec += func;
                    }

                    ThreadPool.QueueUserWorkItem
                    (
                        delegate
                        {
                            DoDataSanitization(FileName, ddso, r);
                        }
                    );
                };


                var DT = DateTime.Now.Ticks;

                long toWork = fns.Length;
                string failedList = "";
                DoDataSanitizationObject.exitedCallback funca = delegate(DoDataSanitizationObject ddso)
                {
                    toWork--;

                    if (toWork > 0)
                    {
                        completed     += ddso.workLength + (1 << 20);
                        ddsoMany.ts    = all / completed * (DateTime.Now.Ticks - DT) / 10000f / 1000f;
                        ddsoMany.stage = completed;

                        if (!ddso.success && ddso.exited)
                            failedList += "\r\n" + ddso.fileName;

                        addNew(fns[i++], ddso.ec);
                    }
                    else
                    {
                        ddsoMany.stage = ddsoMany.MX;
                        if (failedList.Length > 0)
                        {
                            ddsoMany.failed = true;
                            ddsoMany.errorMessage = failedList;
                        }
                        else
                            ddsoMany.success = true;

                        ddsoMany.exited = true;
                    }
                };

                addNew(fns[i++], funca);
            }
        }

        private delegate void addNewDelegate(string FileName, DoDataSanitizationObject.exitedCallback func);

        public class DoDataSanitizationObjectPreStep : DoDataSanitizationObject
        {
            public bool _preStepPassed = false;
            public bool preStepPassed
            {
                get
                {
                    return _preStepPassed;
                }
                set
                {
                    _success = false;
                    _preStepPassed = value;
                }
            }

            public virtual bool preSuccess
            {
                get
                {
                    return _success;
                }
            }

            public bool doNotPreStep = false;
            public override bool success
            {
                get
                {
                    return (preStepPassed || doNotPreStep) && _success;
                }
                set
                {
                    lock (this)
                    {
                        _success = value;
                        /*if (value)
                        {*/
                            if (preStepPassed || doNotPreStep)
                            {
                                _exited = true;
                            }
                        /*}*/

                        if (!callbackCalled && (preStepPassed || doNotPreStep))
                        {
                            if (ec != null)
                                ec(this);
                            callbackCalled = true;
                        }
                    }
                }
            }

            public override bool exited
            {
                get
                {
                    return _exited;
                }
                set
                {
                    lock (this)
                    {
                        _exited = value;
                        if (!callbackCalled && value && (preStepPassed || doNotPreStep))
                        {
                            if (ec != null)
                                ec(this);
                            callbackCalled = true;
                        }
                    }
                }
            }

            public override float prepercent
            {
                set
                {
                    _prepercent= value;
                }
                get
                {
                    return _prepercent;
                }
            }

            protected double _preStage = 0;
            public override double stage
            {
                get
                {
                    if (!doNotPreStep && !preStepPassed)
                        return 0;

                    return _stage;
                }
                set
                {
                    if (!doNotPreStep && !preStepPassed)
                    {
                        prepercent = (float) (value * 100.0 / MX);
                        _preStage  = value;
                        return;
                    }

                    _stage = value;
                }
            }
        }

        public class DoDataSanitizationObject
        {
            public DoDataSanitizationObject parent = null;

            public bool doNotDelete = false;
            public bool complex = false;
            public double _stage = 0;
            public virtual double stage
            {
                get
                {
                    return _stage;
                }
                set
                {
                    _stage = value;
                }
            }

            public double MX = 0;
            public float scale = 0;
            public float ts = 0;
            public float _prepercent = 0;
            public virtual float prepercent
            {
                set
                {
                    _prepercent = value;
                }
                get
                {
                    return _prepercent;
                }
            }

            public virtual float percent
            {
                get
                {
                    if (MX == 0)
                        return 0;

                    if (successd)
                        return 100;

                    return (float) (100.0*(double)stage / (double)MX);
                }
            }
            protected volatile bool _success = false;
            protected bool _exited = false;

            public bool  terminatedByUser = false;
            public bool _doTerminate = false;
            public bool doTerminate
            {
                get
                {
                    if (_doTerminate)
                        return true;
                    if (parent != null && parent._doTerminate)
                        return true;

                    return false;
                }
                set
                {
                    _doTerminate = value;
                }
            }


            protected bool callbackCalled = false;

            public bool successd = false;
            public virtual bool success
            {
                get
                {
                    return _success;
                }
                set
                {
                    lock (this)
                    {
                        _success = value;
                        _exited = true;

                        if (!callbackCalled && value)
                        {
                            if (ec != null)
                                ec(this);
                            callbackCalled = true;
                        }
                    }
                }
            }

            public virtual bool exited
            {
                get
                {
                    return _exited;
                }
                set
                {
                    lock (this)
                    {
                        _exited = value;
                        if (!callbackCalled && value)
                        {
                            if (ec != null)
                                ec(this);
                            callbackCalled = true;
                        }
                    }
                }
            }

            public bool failed = false;
            protected string _errorMessage = "";
            public string errorMessage
            {
                get
                {
                    if (_errorMessage.Length < 1024)
                        return _errorMessage;
                    return _errorMessage.Substring(0, 1021) + "...";
                }
                set
                {
                    _errorMessage = value;
                }
            }

            public exitedCallback ec;
            public delegate void exitedCallback(DoDataSanitizationObject self);

            /// <summary>
            /// Используется, если нужно запомнить объём работ извне и передать в callback
            /// </summary>
            public long   workLength = 0;
            public string fileName   = null;

            /// <summary>
            /// В режиме 4 показывает количество перезатираний
            /// </summary>
            public int countToWrite = 5;
        }

        public static void DoDataSanitization(string FileName, DoDataSanitizationObject ddso = null, int onlySimpleDestruction = 0, int max = 0, int onlyFirstBytes = -1)
        {
            if (!File.Exists(FileName) || ddso.doTerminate)
            {
                if (ddso != null)
                {
                    ddso.success = false;

                    if (!File.Exists(FileName))
                        ddso.errorMessage += "\r\n" + "Файл " + FileName + " не существует";
                }
                return;
            }

            if (ddso == null)
                ddso = new DoDataSanitizationObject();

            var sha = new SHA3(8192);
            var fi = new FileInfo(FileName);
            try
            {
                var cf  = (int) (fi.Length >= 72000 ? 72000 : fi.Length);
                if (cf <= 0)
                    cf = FileName.Length;


                var f = new ASCIIEncoding().GetBytes(fi.FullName + "|" + fi.Length + "|" + fi.LastAccessTimeUtc.ToString("r") + "|" + fi.LastWriteTimeUtc.ToString("r") + "|" + fi.CreationTimeUtc.ToString("r") + "|" + fi.Directory.LastAccessTimeUtc.ToString("r") + "|" + fi.Directory.LastWriteTimeUtc.ToString("r") + "|" + fi.Directory.CreationTimeUtc.ToString("r"));
                var b = new byte[cf + f.Length];
                int count = 0; bool fl = false;
                do
                {
                    try
                    {
                        var s = File.OpenRead(FileName);
                        try
                        {
                            s.Read(b, 0, cf);
                            fl = true;
                        }
                        finally
                        {
                            s.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        count++;
                        if (count > 8)
                        {
                            ddso.errorMessage += "\r\n" + e.Message;
                            if (ddso.parent != null)
                                ddso.parent.errorMessage += "\r\n" + e.Message;

                            ddso.success = false;
                            if (ddso.parent != null && !ddso.parent.failed)
                            {
                                ddso.parent.failed = true;
                            }
                            return;
                        }
                        else
                            Thread.Sleep(50);
                    }
                }
                while (!fl);

                BytesBuilder.CopyTo(f, b, cf);

                var initVector = sha.CreateInitVector(0, 512, 40);
                sha.prepareGamma(b, initVector, true);
                BytesBuilder.ToNull(initVector);

                if (fi.Attributes != FileAttributes.Normal)
                    fi.Attributes = FileAttributes.Normal;

                var fnb = Encoding.GetEncoding("utf-16").GetBytes(FileName);
                /*if (fi.Length < block)
                {
                    var bn = CreateFileW(fnb, 0x40000000, 0, 0, 3, 0x80, 0);
                    if (bn >= 0)
                    {
                        int NumberOfBytesWritten;
                        var a = sha.getGamma(fi.Length);
                        WriteFile(bn, a, a.Length, out NumberOfBytesWritten, 0);
                        CloseHandle(bn);
                    }
                }*/
                
                //int bin = CreateFile(FileName, 0x40000000, 0, 0, 3, 0x80 | 0x08000000, 0); // GENERIC_WRITE, NO_SHARE, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN

                int block = getBlockSize(fi);
                int bin = 0, tryCount = 0;
                do
                {
                    if (tryCount > 0)
                        Thread.Sleep(50);

                    if (fi.Length >= block)
                        bin = CreateFileW(fnb, 0x40000000, 0, 0, 3, 0x80 | 0x20000000 | 0x80000000, 0); // GENERIC_WRITE, NO_SHARE, OPEN_EXISTING, FILE_FLAG_NO_BUFFERING FILE_FLAG_WRITE_THROUGH
                    else
                        bin = CreateFileW(fnb, 0x40000000, 0, 0, 3, 0x80, 0); // GENERIC_WRITE, NO_SHARE, OPEN_EXISTING
                }
                while (tryCount++ < 5 && bin <= 0);

                if (bin <= 0)
                {
                    var lastError = GetLastError();
                    ddso.success = false;

                    var msg = "Не удалось открыть файл для затирания " + FileName + ", ошибка №" + lastError;
                    if (ddso.parent != null)
                    {
                        if (!ddso.parent.failed)
                            ddso.parent.failed = false;
                        ddso.parent.errorMessage += "\r\n" + msg;
                    }
                    else
                        MessageBox.Show(msg);

                    sha.Clear(true);

                    return;
                }

                try
                {
                    if (onlyFirstBytes > 0)
                        DataSanitization(new DriveInfo(fi.Directory.Root.FullName), ddso, fi.Length > onlyFirstBytes ? onlyFirstBytes : fi.Length, sha, bin, 2, max);
                    else
                        DataSanitization(new DriveInfo(fi.Directory.Root.FullName), ddso, fi.Length, sha, bin, onlySimpleDestruction, max);
                }
                finally
                {
                    CloseHandle(bin);
                }
            }
            finally
            {
                if (ddso.successd && File.Exists(FileName))
                {
                    bool flag = false;
                    try
                    {
                        var dir = fi.DirectoryName;
                        var fn  = fi.Name;

                        string renamed = null;
                        var cnt = onlySimpleDestruction == 0 ? 35+32 : 1;
                        if (onlySimpleDestruction != 2)
                            cnt += 3;
                        if (onlySimpleDestruction == 3)
                            cnt = 512;
                        if (onlySimpleDestruction == 4)
                            cnt = ddso.countToWrite;

                        int fl = fn.Length;
                        /*if (fi.DirectoryName.Length + fl < 259)
                            fl = 259 - fi.DirectoryName.Length - 1;*/

                        var di = new DriveInfo(fi.Directory.Root.FullName);
                        for (int k = 0; k < 2; k++)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                                {
                                    ddso.doTerminate = true;

                                    if (ddso.parent != null)
                                        ddso.terminatedByUser = ddso.parent.terminatedByUser;

                                    break;
                                }

                                renamed = NewRandomFileName(FileName, sha, dir, ref fl, di, k, i);

                                File.Move(FileName, renamed);
                                FileName = renamed;

                                if (onlyFirstBytes > 0)
                                    break;

                            }

                            if (onlyFirstBytes > 0)
                                break;

                            // fl = 259; //259 - fi.DirectoryName.Length - 1;
                        }

                        if (!ddso.doNotDelete && !ddso.doTerminate)
                        {
                            File.WriteAllBytes(renamed, sha.getGamma(1024*4));
                            File.Delete(renamed);
                        }

                        if (!ddso.doTerminate)
                            flag = true;
                    }
                    catch (Exception e)
                    {
                        ddso.errorMessage += "\r\n" + e.Message;
                        if (ddso.parent != null)
                            ddso.parent.errorMessage += "\r\n" + e.Message;
                    }

                    sha.Clear(true);
                    ddso.success = flag;
                }
                else
                {
                    sha.Clear(true);
                    ddso.success = false;
                }
            }
        }

        /// <summary>
        /// Генерирует новое случайное имя файла. Если файл невозможно переименовать, то генерирует случайное имя длиннее того, что задано в fl
        /// </summary>
        /// <param name="FileName">Исходное полное имя файла</param>
        /// <param name="sha">Инициализированный объект SHA3 для генерации случайной последовательности</param>
        /// <param name="dir">Имя директории</param>
        /// <param name="fl">Длина генерируемого имени файла</param>
        /// <param name="di">Информация о логическом диске</param>
        /// <param name="k">Параметр k. Если k и i равны нулю, то идет генерация имени файла в ASCII формате. В настоящий момент времени всегда используется ASCII формат</param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string NewRandomFileName(string FileName, SHA3 sha, string dir, ref int fl, DriveInfo di, int k, int i)
        {
            string renamed;
            int countFl = 0;
            do
            {
                string fn2;

                do
                {
                    if (true || di.DriveFormat.StartsWith("FAT") || k == 0 && i == 0)
                        fn2 = SHA3.generatePwd(sha.getGamma(fl), "qwertyuioplkjjhgfdsazxcvbnm0123456789.-!@#$%^&()_~+");
                    else
                        fn2 = Encoding.GetEncoding("utf-16").GetString(getNTFSRandomName(sha, fl << 1));
                }
                while (fn2 == "." || fn2 == "..");

                renamed = Path.Combine(dir, fn2);
                countFl++;

                if (countFl > 65536 * fl) // Программа не должна зациклиться, если имя файла невозможно изменить, т.к. оно слишком короткое
                    fl++;
            }
            while (FileName == renamed || File.Exists(renamed) || Directory.Exists(renamed));
            return renamed;
        }

        private static int getBlockSize(string DriveName)
        {
            uint lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters;
            GetDiskFreeSpaceA(DriveName, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
            int block = (int)(lpSectorsPerCluster * lpBytesPerSector);
            return block;
        }

        private static int getBlockSize(FileInfo fi)
        {
            return getBlockSize(Path.GetPathRoot(fi.FullName));
        }

        public static void DataSanitization(DriveInfo di, DoDataSanitizationObject ddso, long fl, SHA3 sha, int bin, int onlySimpleDestruction = 0, int maxData = 0)
        {
            int block = getBlockSize(di.Name);

            int NumberOfBytesWritten = 0; long tmp = 0;
            long length = 0;

            
            // Питер Гутман
            var bytes = new byte[][] {  new byte[] {}, new byte[] {}, new byte[] {}, new byte[] {}, 
                                        new byte[] { 0x55 }, new byte[] { 0xAA }, new byte[] { 0x92, 0x49, 0x24 }, new byte[] { 0x49, 0x24, 0x92 }, new byte[] { 0x24, 0x92, 0x49 },
                                        new byte[] { 0x00 }, new byte[] { 0x11 }, new byte[] { 0x22 }, new byte[] { 0x33 }, new byte[] { 0x44 },
                                        new byte[] { 0x55 }, new byte[] { 0x66 }, new byte[] { 0x77 }, new byte[] { 0x88 }, new byte[] { 0x99 },
                                        new byte[] { 0xAA }, new byte[] { 0xBB }, new byte[] { 0xCC }, new byte[] { 0xDD }, new byte[] { 0xEE }, new byte[] { 0xFF },
                                        new byte[] { 0x92, 0x49, 0x24 }, new byte[] { 0x49, 0x24, 0x92 }, new byte[] { 0x24, 0x92, 0x49 },
                                        new byte[] { 0x6D, 0xB6, 0xDB }, new byte[] { 0xB6, 0xDB, 0x6D }, new byte[] { 0xDB, 0x6D, 0xB6 },
                                        new byte[] {}, new byte[] {}, new byte[] {}, new byte[] {}, 
                                    };

            ddso.stage = 0;

            var t1 = DateTime.Now;
            byte[] nullb = null;

            if (fl <= block)
                nullb = sha.getGamma(fl);
            else
                nullb = sha.getGamma(block);

            var ts1 = DateTime.Now.Subtract(t1);

            t1 = DateTime.Now;
            // Простая перезапись одинаковым случайным блоком всего файла
            SetFilePointerEx(bin, 0, out tmp, 0);
            
            for (length = 0; length < fl; length += nullb.Length)
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                ddso.prepercent = (float) length * 100f / fl;
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
            FlushFileBuffers(bin);
            ddso.prepercent = 100f;

            if (onlySimpleDestruction == 2)
            {
                ddso.successd = true;
                return;
            }

            var ts2   = DateTime.Now.Subtract(t1);
            float scale = (float) (ts1.TotalSeconds * fl / block / ts2.TotalSeconds) + 1f;
            if (scale < 1)
                scale = 1;

            t1 = DateTime.Now;
            var cntts = 0;
            do
            {
                nullb = sha.getGamma(nullb.Length);
                SetFilePointerEx(bin, 0, out tmp, 0);
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                FlushFileBuffers(bin);

                nullb = sha.getGamma(nullb.Length);
                SetFilePointerEx(bin, 0, out tmp, 0);
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                FlushFileBuffers(bin);

                cntts += 2;
            }
            while (DateTime.Now.Subtract(t1).Ticks < 10*10000); // 10 мс
            var ts3   = DateTime.Now.Subtract(t1).TotalSeconds / (double) cntts;

            var oneMx = ts3/ts2.TotalSeconds*((double)fl/(double)nullb.Length);
            var sc = 1.0;
            if (fl < block)
                sc *= 2.0;
            ddso.scale = scale;
            var MX = 4 + bytes.Length + 8*scale + maxData*scale;
            if (onlySimpleDestruction == 1)
                MX = 4;
            else
            if (onlySimpleDestruction == 3)
                MX = 4+8 + (float)(sc*ts3*((float)fl/(float)nullb.Length)*(512)/ts2.TotalSeconds);
            else
            if (onlySimpleDestruction == 4)
                if (ddso.complex)
                    MX = 4 + 8 + (float)(sc*ts3*((float)fl/(float)nullb.Length)*ddso.countToWrite/ts2.TotalSeconds);
                else
                    MX = 4 + 8 + ddso.countToWrite;


            ddso.MX = MX;

            if (onlySimpleDestruction == 3)
                ddso.ts = (float) (ts2.TotalSeconds*MX);
            else
            if (onlySimpleDestruction == 4)
                ddso.ts = (float) (ts2.TotalSeconds*MX);
            else
                ddso.ts = (float) (ts2.TotalSeconds*MX);

            // Перезапись полудополненными значениями
            for (int i = 0; i < nullb.Length; i++)
            {
                nullb[i] ^= 0x55;
            }
            var st = ddso.stage;
            SetFilePointerEx(bin, 0, out tmp, 0);
            
            for (length = 0; length < fl; length += nullb.Length)
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                ddso.stage = (float) (st + (float) length / fl);
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
            FlushFileBuffers(bin);
            ddso.stage = st + 1;

            // Окончательная перезапись
            for (int i = 0; i < nullb.Length; i++)
            {
                nullb[i] ^= 0xFF;
            }
            st = ddso.stage;
            SetFilePointerEx(bin, 0, out tmp, 0);
            
            for (length = 0; length < fl; length += nullb.Length)
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                ddso.stage = (float) (st + (float) length / fl);
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
            FlushFileBuffers(bin);
            ddso.stage = st + 1;

            // Вторичная перезапись одинаковым случайным блоком всего файла
            nullb = sha.getGamma(nullb.Length);
            SetFilePointerEx(bin, 0, out tmp, 0);
            st = ddso.stage;
            for (length = 0; length < fl; length += nullb.Length)
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                ddso.stage = (float) (st + (float) length / fl);
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
            FlushFileBuffers(bin);
            ddso.stage = st + 1;

            // Вторичная окончательная перезапись
            for (int i = 0; i < nullb.Length; i++)
            {
                nullb[i] ^= 0xFF;
            }
            st = ddso.stage;
            SetFilePointerEx(bin, 0, out tmp, 0);
            for (length = 0; length < fl; length += nullb.Length)
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                ddso.stage = (float) (st + (float) length / fl);
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
            FlushFileBuffers(bin);
            ddso.stage = st + 1;

            if (onlySimpleDestruction == 1 || ddso.doTerminate)
            {
                ddso.successd = !(ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate));
                return;
            }

            if (onlySimpleDestruction == 0)
            {
                // Уничтожение по Питеру Гутману
                SimpleDataSanitization(di, ddso, sha, fl, bin, ref NumberOfBytesWritten, ref tmp, ref length, bytes);
            }
            else
            if (onlySimpleDestruction == 4)
            {
                // Многократное перезатирание псевдослучайными криптостойкими значениями
                PermanentDataSanitization(ddso.countToWrite, di, ddso, fl, sha, bin, ref NumberOfBytesWritten, ref tmp, ref length, ddso.stage, new object(), oneMx);
            }
            else
            {
                var bytes3 = new byte[][] {  new byte[] {0x55, 0xAA, 0x55, 0xAA, 0xAA, 0x55, 0xAA, 0x55}, new byte[] {0xAA, 0x55, 0xAA, 0x55, 0x55, 0xAA, 0x55, 0xAA}};
                for (int i = 0; i < 4; i++) // 4 по два раза
                {
                    SimpleDataSanitization(di, ddso, sha, fl, bin, ref NumberOfBytesWritten, ref tmp, ref length, bytes3);
                }

                // Ещё 512 раз
                PermanentDataSanitization(512, di, ddso, fl, sha, bin, ref NumberOfBytesWritten, ref tmp, ref length, ddso.stage, bytes3, oneMx);
            }

            // Дополнительное перезатирание
            for (int i = 0; i < maxData; i++)
            {
                DataSanitizationGamma(ddso, fl, sha, bin, di);
                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }

            if (fl < block)
                DataSanitization(di, ddso, block, sha, bin, onlySimpleDestruction, maxData);

            ddso.successd = !(ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate));
        }

        private static void PermanentDataSanitization(int count, DriveInfo di, DoDataSanitizationObject ddso, long fl, SHA3 sha, int bin, ref int NumberOfBytesWritten, ref long tmp, ref long length, double st, object bytes3, double oneMX)
        {
            /*uint lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters;
            GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
            int block = (int)(lpSectorsPerCluster * lpBytesPerSector);*/
            int  block   = getBlockSize(di.Name);
            bool complex = ddso.complex;

            var cf = (int) Math.Ceiling((double)fl/block)*block;
            if (cf > 24*1024*1024)
                cf = 24*1024*1024;
            if (complex)
                cf = block;

            /*
            if (cf < block)
                cf = block;*/
            if (fl < block)
                cf = (int) fl;

            var fa = fl;
            var fm = cf > 0 ? fl % cf : 0;
            if (fm > 0)
                fa += block - fm;
            else
            if (cf == 0)
            {
                fa = block;
            }

            var cBlock = sha.getGamma(cf);

            bool ended = false;
            var T = new Thread(new ThreadStart
                    (delegate
                    {
                        while (!ended)
                        {
                            var t = sha.getGamma(cf);
                            lock (bytes3)
                            {
                                cBlock = t;
                                Monitor.Pulse(bytes3);
                            }
                        }
                    })
                );
            T.IsBackground = true;
            T.Start();

            if (!complex)
                try
                {
                    var cb = cBlock;
                    for (int i = 0; i < count; i++)
                    {
                        // cf = cBlock.Length;
                        SetFilePointerEx(bin, 0, out tmp, 0);
                        for (length = 0; length < fl; length += cf)
                        {
                            lock (bytes3)
                            {
                                while (cBlock == null)
                                {
                                    Monitor.Wait(bytes3, 5000);
                                }

                                cb = cBlock;
                                cBlock = null;
                            }

                            WriteFile(bin, cb, cf, out NumberOfBytesWritten, 0);

                            FlushFileBuffers(bin);
                            ddso.stage = (float)(st + (float)length / fa * oneMX);

                            if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                                break;
                        }
                        st += oneMX;
                        ddso.stage = st;

                        FlushFileBuffers(bin);
                        if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                            break;
                    }
                }
                finally
                {
                    ended = true;
                }
            else
                try
                {
                    var a = st;
                    var cb = cBlock;

                    //SetFilePointerEx(bin, 0, out tmp, 0);
                    for (length = 0; length < fl; length += cf)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            /*if (i != 0)
                            SetFilePointerEx(bin, -cf, out tmp, 1);*/
                            SetFilePointerEx(bin, length, out tmp, 0);

                            lock (bytes3)
                            {
                                while (cBlock == null)
                                {
                                    Monitor.Wait(bytes3, 500);
                                }

                                cb = cBlock;
                                cBlock = null;
                            }

                            WriteFile(bin, cb, cf, out NumberOfBytesWritten, 0);

                            FlushFileBuffers(bin);

                            var b = (double)((double) i * cf / (double) fa * oneMX);
                            ddso.stage = a + b;

                            if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                                break;
                        }
                        a += (double) cf / (double) fa * (double) count * oneMX;
                        ddso.stage = a;

                        FlushFileBuffers(bin);
                        if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                            break;
                    }
                }
                finally
                {
                    ended = true;
                }
        }

        [DllImport("Kernel32.dll")]
        public static extern Int32 CreateFile(string lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, Int32 lpSecurityAttributes,
                                                                Int32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("Kernel32.dll")]
        public static extern Int32 CreateFileW(byte[] lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, Int32 lpSecurityAttributes,
                                                                Int32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("Kernel32.dll")]
        public static extern Int32 SetFilePointerEx(Int32 hFile, long liDistanceToMove, out long lpNewFilePointer, int dwMoveMethod);

        [DllImport("Kernel32.dll")]
        public static extern Int32 FlushFileBuffers(Int32 hFile);

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 WriteFile(Int32 hFile, byte[] buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);
        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 WriteFile(Int32 hFile, byte* buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        public static extern Int32 GetDiskFreeSpaceA(string LpRootPathName , out UInt32 lpSectorsPerCluster, out UInt32 lpBytesPerSector, out UInt32 lpNumberOfFreeClusters,
                                                                out UInt32 lpTotalNumberOfClusters);

        
        unsafe public struct FILE_STREAM_INFO
        {
          public Int32         NextEntryOffset;
          public Int32         StreamNameLength;
          public Int64         StreamSize;
          public Int64         StreamAllocationSize;
          // public char[]          StreamName;
        }

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 GetFileInformationByHandleEx(Int32 hFile, Int32 FileInformationClass, FILE_STREAM_INFO * lpFileInformation, Int32 dwBufferSize);


        private static void SimpleDataSanitization(DriveInfo di, DoDataSanitizationObject ddso, SHA3 sha, long fl, int bin, ref int NumberOfBytesWritten, ref long tmp, ref long length, byte[][] bytes)
        {
            //var cf = fl > 3*1024*1024 ? 3*1024*1024 : 65536*3;
            /*uint lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters;
            GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
            int block = (int)(lpSectorsPerCluster * lpBytesPerSector);*/
            int block = getBlockSize(di.Name);

            var cf = (int) Math.Ceiling((double)fl/block)*block;
            if (cf > 24*1024*1024)
                cf = 24*1024*1024;
            /*
            if (cf < block)
                cf = block;*/
            if (fl < block)
                cf = (int) fl;

            var tmpa = new byte[cf];
            for (int i = 0; i < bytes.Length; i++)
            {
                SetFilePointerEx(bin, 0, out tmp, 0);

                if (bytes[i].Length > 0)
                {
                    for (int j = 0; j < cf; j ++)
                    {
                        tmpa[j] = bytes[i][j % bytes[i].Length];
                    }

                    var st = ddso.stage;

                    
                    for (length = 0; length < fl; length += cf)
                    {
                        WriteFile(bin, tmpa, tmpa.Length, out NumberOfBytesWritten, 0);
                        FlushFileBuffers(bin);
                        ddso.stage = (float) (st + (float) length / fl);

                        if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                            break;
                    }

                    FlushFileBuffers(bin);
                    ddso.stage = st + 1;
                }
                else
                {
                    if (bytes.Length > 32 && i < 4)
                        DataSanitizationGamma(ddso, fl, sha, bin, di, 1088);
                    else
                        DataSanitizationGamma(ddso, fl, sha, bin, di);
                }

                BytesBuilder.ToNull(tmpa);

                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }
        }

        protected static void DataSanitizationGamma(DoDataSanitizationObject ddso, long fl, SHA3 sha, int bin, DriveInfo di, int r = 576)
        {
            var st = ddso.stage;
            int NumberOfBytesWritten; long tmp, length;

            SetFilePointerEx(bin, 0, out tmp, 0);
            //sha.prepareGamma(sha.CreateInitVector(200), sha.CreateInitVector(71));

            //var cf = fl > 1024*1024 ? 1024*1024 : 65536*3;
            /*uint lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters;
            GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
            int block = (int)(lpSectorsPerCluster * lpBytesPerSector);*/
            int block = getBlockSize(di.Name);

            var cf = (int) Math.Ceiling((double)fl/block)*block;
            if (cf > 24*1024*1024)
                cf = 24*1024*1024;
            /*
            if (cf < block)
                cf = block;*/
            if (fl < block)
                cf = (int) fl;

            for (length = 0; length < fl; length += cf)
            {
                var tmpa = sha.getGamma(cf, false, r);
                
                WriteFile(bin, tmpa, cf, out NumberOfBytesWritten, 0);
                FlushFileBuffers(bin);
                ddso.stage = (float) (st + ddso.scale * (float) length / fl);
                BytesBuilder.ToNull(tmpa);

                if (ddso.doTerminate || (ddso.parent != null && ddso.parent.doTerminate))
                    break;
            }

            FlushFileBuffers(bin);
            GC.Collect();

            ddso.stage = st + ddso.scale;
        }

        private void удалитьДиректориюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            КомандаУдалитьДиректорию(0);
        }     
   
        private void удалитьДиректориюОднимПроходомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            КомандаУдалитьДиректорию(2);
        }

        private void удалитьДиректориюТремяПроходамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            КомандаУдалитьДиректорию(1);
        }

        
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            КомандаУдалитьДиректорию(3);
        }

        
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            КомандаУдалитьДиректорию(4);
        }

        private void КомандаУдалитьДиректорию(int regime)
        {
            var cnt = new OFDC(this);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = false;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.Title = "Выберите любой файл папки, которую хотите уничтожить";
            cnt.regime = regime;

            cnt.closedEvent += new Form1.OpenFileDialogContext.closed(processDeleteD);
            cnt.show();
        }

        private void processDeleteD(Form1.OpenFileDialogContext cnt, bool isOk)
        {
            var context = cnt as OFDC;
            if (isOk)
            {
                var fn = Path.GetDirectoryName(context.dialog.FileName);
                var r  = context.regime;
                if (MessageBox.Show("Вы уверены, что хотите удалить содержимое директории " + fn + " ?\r\nЗакройте все программы, которые могут использовать удаляемую директорию. Проверьте, что директория не открыта в проводнике", "Запрос на удаление файла", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != System.Windows.Forms.DialogResult.Yes)
                    return;

                var ddso = new DoDataSanitizationObjectPreStep();
                var f = new DataSanitizationProgressForm(ddso);
                f.Show();
                f.Focus();
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        удалитьДиректорию(r, fn, ddso);
                    }
                );
            }
        }

        
        private void создатьНаДискеБольшойФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void СоздатьБольшойФайлДиалог(int regime)
        {
            var cnt = new OFDC(this);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = false;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.RestoreDirectory = false;
            cnt.dialog.Title = "Выберите любой файл диска, в котором необходимо создать много файлов";
            cnt.regime = regime;

            cnt.closedEvent += new Form1.OpenFileDialogContext.closed(СоздатьБольшойФайл);
            cnt.show();
        }

        static int sleepForDiskSpaceClean = 100;
        private void создатьБольшойФайлОдинПроходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sleepForDiskSpaceClean = 0;
            СоздатьБольшойФайлДиалог(2);
        }
        
        private void создатьМногоФайловзадержкаБезГарантийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sleepForDiskSpaceClean = 500;
            СоздатьБольшойФайлДиалог(2);
        }

        // Выклюячено
        private void создатьБольшойФайлТриПроходаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            СоздатьБольшойФайлДиалог(1);
        }

        // Выклюячено
        private void создатьНаДискеБольшойФайлToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            СоздатьБольшойФайлДиалог(0);
        }

        private void СоздатьБольшойФайл(Form1.OpenFileDialogContext cnt, bool isOk)
        {
            var context = cnt as OFDC;
            if (isOk)
            {
                var fn = Path.GetDirectoryName(context.dialog.FileName);
                var r  = context.regime;

                var ddso = new DoDataSanitizationObject();
                var f = new DataSanitizationProgressForm(ddso);
                f.MsgTerminate = "Создание файлов прекращено пользователем";
                f.MsgFailed    = "Создание файлов окончилось неуспешно";
                f.MsgSuccess   = "Создание файлов успешно окончилось";
                f.Show();
                f.Focus();
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        СоздатьБольшойФайл(new DirectoryInfo(fn), ddso, r);
                    }
                );

                if (MessageBox.Show("После создания файлов программа может удалить их автоматически либо оставить на диске, например, для последующего удаления с помощью меню 'удалить директорию' или средствами Windows.\r\n"
                    + "\r\nНе удалять файлы автоматически?", "Выбор режима удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    ddso.doNotDelete = true;

                if (!ddso.doNotDelete)
                {
                    MessageBox.Show("Сейчас будет создаваться большое количество файлов в корневой директории выбранного вами диска, все файлы буду располагаться в папке, начинающейся с символов \"_empter.\".\r\nПроверьте, что не работает никакая программа, которой нужно свободное место на диске, в частности, закачиваемые из интернета файлы уже получили необходимый размер и заполняются данными без увеличения.\r\n" + 
                                "Это не гарантирует полную очистку свободного пространства диска, возможно, часть файлов всё равно сможет быть восстановлена. Для гарантированной полной очистки пользуйтесь другими программами\r\n" +
                                "Время выполнения операции прогнозируется без учёта удаления созданных файлов. Удаление может занять время, сравнимое с временем создания файлов\r\n\r\n"
                                + "После завершения перезаписи вы можете сами удалить файлы, если нажали кнопку \"прервать\".");
                }
                else
                {
                    MessageBox.Show("Сейчас будет создаваться большое количество файлов в корневой директории выбранного вами диска, все файлы буду располагаться в папке, начинающейся с символов \"_empter.\".\r\nПроверьте, что не работает никакая программа, которой нужно свободное место на диске, в частности, закачиваемые из интернета файлы уже получили необходимый размер и заполняются данными без увеличения.\r\n" + 
                                "Это не гарантирует полную очистку свободного пространства диска, возможно, часть файлов всё равно сможет быть восстановлена. Для гарантированной полной очистки пользуйтесь другими программами\r\n"
                                + "\r\nПосле завершения операции, удалите созданные программой файлы вручную или с помощью меню 'удалить файлы' для более тщательного перезатирания уже затёртых мест.");
                    f.MsgSuccess += "\r\nНе забудьте удалить файлы самостоятельно, они находятся в папке, находящейся корневой директории диска, с именем, начинающимся на 'empter.'";
                }
            }
        }

        private void СоздатьБольшойФайл(DirectoryInfo directoryInfo, DoDataSanitizationObject ddso, int regime)
        {
            var sha = new SHA3(65536);
            var initVector1 = sha.CreateInitVector(0,  512, 40);
            var initVector2 = sha.CreateInitVector(64, 512, 40);
            sha.prepareGamma(initVector1, initVector2);
            BytesBuilder.ToNull(initVector1);
            BytesBuilder.ToNull(initVector2);

            try
            {
                if (regime == 2)
                    СоздатьБольшойФайлРежим2(directoryInfo, ddso, sha);
                /*else
                if (regime == 1)
                    СоздатьБольшойФайлРежим1(directoryInfo, ddso, sha);*/
                else
                    MessageBox.Show("Ошибка: неизвестный режим удаления");
                /*if (regime < 2)
                    DoDataSanitization(FileName, ddso, regime);*/
            }
            finally
            {
                /*FlushFileBuffers(bin);
                CloseHandle(bin);
                File.Delete(FileName);*/
                ddso.exited = true;
            }
        }

        unsafe private static void СоздатьБольшойФайлРежим2(DirectoryInfo directoryInfo, DoDataSanitizationObject ddso, SHA3 sha)
        {
            var di = new DriveInfo(directoryInfo.Root.FullName);
            uint lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters;
            GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);

            int  block = (int) (lpSectorsPerCluster * lpBytesPerSector);
            long gSize = 4*1024*1024 < block ? (long) block : 4*1024*1024;
            var  nullb = sha.getGamma(gSize);

            ConcurrentQueue<byte[]> gamma = new ConcurrentQueue<byte[]>();
            gamma.Enqueue(nullb);

            bool ended = false;
            var T = new Thread(new ThreadStart
                    (delegate
                    {
                        //int ti = 0;
                        while (!ended)
                        {
                            var t = sha.getGamma(gSize);
                            gamma.Enqueue(t);
                            // BytesBuilder.CopyTo(t, nullb, ti);
                            /*
                            ti += block;
                            if (ti > nullb.Length)
                                ti = 0;*/

                            while (gamma.Count > 16)
                                Thread.Sleep(50);
                        }
                    })
                );
            T.IsBackground = true;
            T.Start();

            long tmp, fl = di.TotalFreeSpace;
            int NumberOfBytesWritten = 0;
            ddso.prepercent = 100f;
            ddso.MX = fl + 1;
            var t1 = DateTime.Now;

            int bytesToWrite = nullb.Length;

            List<string> files = new List<string>();
            var dir = SHA3.generatePwd(sha.getGamma(64), "qwertyuioplkjjhgfdsazxcvbnm0123456789.-!@#$%^&()_~+");
            Directory.CreateDirectory(Path.Combine(directoryInfo.Root.FullName, "_empter." + dir));
            long l = 0;
            int FileNameL = 183;
            int errCount  = 0;
            DateTime dt1 = default, dt2;
            for (long length = 0; /*NumberOfBytesWritten > 0*/ true; )
            {
                if (sleepForDiskSpaceClean > 0)
                {
                    dt1 = DateTime.Now;
                    // Thread.Sleep(sleepForDiskSpaceClean);
                }


                while (!gamma.TryDequeue(out nullb))
                    Thread.Sleep(50);

                int bin; string FileName;
                if (di.DriveFormat.StartsWith("FAT"))
                {
                    var fn2      = SHA3.generatePwd(sha.getGamma(FileNameL), "qwertyuioplkjjhgfdsazxcvbnm0123456789.-!@#$%^&()_~+");
                        FileName = Path.Combine(directoryInfo.Root.FullName, "_empter." + dir, fn2);

                    // Если наш диск уже заполнен и данные размером в кластер писать уже нельзя, применяем буферизацию,
                    // чтобы система попробовала записать байты внутри таблиц файловой системы
                    if (bytesToWrite >= block)
                        bin = CreateFile(FileName, 0x40000000, 0, 0, 1, 0x80 | 0x20000000 | 0x80000000, 0); // CREATE_NEW
                    else
                        bin = CreateFile(FileName, 0x40000000, 0, 0, 1, 0x80, 0); // CREATE_NEW
                }
                else
                {
                    var fn2 = Encoding.GetEncoding("utf-16").GetString(getNTFSRandomName(sha, FileNameL << 1));

                        FileName = Path.Combine(directoryInfo.Root.FullName, "_empter." + dir, fn2);
                    var fn       = Encoding.GetEncoding("utf-16").GetBytes(FileName);

                    if (bytesToWrite >= block)
                        bin = CreateFileW(fn, 0x40000000, 0, 0, 1, 0x80 | 0x20000000 | 0x80000000, 0); // CREATE_NEW
                    else
                        bin = CreateFileW(fn, 0x40000000, 0, 0, 1, 0x80, 0); // CREATE_NEW
                }

                var continueFlag = false;
                if (bin <= 0)
                {
                    // Если данные уже не вмещаются, а FileNameL уже ну никак не хочет
                    if (bytesToWrite <= block && FileNameL <= 0)
                        break;

                    int en = GetLastError();
                    if (en == 112)  // "Недостаточно места на диске"
                    {
                        Thread.Sleep(300);
                        errCount++;

                        if (FileNameL > 0)
                        {
                            FileNameL--;
                            errCount = 0;
                        }

                        if (FileNameL <= 0)
                            break;

                        continueFlag = true;
                    }
                    else
                        ddso.errorMessage += "\r\nСоздание файла окончилось неудачей: ошибка №" + en;
                    /*if (new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace < 1024*4)
                        break;*/

                    GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
                    bytesToWrite = (int) (lpNumberOfFreeClusters * lpSectorsPerCluster * lpBytesPerSector);

                    if (continueFlag)
                        continue;

                    errCount++;
                    if (errCount > 16)
                    {
                        ddso.success = false;
                        ended = true;
                        return;
                    }
                    continue;
                }
                else
                    errCount = 0;

                files.Add(FileName);
                /*
                if (bytesToWrite < block)
                    bytesToWrite = block;
                */

                try
                {
                    SetFilePointerEx(bin, 0, out tmp, 0);

                    toBack:
                    if (bytesToWrite > nullb.Length)
                        bytesToWrite = nullb.Length;

                    if (bytesToWrite >= block)
                    {
                        WriteFile(bin, nullb, bytesToWrite, out NumberOfBytesWritten, 0);
                        if (NumberOfBytesWritten <= 0)
                        {
                            Thread.Sleep(300);

                            GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
                            bytesToWrite = (int) (lpNumberOfFreeClusters * lpSectorsPerCluster * lpBytesPerSector);

                            goto toBack;
                        }
                    }
                    else
                    {
                        var lastCount = 0;
                        fixed (byte* nla = nullb)
                        {
                            int a = 0;
                            do
                            {
                                lastCount = 0;
                                do
                                {
                                    WriteFile(bin, nla+a, 1, out NumberOfBytesWritten, 0);
                                    a++;

                                    if (NumberOfBytesWritten > 0)
                                        lastCount += NumberOfBytesWritten;
                                    else
                                        Thread.Sleep(100);

                                    if (lastCount > block)
                                    {
                                        FlushFileBuffers(bin);
                                        lastCount = 0;
                                    }

                                    if (nullb.Length <= a)
                                        a = 0;
                                }
                                while (NumberOfBytesWritten > 0);

                                FlushFileBuffers(bin);
                            }
                            while (lastCount != 0);
                        }
                    }
                    FlushFileBuffers(bin);

                    length += NumberOfBytesWritten;
                    l += NumberOfBytesWritten;
                    ddso.stage = length;

                    if (NumberOfBytesWritten == 0)
                    {
                        GetDiskFreeSpaceA(di.Name, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters);
                        if (lpNumberOfFreeClusters > 0 && bytesToWrite > block)
                        {
                            bytesToWrite = (int) (lpNumberOfFreeClusters * lpSectorsPerCluster * lpBytesPerSector);
                        }
                        else
                        {
                            //fl = new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace;
                            if (lpNumberOfFreeClusters > 0)
                            {
                                ddso.success = false;
                                ended = true;
                                return;
                            }
                        }
                    }

                    if (l > 0x0FFFFFFF || (ddso.ts == 0 && l > 1024*1024))
                    {
                        l = 0;
                        ddso.ts = (float) DateTime.Now.Subtract(t1).TotalSeconds * fl / length;
                    }

                    if (ddso.doTerminate)
                    {
                        ddso.success = false;
                        ended = true;
                        return;
                    }
                }
                finally
                {
                    CloseHandle(bin);

                    if (sleepForDiskSpaceClean > 0)
                    {
                        dt2 = DateTime.Now;
                        var span = dt2 - dt1;
                        var tm = (int) (span.TotalMilliseconds * 1.0);
                        if (tm < 50)
                            tm = 50;
                        if (tm > 500)
                            tm = 500;

                        Thread.Sleep(tm);
                    }
                }
            }

            ended = true;
            if (!ddso.doNotDelete)
            {
                for (int i = files.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (ddso.doTerminate)
                        {
                            ddso.success = true;
                            return;
                        }
                        File.Delete(files[i]);
                    }
                    catch
                    {}
                }
                Directory.Delete(Path.Combine(directoryInfo.Root.FullName, "_empter." + dir), true);
            }
            ddso.success = true;
        }

        private static byte[] getNTFSRandomName(SHA3 sha, int len)
        {
            var fn2 = sha.getGamma(len);
            for (int i = 0; i < fn2.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    if (fn2[i] < 48)
                        fn2[i] += (byte)(256 - 48);
                    if (fn2[i] >= 123 && fn2[i] <= 127)
                        fn2[i] += 5;
                    if (fn2[i] >= 90 && fn2[i] <= 96)
                        fn2[i] += 7;
                    if (fn2[i] >= 58 && fn2[i] <= 64)
                        fn2[i] += 7;
                }
            }
            return fn2;
        }
        /*
        private static void СоздатьБольшойФайлРежим1(DirectoryInfo directoryInfo, DoDataSanitizationObject ddso, SHA3 sha)
        {
            var fn2 = SHA3.generatePwd(sha.getGamma(32), "qwertyuioplkjjhgfdsazxcvbnm0123456789.-!@#$%^&()_~+");
            var FileName = Path.Combine(directoryInfo.FullName, "_" + fn2 + ".empter");
            var bin = CreateFile(FileName, 0x40000000, 0, 0, 1, 0x80 | 0x20000000 | 0x80000000, 0); // CREATE_NEW

            if (bin <= 0)
                return;

            var nullb = sha.getGamma(65536*1024);

            long tmp, fl = new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace;
            int NumberOfBytesWritten = 0;
            ddso.prepercent = 100f;
            ddso.MX = fl*3 + 1;
            var t1 = DateTime.Now;

            // Простая перезапись одинаковым случайным 512-битным блоком всего файла
            SetFilePointerEx(bin, 0, out tmp, 0);
            long l = 0;
            for (long length = 0; length < fl || NumberOfBytesWritten > 0; )
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);
                
                length += NumberOfBytesWritten;
                l += NumberOfBytesWritten;
                ddso.stage = length;
                
                if (NumberOfBytesWritten == 0)
                {
                    fl = new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace;
                    if (length < fl)
                    {
                        ddso.success = false;
                        ddso.exited  = true;
                        return;
                    }
                }

                if (l > 0x0FFFFFFF || (ddso.ts == 0 && l > 1024*1024))
                {
                    l = 0;
                    ddso.ts = (float) (DateTime.Now.Subtract(t1).TotalSeconds * fl * 3.0 / length);
                }

                if (ddso._doTerminate)
                    return;
            }

            // Перезапись полудополненными значениями
            for (int i = 0; i < nullb.Length; i++)
            {
                nullb[i] ^= 0x55;
            }
            SetFilePointerEx(bin, 0, out tmp, 0);
            for (long length = 0; length < fl || NumberOfBytesWritten > 0; )
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);

                length += NumberOfBytesWritten;
                l += NumberOfBytesWritten;
                ddso.stage = length + fl;
                
                if (NumberOfBytesWritten == 0)
                {
                    fl = new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace;
                    if (length < fl)
                    {
                        ddso.success = false;
                        ddso.exited  = true;
                        return;
                    }
                }

                if (l > 0x0FFFFFFF || (ddso.ts == 0 && l > 1024*1024))
                {
                    l = 0;
                    ddso.ts = (float) (DateTime.Now.Subtract(t1).TotalSeconds * fl * 3.0 / (length + fl));
                }

                if (ddso._doTerminate)
                    return;
            }

            // Перезапись дополненными к полудополненным значениями
            for (int i = 0; i < nullb.Length; i++)
            {
                nullb[i] ^= 0xFF;
            }
            SetFilePointerEx(bin, 0, out tmp, 0);
            for (long length = 0; length < fl || NumberOfBytesWritten > 0; )
            {
                WriteFile(bin, nullb, nullb.Length, out NumberOfBytesWritten, 0);

                length += NumberOfBytesWritten;
                l += NumberOfBytesWritten;
                ddso.stage = length + (fl << 1);
                
                if (NumberOfBytesWritten == 0)
                {
                    fl = new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace;
                    if (length < fl)
                    {
                        ddso.success = false;
                        ddso.exited  = true;
                        return;
                    }
                }

                if (l > 0x0FFFFFFF || (ddso.ts == 0 && l > 1024*1024))
                {
                    l = 0;
                    ddso.ts = (float) (DateTime.Now.Subtract(t1).TotalSeconds * fl * 3.0 / (length + fl));
                }

                if (ddso._doTerminate)
                    return;
            }

            ddso.success = true;
        }*/
    }
}
