using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace BlackDisplay
{
    public partial class HashCalcerForm : Form
    {
        protected HashCalcerForm()
        {
            InitializeComponent();
            hashTypeBox.SelectedIndex = 0;
        }

        public static void ShowForm()
        {
            var sync = new object();
            HashCalcerForm result = null;
            /*
            // Диалог открытия файла поддерживает только потоки STAThread
            var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        result = new HashCalcerForm();
                        result.Show();
                    }
                    finally
                    {
                        lock (sync)
                            Monitor.Pulse(sync);
                    }
                }
                );

            lock (sync)
            {
                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                Monitor.Wait(sync);*/
                /*while (!Monitor.Wait(sync, 0))
                {
                    Application.DoEvents();
                }*/
            /*}*/

            result = new HashCalcerForm();
            result.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            Application.DoEvents();
            //calcHash();
            openFileDialogWindow();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        string fileName;
        object sync = new object();
        public void openFileDialogWindow()
        {
            fileName = null;

            if (hashTypeBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Сначала выберите, какой именно хеш вы хотите рассчитать");
                return;
            }


            try
            {
                #if forLinux
                    if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return;

                    fileName = openFileDialog1.FileName;
                    calcHash();
                #else
                var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            return;

                        fileName = openFileDialog1.FileName;
                        this.Invoke(new openFileDialogWindowDelegate(calcHash));
                    }
                    finally
                    {
                        /*lock (sync)
                        {
                            Monitor.Pulse(sync);
                        }*/
                    }
                }
                );

                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                #endif
            }
            finally
            {
                /*lock (sync)
                    Monitor.Wait(sync);*/
                /*
                lock (sync)
                while (!Monitor.Wait(sync, 50))    // так никогда не выйдет, видимо, своё место играет 0
                {
                    Application.DoEvents();
                }*/
            }
        }

        delegate void openFileDialogWindowDelegate();

        private void calcHash()
        {/*
            if (this.InvokeRequired)
            {
                lock (sync)
                {
                    this.Invoke(new openFileDialogWindowDelegate(openFileDialogWindow));*/
                    /*while (!Monitor.Wait(sync, 0))    // так никогда не выйдет, видимо, своё место играет 0
                    {
                        Application.DoEvents();
                    }*/
                    /*Monitor.Wait(sync);
                }
            }
            else
                openFileDialogWindow();*/

            // openFileDialogWindow();

            if (fileName == null)
                return;

            richTextBox1.Text = "Подождите, хэш вычисляется";
            int si = hashTypeBox.SelectedIndex;

            ThreadPool.QueueUserWorkItem
            (
            delegate
            {
            try
            {
            byte[] data = null;
            if (si < 7  || si > 10)
            if (si < 12 || si > 15)
            data = File.ReadAllBytes(fileName);

            byte[] hash = null;
            string base64 = null;
            switch (si)
            {
                case 1:
                    hash = new MD5Cng().ComputeHash(data);
                    break;
                case 2:
                    hash = new SHA1Cng().ComputeHash(data);
                    break;
                case 3:
                    hash = new RIPEMD160Managed().ComputeHash(data);
                    break;
                case 4:
                    hash = new SHA256Cng().ComputeHash(data);
                    break;
                case 5:
                    hash = new SHA384Cng().ComputeHash(data);
                    break;
                case 6:
                    hash = new SHA512Cng().ComputeHash(data);
                    break;
                case 7:
                    hash = GetHash224(fileName);
                    break;
                case 8:
                    hash = GetHash256(fileName);
                    break;
                case 9:
                    hash = GetHash384(fileName);
                    break;
                case 10:
                    hash = GetHash512(fileName);
                    break;
                case 11:
                    base64 = Convert.ToBase64String(data);
                    break;
                /*case 12:
                    hash = keccak.Gost_34_11_2012.getHash256(data, false, false);
                    break;
                case 13:
                    hash = keccak.Gost_34_11_2012.getHash512(data, false, false);
                    break;*/
                case 12:
                    hash = GetHash256_3411(fileName);
                    //hash = HexStringToBytes(BitConverter.ToString(hash).Replace("-", "").ToLower());
                    break;
                case 13:
                    hash = GetHash512_3411(fileName);
                    //hash = HexStringToBytes(BitConverter.ToString(hash).Replace("-", "").ToLower());
                    break;
                case 14:
                    hash = GetHash256_3411(fileName, true);
                    break;
                case 15:
                    hash = GetHash512_3411(fileName, true);
                    break;
                default:
                    MessageBox.Show("Ошибка в программе: хеш выбран, но в программе не предусмотрен его расчёт. Сообщите разработчику");
                    return;
            }

            this.Invoke(new hashToTextBoxDelegate(hashToTextBox), hash, base64, "");
            }
            catch (Exception ex)
            {
                this.Invoke(new hashToTextBoxDelegate(hashToTextBox), null, null, ex.Message);
            }
            });
        }

        delegate void VOID();

        private byte[] GetHash256_3411(string fileName, bool reversed = false)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 64*1024*1024)
            {
                var data = File.ReadAllBytes(fileName);
                return keccak.Gost_34_11_2012.getHash256(data, reversed);
            }

            VisibleProgressBar();

            var buff  = new byte[64*1024*1024];
            var g3411 = new keccak.Gost_34_11_2012();

            byte[] hash = null;
            using (var or = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 512, FileOptions.WriteThrough))
            {
                int  tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    hash = keccak.Gost_34_11_2012.getHash256(buff, reversed, true, false, tmp, isInit, tmp != buff.Length, all, g3411);
                    isInit = true;

                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            try
            {
                this.Invoke
                (
                    new VOID
                    (
                        delegate()
                        {
                            this.progressBar1.Visible = false;
                        }
                    )
                );
            }
            catch
            {}

            return hash;
        }

        private byte[] GetHash512_3411(string fileName, bool reversed = false)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 64 * 1024 * 1024)
            {
                var data = File.ReadAllBytes(fileName);
                return keccak.Gost_34_11_2012.getHash512(data, reversed);
            }

            VisibleProgressBar();

            var buff = new byte[64 * 1024 * 1024];
            var g3411 = new keccak.Gost_34_11_2012();

            byte[] hash = null;
            using (var or = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 512, FileOptions.WriteThrough))
            {
                int tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    hash = keccak.Gost_34_11_2012.getHash512(buff, reversed, true, false, tmp, isInit, tmp != buff.Length, all, g3411);
                    isInit = true;
                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            ProgressBarUnvisible();

            return hash;
        }

        private void ProgressBarUnvisible()
        {
            try
            {
                this.Invoke
                (
                    new VOID
                    (
                        delegate ()
                        {
                            this.progressBar1.Visible = false;
                        }
                    )
                );
            }
            catch
            { }
        }

        private void ProgressToProgressBar(long fl, long all)
        {
            try
            {
                this.Invoke
                (
                    new VOID
                    (
                        delegate ()
                        {
                            this.progressBar1.Value = (int)(all * 1000 / fl);
                        }
                    )
                );
            }
            catch
            { }
        }

        private void VisibleProgressBar()
        {
            try
            {
                this.Invoke
                (
                    new VOID
                    (
                        delegate ()
                        {
                            this.progressBar1.Visible = true;
                            this.progressBar1.Value = 0;
                        }
                    )
                );
            }
            catch
            { }
        }

        private byte[] GetHash224(string fileName)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 72*1024*1024)
            {
                var data = File.ReadAllBytes(fileName);
                return new keccak.SHA3(data.Length).getHash224(data);
            }

            VisibleProgressBar();

            var buff = new byte[72*1024*1024];
            var hash = new byte[64];
            var sha3 = new keccak.SHA3(buff.Length);

            using (var or = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.WriteThrough))
            {
                int  tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    sha3.getHash224(buff, tmp, isInit, tmp != buff.Length, hash);
                    isInit = true;
                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            ProgressBarUnvisible();
            return hash;
        }

        private byte[] GetHash256(string fileName)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 72*1024*1024)
            {
                var data = File.ReadAllBytes(fileName);
                return new keccak.SHA3(data.Length).getHash256(data);
            }

            VisibleProgressBar();

            var buff = new byte[72*1024*1024];
            var hash = new byte[64];
            var sha3 = new keccak.SHA3(buff.Length);
            using (var or = File.OpenRead(fileName))
            {
                int  tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    sha3.getHash256(buff, tmp, isInit, tmp != buff.Length, hash);
                    isInit = true;
                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            ProgressBarUnvisible();
            return hash;
        }

        private byte[] GetHash384(string fileName)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 72*1024*1024)
            {
                var data = File.ReadAllBytes(fileName);
                return new keccak.SHA3(data.Length).getHash384(data);
            }

            VisibleProgressBar();

            var buff = new byte[72*1024*1024];
            var hash = new byte[64];
            var sha3 = new keccak.SHA3(buff.Length);
            using (var or = File.OpenRead(fileName))
            {
                int  tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    sha3.getHash384(buff, tmp, isInit, tmp != buff.Length, hash);
                    isInit = true;
                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            ProgressBarUnvisible();
            return hash;
        }

        private byte[] GetHash512(string fileName)
        {
            var fl = new FileInfo(fileName).Length;
            if (fl < 72*1024*1024)
            {
                var data = File.ReadAllBytes(fileName);
                return new keccak.SHA3(data.Length).getHash512(data);
            }

            VisibleProgressBar();

            var buff = new byte[72*1024*1024];
            var hash = new byte[64];
            var sha3 = new keccak.SHA3(buff.Length);
            using (var or = File.OpenRead(fileName))
            {
                int  tmp = 0;
                long all = 0;
                bool isInit = false;
                do
                {
                    tmp = or.Read(buff, 0, buff.Length);
                    all += tmp;
                    sha3.getHash512(buff, tmp, isInit, tmp != buff.Length, hash);
                    isInit = true;
                    ProgressToProgressBar(fl, all);
                }
                while (tmp > 0 && tmp == buff.Length);
            }

            ProgressBarUnvisible();
            return hash;
        }

        delegate void hashToTextBoxDelegate(byte[] hash, string base64 = null, string error = "");
        private void hashToTextBox(byte[] hash, string base64 = null, string error = "")
        {
            if (base64 != null)
                richTextBox1.Text = base64;
            else
            {
                if (hash == null)
                    richTextBox1.Text = "Вычисление закончилось ошибкой " + error;
                else
                    richTextBox1.Text = BitConverter.ToString(hash).Replace("-", "");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = richTextBox1.Text.ToLower();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (hashStringCanonize(richTextBox2.Text) == hashStringCanonize(richTextBox1.Text))
                MessageBox.Show("Хеши равны");
            else
                MessageBox.Show("Хэши различны");
        }

        static Regex r = new Regex("[^0-9abcdef]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static string hashStringCanonize(string str)
        {
            return r.Replace(str.Trim().ToLower(), "", str.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var str = r.Replace(richTextBox1.Text, "", richTextBox1.Text.Length);
            for (int i = str.Length - 8; i > 0; i -= 8)
                str = str.Insert(i, " ");

            richTextBox1.Text = str;
        }

        private void hashTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private unsafe void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = reverseString(richTextBox1.Text);
        }

        unsafe private static string reverseString(string str)
        {
            var sb = new StringBuilder(str.Length);
            for (int j = str.Length - 1; j >= 0; j--)
            {
                sb.Append(str[j]);
            }
            return sb.ToString();
        }

        public static byte[] HexStringToBytes(string hexMessage)
        {
            var bt = new keccak.BytesBuilder();

            var sb = new StringBuilder();
            for (int i = 0; i < hexMessage.Length; i += 2)
            {
                bt.addByte(  Convert.ToByte(hexMessage.Substring(i, 2), 16)  );
            }

            return bt.getBytes();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var r = HexStringToBytes(hashStringCanonize(richTextBox1.Text));
            Array.Reverse(r);
            richTextBox1.Text = BitConverter.ToString(r).Replace("-", "").ToLower();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = hashStringCanonize(richTextBox1.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = richTextBox1.Text.ToUpper();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                var r = HexStringToBytes(hashStringCanonize(richTextBox1.Text));
                richTextBox1.Text = Convert.ToBase64String(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
