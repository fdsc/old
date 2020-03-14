using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using keccak;
using System.IO;

namespace BlackDisplay
{
    public partial class Form1
    {
        public class OpenFileDialogContext// : ApplicationContext
        {
            public readonly OpenFileDialog dialog;
            public readonly Form parentForm;
            public readonly bool noInvoke;
            public OpenFileDialogContext(Form parentForm = null, OpenFileDialog initedDialog = null, bool noInvoke = false)
            {
                if (initedDialog == null)
                    dialog = new OpenFileDialog();
                else
                    dialog = initedDialog;

                this.parentForm = parentForm;
                this.noInvoke   = noInvoke;
            }

            public delegate void closed(OpenFileDialogContext context, bool isOk);
            public event closed closedEvent;
            protected Form f = null;

            public DialogResult result = DialogResult.None;
            public bool isCompleted = false;
            public void show()
            {
                #if forLinux
            /*
                f = new Form();
                f.Icon = Program.mainForm.Icon;
;
                f.MinimumSize = new System.Drawing.Size(0, 0);
                f.Size = new System.Drawing.Size(0, 0);
                f.Resize += new EventHandler(f_Resize);
                f.FormBorderStyle = FormBorderStyle.None;
                f.WindowState = FormWindowState.Minimized;
                f.Show();
            */
                result = dialog.ShowDialog(parentForm);
                isCompleted = true;
                closedEvent?.Invoke(this, result == DialogResult.OK);
                #else
                // ОСТОРОЖНО! Дублирование кода для Windows и Linux
                var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        try
                        {
                            f = new Form();
                            f.Icon = Program.mainForm.Icon;
;
                            f.MinimumSize = new System.Drawing.Size(0, 0);
                            f.Size = new System.Drawing.Size(0, 0);
                            f.Resize += new EventHandler(f_Resize);
                            f.FormBorderStyle = FormBorderStyle.None;
                            f.WindowState = FormWindowState.Minimized;
                            f.Show();

                            result = dialog.ShowDialog(f);
                            f.Close();
                        }
                        finally
                        {
                            lock (this)
                            {
                                isCompleted = true;
                                Monitor.PulseAll(this);
                            }
                        }

                        if (closedEvent != null)
                        {
                            if (parentForm != null && !noInvoke && parentForm.InvokeRequired)
                                try
                                {
                                    parentForm.Invoke(closedEvent, new object[] {this, result == DialogResult.OK});
                                }
                                catch
                                {
                                    closedEvent(this, result == DialogResult.OK);
                                }
                            else
                                closedEvent(this, result == DialogResult.OK);
                        }
                    }
                    catch
                    {
                        if (closedEvent != null)
                            closedEvent(this, false);
                    }
                }
                );

                thr.SetApartmentState(ApartmentState.STA);
                thr.IsBackground = true;
                thr.Start();
                #endif
            }

            void f_Resize(object sender, EventArgs e)
            {
                //f.WindowState = FormWindowState.Minimized;
                f.Size = new System.Drawing.Size(0, 0);
            }

            /*
            protected override void OnMainFormClosed(object sender, EventArgs e)
            {
                base.OnMainFormClosed(sender, e);

                if (closedEvent != null)
                {
                    if (parentForm != null && parentForm.InvokeRequired)
                        parentForm.Invoke(closedEvent);
                    else
                        closedEvent(this);
                }

                ExitThread();
            }
        }
            */
        }

        public class SaveFileDialogContext
        {
            public readonly SaveFileDialog dialog;
            public readonly Form parentForm;
            public SaveFileDialogContext(Form parentForm = null, SaveFileDialog initedDialog = null)
            {
                if (initedDialog == null)
                    dialog = new SaveFileDialog();
                else
                    dialog = initedDialog;

                this.parentForm = parentForm;
            }

            public delegate void closed(SaveFileDialogContext context, bool isOk);
            public event closed closedEvent;
            protected Form f = null;

            public DialogResult result = DialogResult.None;
            public bool isCompleted = false;
            public void show()
            {
                #if forLinux
            /*
                f = new Form();
                f.Icon = Program.mainForm.Icon;

                f.MinimumSize = new System.Drawing.Size(0, 0);
                f.Size = new System.Drawing.Size(0, 0);
                f.Resize += new EventHandler(f_Resize);
                f.FormBorderStyle = FormBorderStyle.None;
                f.WindowState = FormWindowState.Minimized;
                f.Show();
                */
                result = dialog.ShowDialog(parentForm);
                isCompleted = true;
                closedEvent?.Invoke(this, result == DialogResult.OK);
#else
                var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        try
                        {
                            f = new Form();
                            f.Icon = Program.mainForm.Icon;
;
                            f.MinimumSize = new System.Drawing.Size(0, 0);
                            f.Size = new System.Drawing.Size(0, 0);
                            f.Resize += new EventHandler(f_Resize);
                            f.FormBorderStyle = FormBorderStyle.None;
                            f.WindowState = FormWindowState.Minimized;
                            f.Show();

                            result = dialog.ShowDialog(f);
                            f.Close();
                        }
                        finally
                        {
                            lock (this)
                            {
                                isCompleted = true;
                                Monitor.PulseAll(this);
                            }
                        }

                        if (closedEvent != null && result == DialogResult.OK)
                        {
                            if (parentForm != null && parentForm.InvokeRequired)
                                parentForm.Invoke(closedEvent, new object[] {this, true});
                            else
                                closedEvent(this, true);
                        }
                        else
                            if (closedEvent != null)
                                closedEvent(this, false);
                    }
                    catch
                    {
                        if (closedEvent != null)
                            closedEvent(this, false);
                    }
                }
                );

                thr.SetApartmentState(ApartmentState.STA);
                thr.IsBackground = true;
                thr.Start();
#endif
            }

            void f_Resize(object sender, EventArgs e)
            {
                f.Size = new System.Drawing.Size(0, 0);
            }
        }

        public void cryptFile()
        {
            var cnt = new OpenFileDialogContext(this, null);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = false;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.Title = "Выберите файл для шифрования";

            cnt.closedEvent += new OpenFileDialogContext.closed(cnt_closedEvent);
            cnt.show();
        }

        void cnt_closedEvent(Form1.OpenFileDialogContext context, bool isOk)
        {
            if (!isOk)
                return;

            DoublePasswordForm pwdForm1, pwdForm2 = null;
            bool isSuccess = false;
            do
            {
                pwdForm1 = new DoublePasswordForm(1, Path.GetFileName(context.dialog.FileName));
                pwdForm1.ShowDialog();

                if (pwdForm1.resultText == null)
                    return;

                if (pwdForm1.resultText.Length < 6)
                {
                    MessageBox.Show("Извините, но введённый текст настолько мал, что не может являться паролем", "Шифрование", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    continue;
                }

                if (pwdForm1.fromFile)
                    break;

                pwdForm2 = new DoublePasswordForm(2, Path.GetFileName(context.dialog.FileName), true);
                pwdForm2.ShowDialog();

                if (pwdForm2.cancel)
                {
                    pwdForm1.clearResultText();
                    return;
                }

                if (/*!String.IsNullOrEmpty(pwdForm2.resultText) && */pwdForm1.resultText != pwdForm2.resultText)
                {
                    pwdForm1.clearResultText();
                    pwdForm2.clearResultText();
                    if (MessageBox.Show("Введённые пароли не равны друг другу\r\nХотите попробовать ещё раз?", "Шифрование", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                        return;
                }
                else
                    isSuccess = true;

            }
            while (!isSuccess);

            if (pwdForm2 != null)
                pwdForm2.clearResultText();

            new FormCrypt(pwdForm1, context.dialog.FileName).Show();
        }

        public static void GetKeyByPassword(DoublePasswordForm pwdForm, out byte[] key, int regime)
        {
            byte[] bytes;

            bytes = Encoding.Unicode.GetBytes(pwdForm.resultText);
            pwdForm.clearResultText();
            pwdForm = null;
/*
#warning INSECURE
File.AppendAllText("unsecure.log", "password: " + BitConverter.ToString(bytes).Replace("-", "") + "\r\n");
*/
            GetKeyByPassword(regime, bytes, out key);
        }

        private static void GetKeyByPassword(int regime, byte[] bytes, out byte[] key)
        {
            SHA3 sha = new SHA3(bytes.Length);
            if (regime < 10)
                sha.useOldDuplex = true;

            int pc = 4;
            var oiv = regime >= 20 ?
                                      Convert.FromBase64String("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")
                                    : new UTF8Encoding().GetBytes("passwordJjVjRyjcWwo7761qPBQUb3Sx8DACNpassword");
            var dk = 1024;
            if (regime >= 20)
            {
                dk = regime >= 30 ? 16 : 24;
                if (regime >= 34)
                    dk = 8;
            }
            key = sha.getDerivatoKey(bytes, oiv, dk, ref pc, regime >= 20 ? bytes.Length << 1 : bytes.Length, regime / 10);

            sha.Clear(true);
            sha = null;
            BytesBuilder.ToNull(bytes);
        }


        private void decryptFile()
        {
            var cnt = new OpenFileDialogContext(this);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = false;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.Filter = "All|*.*|Black display crypt. files|*.bdc";
            cnt.dialog.FilterIndex = 1;
            cnt.dialog.Title = "Выберите файл для расшифровки";

            cnt.closedEvent += new OpenFileDialogContext.closed(cnt_closedEventDecrypt);
            cnt.show();
        }

        void cnt_closedEventDecrypt(Form1.OpenFileDialogContext context, bool isOK)
        {
            if (!isOK)
                return;

            FileInfo fi;
            byte[] bt;
            GetPasswordAndDecryptFile(context, out fi, out bt, false, false, false, new onEnd(DecryptFile));
        }

        private void strToClipboard(string str)
        {
            var t = new object();
            // Для работы с буфером обмена приходится извращаться, т.к. он поддерживает только потоки STAThread
            var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        Clipboard.SetText(str);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалась работа с буфером обмена: " + ex.Message, "Сохранение содержимого", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        lock (t)
                            Monitor.Pulse(t);
                    }
                }/*,
                64 * 1024*/
                );

            lock (t)
            {
                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                //Monitor.Wait(t);
            }
        }

        private void DecryptFile(FileInfo fi, byte[] bt)
        {
            if (bt == null)
                return;

            do
            {
                var sdc = new SaveFileDialogContext(this, null);
                sdc.dialog.InitialDirectory = fi.DirectoryName;
                sdc.dialog.SupportMultiDottedExtensions = true;
                sdc.dialog.CheckFileExists = false;
                sdc.dialog.Title = "Выберите имя файла для сохранения расшифрованного содержимого";
                sdc.dialog.RestoreDirectory = true;

                #if forLinux
                sdc.show();
                #else
                lock (sdc)
                {
                    sdc.show();
                    while (!sdc.isCompleted)
                    {
                        Monitor.Wait(sdc, 0);
                        Application.DoEvents();
                    }
                }
                #endif

                if (sdc.result == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(sdc.dialog.FileName))
                    {
                        var dlgResult = MessageBox.Show("Файл с таким именем уже существует, хотите перезаписать?", "Расшифровка", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (dlgResult == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (dlgResult == System.Windows.Forms.DialogResult.No)
                            continue;
                    }

                    File.WriteAllBytes(sdc.dialog.FileName, bt);
                }
                else
                {
                    if (MessageBox.Show(this, "Хотите сохранить расшифрованное содержимое в буфер обмена?", "Копирование в буфер обмена", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        try
                        {
                            strToClipboard(new UTF8Encoding().GetString(bt));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Не удалась работа с буфером обмена: " + ex.Message, "Сохранение содержимого", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    }
                }

                break;
            }
            while (true);

            if (bt != null)
            {
                BytesBuilder.ToNull(bt);
                bt = null;
            }
        }

        public delegate void onEnd(FileInfo fif, byte[] decrypted);
        public static bool GetPasswordAndDecryptFile(Form1.OpenFileDialogContext context, out FileInfo fi, out byte[] bt, bool AcceptEmptyPassword = false, bool noMsg = false, bool keyFromFile = false, onEnd ended = null)
        {
            fi = new FileInfo(context.dialog.FileName);

            int countOfTryes = -1;
            bt = null;

            DoublePasswordForm pwdForm1, pwdForm2 = null;
            do
            {
                countOfTryes++;

                pwdForm1 = new DoublePasswordForm(1, fi.Name);
                pwdForm1.ShowDialog();

                if (pwdForm1.resultText == null)
                    return false;

                if (pwdForm1.fromFile)
                    break;

                if (AcceptEmptyPassword && pwdForm1.resultText.Length == 0)
                {
                    return true;
                }

                if (!AcceptEmptyPassword && pwdForm1.resultText.Length < 6)
                {
                    MessageBox.Show("Извините, но введённый текст настолько мал, что не может являться паролем", "Шифрование", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    continue;
                }

                if (fi.Length > 1024 * 1024 || countOfTryes > 1)
                {
                    pwdForm2 = new DoublePasswordForm(2, fi.Name, true);
                    pwdForm2.ShowDialog();

                    if (pwdForm2.cancel)
                    {
                        pwdForm1.clearResultText();
                        return false;
                    }

                    if (!String.IsNullOrEmpty(pwdForm2.resultText) && pwdForm1.resultText != pwdForm2.resultText)
                    {
                        pwdForm1.clearResultText();
                        pwdForm2.clearResultText();
                        if (MessageBox.Show("Введённые пароли не равны друг другу\r\nХотите попробовать ещё раз?", "Шифрование", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                            return false;

                        continue;
                    }

                    pwdForm2.clearResultText();
                }

                break;
            }
            while (true);

            GC.Collect();

            if (ended == null)
            {
                GetPasswordAndDecryptFile(fi, out bt, pwdForm1);

                if (bt == null)
                {
                    if (!noMsg)
                        if (MessageBox.Show("Расшифровка файла '" + fi.Name + "' не удалась - возможно, введён неверный пароль", "Расшифровка", MessageBoxButtons.OK, MessageBoxIcon.Stop) == System.Windows.Forms.DialogResult.Retry)
                            return false;   // continue;
                        else
                            return false;
                    else
                        return false;
                }

                return true;
            }
            else
            {
                var fif = fi;
                #if forLinux
                {
                    byte[] bd;
                    GetPasswordAndDecryptFile(fif, out bd, pwdForm1);

                    ended(fif, bd);

                    if (bd == null)
                    {
                        if (!noMsg)
                            MessageBox.Show("Расшифровка файла '" + fif.Name + "' не удалась - возможно, введён неверный пароль", "Расшифровка", MessageBoxButtons.OK, MessageBoxIcon.Stop); // == System.Windows.Forms.DialogResult.Retry
                    }
                }
                #else
                ThreadPool.QueueUserWorkItem
                (delegate
                {
                    byte[] bd;
                    GetPasswordAndDecryptFile(fif, out bd, pwdForm1);

                    ended(fif, bd);

                    if (bd == null)
                    {
                        if (!noMsg)
                            MessageBox.Show("Расшифровка файла '" + fif.Name + "' не удалась - возможно, введён неверный пароль", "Расшифровка", MessageBoxButtons.OK, MessageBoxIcon.Stop); // == System.Windows.Forms.DialogResult.Retry
                    }
                }
                );
                #endif
                return true;
            }
        }

        private static void GetPasswordAndDecryptFile(FileInfo fi, out byte[] bt, DoublePasswordForm pwdForm1)
        {
            byte[] key;
            int regime;
            using (var file = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                file.Position = 1;
                regime = file.ReadByte();
            }

            GetKeyByPassword(pwdForm1, out key, regime);
/*
#warning INSECURE
File.AppendAllText("unsecure.log", "key: " + BitConverter.ToString(key).Replace("-", "") + "\r\n");
*/
            pwdForm1.clearResultText();

            var sha = new SHA3(fi.Length);
            if (regime < 10)
                sha.useOldDuplex = true;

            var cbt = File.ReadAllBytes(fi.FullName);

            try
            {
                bt = sha.multiDecryptLZMA(cbt, key);
            }
            catch (Exception e)
            {
                MessageBox.Show("Расшифрование не удалось, возможно файл не является файлом программы rtbd. " + e.Message, "Расшифрование не удалось", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                bt = null;
                return;
            }
            finally
            {
                sha.Clear(true);
                BytesBuilder.ToNull(key);
                BytesBuilder.ToNull(cbt);
                cbt = null;
                key = null;
            }
        }
    }
}
