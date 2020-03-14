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

namespace BlackDisplay
{
    public partial class DoublePasswordForm : Form
    {
        static protected int opened = 0;
        static public    int OpenedCount
        {
            get
            {
                return opened;
            }
        }

        public readonly string FileName;
        public readonly int    PwdCount;
        public DoublePasswordForm(int pwdCount = 0, string fileName = "", bool byFileDisabled = false)
        {
            InitializeComponent();
            opened++;
            box1.Masked = true;
            ByFileButton.Enabled = !byFileDisabled;
            
            FileName = fileName;
            switch (pwdCount)
            {
                case 0: PwdCount = 0;
                    break;
                case 1: PwdCount = 1;
                    break;
                case 2: PwdCount = 2;
                    break;
                default:
                        PwdCount = 0;
                    break;
            }
        }

        private void box1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            if (!box1.isHaotic)
            MessageBox.Show("Введите пароль в данное окно\r\n" +
                            "Пароль должен быть длинным, рекомендуется делать часть пароля сложным, часть - длинным, например, длинной частью может служить отрывок стиха\r\nРекомендуемая длина стиха или прозы - не менее 12-ти слов, сложной части - не менее 8-ми символов"
                            , "Введите пароль", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void box2_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Повторите ввод пароля в данное окно для проверки его правильности", "Введите пароль", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoublePasswordForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            opened--;
            box1.Clear();
        }

        private void mod1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Сам пароль будет хранится после ввода до перезагрузки компьютера.\r\nЧтобы им не воспользовался другой человек, он защищён модификатором пароля, который нужно вводить каждый раз, когда вы хотите использовать пароль\r\nМодификатор может быть пустым", "Введите пароль", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public string resultText = null;
        private void EnterPwd_Click(object sender, EventArgs e)
        {
            if (oldPassword.Checked)
                resultText = box1.textReversed;
            else
                resultText = box1.text;

            cancel     = false;
            Close();
        }

        public void clearResultText()
        {
            if (String.IsNullOrEmpty(resultText))
                BytesBuilder.ClearString(resultText);
        }

        public bool cancel = false;
        private void Cancel_Click(object sender, EventArgs e)
        {
            clearResultText();
            resultText = null;
            cancel     = true;
            Close();
        }

        protected bool byFile = false;
        private void ByFileButton_Click(object sender, EventArgs e)
        {
            var cnt = new BlackDisplay.Form1.OpenFileDialogContext(this);
            cnt.dialog.InitialDirectory = "/";
            cnt.dialog.Multiselect = false;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = true;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.Title = "Выберите файл с паролем";

            cnt.closedEvent += new Form1.OpenFileDialogContext.closed(passwordInFile);

            //ByFileButton.Enabled = false;
            EnterPwd    .Enabled = false;
            box1        .Enabled = false;
            vd = focus;
            vc = close;
            cnt.show();
        }

        delegate void voidDelegate();
        voidDelegate vd, vc;
        public void focus()
        {
            ByFileButton.Enabled = true;
            EnterPwd    .Enabled = true;
            box1        .Enabled = true;
            Focus();
        }

        // Используется при вводе пароля из файла
        public void close()
        {
            cancel     = false;
            Close();
        }

        public bool fromFile = false;
        protected void passwordInFile(Form1.OpenFileDialogContext cnt, bool isOk)
        {
            if (!isOk)
            {
                try
                {
                    if (this.InvokeRequired)
                        this.Invoke(vd);
                    else
                        vd();
                }
                catch
                {
                    vd();
                }
                return;
            }

            FileInfo fi;
            byte[] bt;
            if (!Form1.GetPasswordAndDecryptFile(cnt, out fi, out bt, false, false, true))
            {
                try
                {
                    if (this.InvokeRequired)
                        this.Invoke(vd);
                    else
                        vd();
                }
                catch
                {
                    vd();
                }
                return;
            }

            if (bt == null)
                bt = File.ReadAllBytes(fi.FullName); //File.ReadAllText(fi.FullName, Encoding.Unicode); Little Endian

            resultText = Encoding.Unicode.GetString(bt);
            if (resultText[0] == (char) 0xFEFF)
                resultText = resultText.Substring(1);
            /*
            int i = resultText.IndexOf("01234567890123456789");
            if (i > 0)
                resultText = resultText.Substring(0, i);
            */
            fromFile = true;
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(vc);
                else
                    vc();
            }
            catch
            {
                vc();
            }
        }

        private void DoublePasswordForm_Shown(object sender, EventArgs e)
        {
            var str1 = PwdCount == 2 ? "Второй ввод пароля" : 
                (PwdCount == 1 ? "Первый ввод пароля" : "Введите пароль");

            if (!String.IsNullOrEmpty(FileName))
                this.Text = str1 + " к файлу " + FileName;
        }
    }
}
