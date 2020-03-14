using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using keccak;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlackDisplay
{
    public partial class FormCrypt : Form
    {
        protected byte[] key = null;
        public readonly string FileName;

        private FormCrypt()
        {
            InitializeComponent();
        }

        public readonly string CryptFileName;
        public FormCrypt(DoublePasswordForm pwdForm, string fileName): this()
        {
            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    Form1.GetKeyByPassword(pwdForm, out key, 41);
                    pwdForm.clearResultText();

                    if (this.InvokeRequired)
                    {
                        try
                        {
                            this.Invoke(new postFileEncryptFunc(endOfPasswordCrypt));
                        }
                        catch
                        {
                            endOfPasswordCrypt();
                        }
                    }
                    else
                        endOfPasswordCrypt();
                }
            );

            richTextBox1.Text = "Перед шифрованием запомните пароль/ключ (остальные опции запомнятся в зашифрованном файле)\r\n\r\nЖдите, выполняется предварительное криптографическое преобразование пароля в ключ";
            CryptButton.Text  = "Подождите";

            FileName = fileName;
            FileNameBox.Text = FileName;

            CryptFileName = FileName + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bdc";
            PostfixBox.Text  = CryptFileName;
        }

        public void endOfPasswordCrypt()
        {
            CryptButton.Text    = "Зашифровать";
            CryptButton.Enabled = true;
            richTextBox1.Text   = "Перед шифрованием запомните пароль/ключ (остальные опции запомнятся в зашифрованном файле)";
        }

        private void FormCrypt_Shown(object sender, EventArgs e)
        {
        }

        private void GetHashCount()
        {
            var hashCountSize = SHA3.getHashCountForMultiHash20(71, 0, 300, 0, 1) - 6;
            if (hashCountSize < hashCountBox.Minimum)
                hashCountBox.Value = hashCountBox.Minimum;

            hashCountBox.Value = hashCountSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                Application.DoEvents();

                GetHashCount();
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private void FormCrypt_FormClosed(object sender, FormClosedEventArgs e)
        {
            BytesBuilder.ToNull(key);
            BytesBuilder.ClearString(FileName);
        }

        private void CryptButton_Click(object sender, EventArgs e)
        {
            FileEncrypt(key, FileName);
        }

        private void FileEncrypt(byte[] key, string FileName)
        {
            CryptButton .Enabled = false;
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите режим шифрования", "Шифрование файла " + FileName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                CryptButton .Enabled = true;
                return;
            }

            if (File.Exists(CryptFileName))
                if (MessageBox.Show("Файл, в который будет произведено шифрование, уже существует. Перезаписать?", "Шифрование файла " + FileName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                {
                    CryptButton .Enabled = true;
                    return;
                }
                else
                    File.Delete(CryptFileName);

            if (comboBox1.SelectedIndex == 3)
            {/*
                var fi = new FileInfo(FileName);
                var sha   = new SHA3(key.Length << 4);
                SHA3.ProgressObject progress = new SHA3.ProgressObject();
                sha.parallelCrypt(FileName, CryptFileName, false, key, null, (byte) (comboBox1.SelectedIndex + 20), progress, (int) hashCountBox.Value);
              * */
            }
            else
            {
                var regime = 40;
                if (comboBox1.SelectedIndex > 0)
                    regime = 41;
                int  hashCount = (int) hashCountBox.Value;
                bool LZMA = !nonLZMA.Checked;

                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        FileEncrypt(key, FileName, regime, hashCount, LZMA);
                    }
                );

                richTextBox1.Text = "Идёт шифрование файла. Ждите.\r\n\r\n" + "После шифрования удалите файл, который шифровали, если в этом есть необходимость";
            }
        }

        public void postFileEncrypt()
        {
            CryptButton .Visible = false;
            OpenButton  .Visible = true;
            checkButton .Visible = true;
            DeleteButton.Visible = true;
            CloseButton .Visible = true;
            richTextBox1.Text    = "Файл зашифрован\r\nПроверьте возможность расшифровки\r\nУдалите нешифрованный файл, если это требуется";

            this.BringToFront();
            this.Activate();
        }

        public delegate void postFileEncryptFunc();

        private void FileEncrypt(byte[] key, string FileName, int regime, int hashCount, bool LZMA)
        {
            GC.Collect();

            var bytes = File.ReadAllBytes(FileName);
            var sha = new SHA3(bytes.LongLength);

            var crypted = sha.multiCryptLZMA(bytes, key, null, (byte)regime, LZMA, (byte)(LZMA ? 19 : 0), hashCount);
            BytesBuilder.ToNull(key);
            BytesBuilder.ToNull(bytes);
            sha.Clear(true);
            bytes = null;
            key = null;

            File.WriteAllBytes(CryptFileName, crypted);
            BytesBuilder.ToNull(crypted);
            crypted = null;

            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new postFileEncryptFunc(this.postFileEncrypt));
                }
                catch
                {
                    postFileEncrypt();
                }
            }
            else
                postFileEncrypt();

            // MessageBox.Show("Файл зашифрован\r\nПроверьте возможность расшифровки\r\nУдалите нешифрованный файл, если это требуется", "Шифрование файла " + FileName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        [DllImport("Kernel32.dll")]
        public static extern Int32 CreateFile(string lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, Int32 lpSecurityAttributes,
                                                                Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("Kernel32.dll")]
        public static extern Int32 SetFilePointerEx(Int32 hFile, long liDistanceToMove, out long lpNewFilePointer, int dwMoveMethod);

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 ReadFile(Int32 hFile, byte* buffer, int nNumberOfBytesToRead, out int bytesReaded, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 ReadFile(Int32 hFile, byte[] buffer, int nNumberOfBytesToRead, out int bytesReaded, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 WriteFile(Int32 hFile, byte[] buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        public static unsafe extern Int32 WriteFile(Int32 hFile, byte* buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        public static extern Int32 CloseHandle(Int32 lpBaseAddress);

        [DllImport("Kernel32.dll")]
        public static extern Int32 FlushFileBuffers(Int32 hFile);

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Хотите удалить исходный (незашифрованный) файл\r\n" + FileName + " ?\r\n\r\nОтменить операцию будет невозможно, файл не попадёт в корзину!", "Удаление незашифрованного файла", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                return;

            var ddso = new Form1.DoDataSanitizationObject();
            var f = new DataSanitizationProgressForm(ddso);
            f.Show();
            f.Focus();

            System.Threading.ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    Form1.DoDataSanitization(FileName, ddso);
                }
            );
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.GetDirectoryName(FileName));
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            CheckCrypt();
        }

        private void CheckCrypt()
        {
            var Success = false;
            FileInfo fi; byte[] bytes = null, bt;

            var cnt = new Form1.OpenFileDialogContext(this);
            cnt.dialog.FileName = CryptFileName;

            Form1.GetPasswordAndDecryptFile(cnt, out fi, out bt, false, true);
            if (bt != null)
            {
                if (File.Exists(FileName))
                {
                    bytes = File.ReadAllBytes(FileName);
                    Success = BytesBuilder.Compare(bytes, bt);
                    BytesBuilder.ToNull(bytes);
                    BytesBuilder.ToNull(bt);
                }
            }


            if (Success && bytes != null)
                MessageBox.Show("Расшифровка файла произошла успешно", "Проверка шифрования", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            if (Success)
                MessageBox.Show("Проверка расшифровки полностью невозможна, так как не найден исходный файл\r\nРасшифровка файла произошла успешно", "Проверка шифрования", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show("Файл не удалось расшифровать правильно с этим паролем/ключом", "Проверка шифрования", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
