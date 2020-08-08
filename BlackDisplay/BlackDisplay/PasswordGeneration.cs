using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using keccak;
using System.IO;

namespace BlackDisplay
{
    public partial class PasswordGeneration : Form
    {
        protected static PasswordGeneration pwdForm = null;
        protected PasswordGeneration()
        {
            InitializeComponent();
            inBox.isHaotic = true;
            inBox.isMouseAllow = MouseAllowBox.Checked;
            PwdLengthCombo.SelectedIndex = PwdLengthCombo.Items.Count - 1;

            inBox.isGlobalBackground = GlobalHookPwdCheckBox.Checked;
        }

        public static PasswordGeneration openedPwdForm//
        {
            get
            {
                return pwdForm;
            }
        }

        public static PasswordGeneration newPasswordGeneration()
        {
            if (pwdForm != null)
                return pwdForm;

            pwdForm = new PasswordGeneration();
            return pwdForm;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void ClearInput()
        {
            inBox.ClearHaotic();
        }

        string latinChars      = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        string latinLowerChars = "qwertyuiopasdfghjklzxcvbnm1234567890";
        string yandexChars     = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890!@#$%^&*()+_"; //"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890!@#$%^&*()_-+:;,.";
        string numberChars     = "0123456789";
        string allLatinChars   = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890~`!@#$%^&*()_-+=|\\{[}]:;\"'<,>.?/";
        string hexChars        = "0123456789ABCDEF";

        int    generated = 0;
        byte[] duplex = null;
        string pwd    = null;
        string Base64 = null;
        private int Generate(int count, bool base64)
        {
            var input = SimplePasswordBox.inputBytesHaotic;
            if ((input.Length / 9) < 64)
            {
                MessageBox.Show("Введите хотя бы 64 бита случайной информации", "Генерация случайного ключа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }
/*
            var len = (input.Length / 9) >> 3;
            if (count <= 0)
                count = len;

            var genCount = (int) Math.Ceiling((float) count / (float) len);

            duplex = keccak.SHA3.generateRandomPwd(input, genCount);*/

            duplex = keccak.SHA3.generateRandomPwdByDerivatoKey(input, count, false, 40);

            int count64;
            if (count <= 0)
                count64 = duplex.Length;
            else
                count64 = count;

            if (count64 > duplex.Length)
            {
                count64 = duplex.Length;
                MessageBox.Show("Извините, но удалось сгенерировать только " + count64 + " байтов (" + count64 * 8 + " битов) информации", "Генерация ключа: предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            keccak.BytesBuilder.ClearString(Base64);
            keccak.BytesBuilder.ClearString(pwd);

            if (base64)
            {
                var base64Bytes = new byte[count64];
                keccak.BytesBuilder.CopyTo(duplex, base64Bytes);

                Base64 = Convert.ToBase64String(base64Bytes);

                keccak.BytesBuilder.BytesToNull(base64Bytes);
            }
            else
            {
                generatePwd(count64);
            }

            if (count64 > generated)
                generated = count64;

            keccak.BytesBuilder.BytesToNull(duplex);

            return count64;
        }

        private void generatePwd(int count)
        {
            var pwd1 = "";
            switch (comboBox1.SelectedIndex)
            {
                case 1:
                    pwd1 = keccak.SHA3.generatePwd(duplex, yandexChars);
                    break;
                case 2:
                    pwd1 = keccak.SHA3.generatePwd(duplex, numberChars);
                    break;
                case 3:
                    pwd1 = keccak.SHA3.generatePwd(duplex, allLatinChars);
                    break;
                case 4:
                    pwd1 = keccak.SHA3.generatePwd(duplex, hexChars);
                    break;
                case 5:
                    pwd1 = keccak.SHA3.generatePwd(duplex, hexChars);
                    pwd1 = pwd1.Insert(20, "-");
                    pwd1 = pwd1.Insert(16, "-");
                    pwd1 = pwd1.Insert(12, "-");
                    pwd1 = pwd1.Insert(8, "-");        // // {D3BF42FE-333B-438D-B9B6-85251005BEDA}   // 8+4+4+4+12=32
                    count = 32+4;
                    break;
                case 6:
                    pwd1 = keccak.SHA3.generatePwd(duplex, latinLowerChars);
                    break;
                default:
                    pwd1 = keccak.SHA3.generatePwd(duplex, latinChars);
                    break;
            }

            pwd = pwd1.Substring(0, 1) + pwd1.Substring(1, count - 1);  // Иначе строка не всегда копируется
            keccak.BytesBuilder.ClearString(pwd1);
        }

        private void PasswordGeneration_Load(object sender, EventArgs e)
        {
            PwdLengthCombo   .SelectedIndex = PwdLengthCombo.Items.Count - 1;
            base64LengthCombo.SelectedIndex = 1;
        }

        // Осторожно, этот код дублирован в генерации данных в файл
        private void generate_Click(object sender, EventArgs e)
        {
            try
            {
                int count = -1;

                if (base64LengthCombo.SelectedIndex == -1)
                    count = Int32.Parse(base64LengthCombo.Text);
                else
                    if (base64LengthCombo.SelectedIndex > 0)
                        count = 4 << base64LengthCombo.SelectedIndex;   // 32 бита сдвинуть на base64LengthCombo.SelectedIndex влево (64, 128, 256, 512)

                if (Generate(count, true) != 0)
                    Base64ToClipboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Извините, вы ввели некорректную информацию в поле ввода длинны ключа: " + ex.Message, "Генерация ключа", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Base64ToClipboard()
        {
            // Для работы с буфером обмена приходится извращаться, т.к. он поддерживает только потоки STAThread
            /*var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    Clipboard.SetText(Base64);
                },
                64 * 1024
                );

            thr.SetApartmentState(ApartmentState.STA);
            thr.Start();*/

            pwdToClipboard(Base64);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int count = -1;

                if (PwdLengthCombo.SelectedIndex == -1)
                    count = Int32.Parse(PwdLengthCombo.Text);
                else
                    if (PwdLengthCombo.SelectedIndex > 0)
                        count = 15 + PwdLengthCombo.SelectedIndex * 3;
                    else
                        count = (int) SimplePasswordBox.countOfHaoticBytes / 9;

                if (Generate(count, false) != 0)
                    pwdToClipboard(pwd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Извините, вы ввели некорректную информацию в поле ввода длинны пароля: " + ex.Message, "Генерация пароля", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void pwdToClipboard(string pwd)
        {
            var t = new object();
            // Для работы с буфером обмена приходится извращаться, т.к. он поддерживает только потоки STAThread
            var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        Clipboard.SetText(pwd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалась работа с буфером обмена: " + ex.Message, "Генерация пароля", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Monitor.Wait(t);
            }
        }

        private void MouseAllowBox_CheckedChanged(object sender, EventArgs e)
        {
            inBox.isMouseAllow = MouseAllowBox.Checked;
        }

        private void PasswordGeneration_FormClosed(object sender, FormClosedEventArgs e)
        {
            keccak.BytesBuilder.ClearString(Base64);
            keccak.BytesBuilder.ClearString(pwd);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            inBox.isGlobalBackground = GlobalHookPwdCheckBox.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TrimGeneratedBytes();
        }

        private void TrimGeneratedBytes()
        {
            inBox.ClearFirstInputBytesInBox(generated);
            generated = 0;
        }

        private void PasswordGeneration_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void PasswordGeneration_FormClosing(object sender, FormClosingEventArgs e)
        {
            TrimGeneratedBytes();

            pwdForm  = null;
            e.Cancel = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            checkBufferPwdToStrength();
        }

        private static void checkBufferPwdToStrength()
        {
            object t = new object();
            string pwdToCheck = null;

            var thr = new Thread
                ((ThreadStart)
                delegate
                {

                    try
                    {
                        pwdToCheck = Clipboard.GetText();
                    }
                    catch
                    {
                        pwdToCheck = "";
                    }
                    finally
                    {
                        lock (t)
                            Monitor.Pulse(t);
                    }
                }/*,
                64 * 1024*/     // кажется, при включённом emet начинаются проблемы, если установлен размер стека
                );

            lock (t)
            {
                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                Monitor.Wait(t);
            }

            if (pwdToCheck.Length < 8 && pwdToCheck.Length > 4096)
            {
                MessageBox.Show("В буфере обмена нет пароля или не удалось считать пароль (проверяемый пароль должен быть не менее 8-ми символов и не более 4096-ми)");
                return;
            }

            keccak.SHA3.PwdCheckResult checkResult;
            keccak.SHA3.checkPwd(pwdToCheck, out checkResult);
            keccak.BytesBuilder.ClearString(pwdToCheck);

            new PwdCheckResultForm(checkResult).ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            generatePasswordTable("-.абвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789");
        }

        
        private void button5_Click(object sender, EventArgs e)
        {
            generatePasswordTable("-.abcdefghijklmnopqrstuvwxyz0123456789");
        }

        private void generatePasswordTable(string c)
        {
            var input = SimplePasswordBox.inputBytesHaotic;

            if ((input.Length / 9) < 64)
            {
                MessageBox.Show("Введите хотя бы 64 бита случайной информации", "Генерация случайного ключа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var duplex = keccak.SHA3.generateRandomPwdByDerivatoKey(input, input.Length, false, 40);
            byte c1 = (byte) c.Length, c2 = c1;
            var table = SHA3.getPasswordCypherTable(duplex, c1, c2);
            BytesBuilder.ToNull(duplex);

            int gc1 = 20, gc2 = 8, gc3 = 1;
            var b = new Bitmap((c1+1)*(gc1+gc2) + gc3*2, (c2+1)*(gc1+gc2) + gc3*2, this.CreateGraphics());
            var g = Graphics.FromImage(b);
            g.Clear(Color.White);

            var f  = new Font("Courier New", gc1);
            var p1 = new Pen(Color.Black, 1f);
            var p2 = new Pen(Color.Gray, 3f);
            var brush  = new SolidBrush(Color.Black);
            var brushw = new SolidBrush(Color.White);

            int x2 = 0, y2 = 0;
            for (int i = 0; i < c2; i++)
            {
                g.DrawLine((i & 1) > 0 ? p1 : p2, gc3, gc3 + (gc1+gc2)*(i+1), b.Width - gc3, gc3 + (gc1+gc2)*(i+1));
                g.DrawString(c.Substring(i, 1), f, brush, -(gc2>>1) + gc3 + gc2, /*-(gc2>>1) +*/ gc3 + (gc1+gc2)*(i+1));

                if (y2 == 0 && i > (c2 >> 1))
                    y2 = gc3 + (gc1+gc2)*(i+1);
            }

            for (int j = 0; j < c1; j++)
            {
                g.DrawLine((j & 1) > 0 ? p1 : p2, gc3 + (gc1+gc2)*(j+1), gc3, gc3 + (gc1+gc2)*(j+1), b.Height-gc3);
                g.DrawString(c.Substring(j, 1), f, brush, gc3 + (gc1+gc2)*(j+1), gc3/*-(gc2>>1)*/);

                if (x2 == 0 && j > (c1 >> 1))
                    x2 = gc3 + (gc1+gc2)*(j+1);
            }

            for (int i = 0; i < c2; i++)
            {
                for (int j = 0; j < c1; j++)
                {
                    var t = table[j*c2 + i];
                    g.DrawString(c.Substring(t%c2, 1), f, brush, gc3 + (gc1+gc2)*(i+1), /*-(gc2>>1) +*/ gc3 + (gc1+gc2)*(j+1));
                }
            }

            BytesBuilder.ToNull(table);

            var b1 = b.Clone(new Rectangle(0, 0, b.Width, /*(b.Height >> 1) + gc1+gc3*/y2+3), b.PixelFormat);
            var b2 = b.Clone(new Rectangle(0, /*(b.Height >> 1) - gc1+gc3*/y2 - gc1 - gc2 - gc3, b.Width, b.Height - (y2 - gc1 - gc2 - gc3)/*(b.Height >> 1) + gc1-gc3*/), b.PixelFormat);

            var g2 = Graphics.FromImage(b2);
            g2.FillRectangle(brushw, 0, 0, b2.Width, gc1+gc3+gc2);
            for (int j = 0; j < c1; j++)
            {
                g2.DrawLine((j & 1) > 0 ? p1 : p2, gc3 + (gc1+gc2)*(j+1), gc3, gc3 + (gc1+gc2)*(j+1), b.Height-gc3);
                g2.DrawString(c.Substring(j, 1), f, brush, gc3 + (gc1+gc2)*(j+1), gc3/*-(gc2>>1)*/);
            }

            b1.Save(System.IO.Path.Combine(Application.StartupPath, "tmp.gif"), System.Drawing.Imaging.ImageFormat.Gif);
            b2.Save(System.IO.Path.Combine(Application.StartupPath, "tmp2.gif"), System.Drawing.Imaging.ImageFormat.Gif);
            g.Clear(Color.White);
            Graphics.FromImage(b1).Clear(Color.White);
            Graphics.FromImage(b2).Clear(Color.White);

            System.Diagnostics.Process.Start(Application.StartupPath);
        }

        // Осторожно, этот код частично дублирован в генерации данных в формате Base64
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                int count = -1;

                if (base64LengthCombo.SelectedIndex == -1)
                    count = Int32.Parse(base64LengthCombo.Text);
                else
                    if (base64LengthCombo.SelectedIndex > 0)
                        count = 4 << base64LengthCombo.SelectedIndex;   // 32 бита сдвинуть на base64LengthCombo.SelectedIndex влево (64, 128, 256, 512)

                var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    if (Generate(count, true) != 0)
                    try
                    {
                        var sfd = new SaveFileDialog();
                        if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            return;

                        File.WriteAllBytes(sfd.FileName, Convert.FromBase64String(Base64));
                        MessageBox.Show("Записано в файл " + sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Извините, вы ввели некорректную информацию в поле ввода длинны ключа: " + ex.Message, "Генерация ключа", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                );

                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Извините, вы ввели некорректную информацию в поле ввода длинны ключа: " + ex.Message, "Генерация ключа", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
