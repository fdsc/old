using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BlackDisplay
{
    public partial class shortAsk : Form
    {
        public class shortAskOwner: IWin32Window
        {
            readonly IntPtr handle;
            public IntPtr Handle
            {
                get { return handle; }
            }

            public shortAskOwner()
            {
                #if forLinux
                handle = (IntPtr) 1;
                #else
                handle = Form1.GetForegroundWindow();
                #endif
            }
        }

        private shortAsk()
        {
            InitializeComponent();
        }

        public readonly Form1  form;
               readonly string optName;
        public shortAsk(Form1 f): this()
        {
            form = f;

            optName = form.currentShort == 1 ? Form1.optsName[19] : Form1.optsName[20];
            long shortTimeToBlack = (long) Form1.opts[optName, 0] * Form1.minute / 10;

            if (form.currentShort == 2)
                increaseButton.Text = "Увеличить до " + (Form1.opts[Form1.optsName[20], 0] + 1) + " * 1/10 мин";
            else
            {
                increaseButton.Text = "Перейти в замедленный режим";
                timer1.Interval = 5000;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            if (form.currentShort == 2)
            {
                var s = Form1.opts[Form1.optsName[20], 0] + 1;
                if (MessageBox.Show("Вы уверены, что хотите увеличить время ожидания в замедленном режиме до " + s + " * 1/10 минуты?", "Увеличить время ожидания?", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != System.Windows.Forms.DialogResult.Yes)
                {
                    timer1.Enabled = true;
                    return;
                }

                Form1.opts.add(Form1.optsName[20], s); // сохранение на автомате
                Close();
                return;
            }


            if (form.currentShort == 1)
            {
                form.ПерейтиВЗамедленный();
                Close();
                return;
            }

            throw new Exception("Не удалось распознать функцию кнопки shortAsk.button4");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Close();

            Application.DoEvents();

            // Перерегистрируем хуки, т.к. может оказаться, что у нас проблемы с тем, что хук слетел
#if forLinux
#else
            Form1.tagLASTINPUTINFO p;
            int result;
            long dwTime;
            Form1.GetDelayTime(out p, out result, out dwTime, true);
            if (dwTime > 3000)  // 1000 - 1 секунда
            {
                Program.toLogFileMessage("Попытка перерегистрации хуков; простой " + dwTime);
                Form1.registerHooks(true, true);
            }
#endif
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Form1.tagLASTINPUTINFO p;
            int result;
            long dwTime;
            Form1.GetDelayTime(out p, out result, out dwTime, Form1.opts[Form1.optsName[25], true]);

            // Если простой больше интервала действия таймера (миллисекунд), то бросто считаем, что пользователя на месте нет
            // Иначе ждём, пока пользователь сам среагирует
            if (dwTime > timer1.Interval)
                showBlack();
        }

        private void showBlack()
        {
            timer1.Enabled = false;
            form.blackVisible();
            Close();
        }

        private void shortAsk_Shown(object sender, EventArgs e)
        {
            timer1.Enabled = true;

            button4.Focus();
            /*if (form.currentShort == 1)
                increaseButton.Focus();
            else
            if (form.currentShort == 1)
                button3.Focus();*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showBlack();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            form.toSlowReaction();
            timer1.Enabled = false;
            Close();
        }

        private void shortAsk_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;

            form.shortAskForm = null;
            // Program.toLogFileMessage("Диалог запроса о бездействии закрыт в " + DateTime.Now.Ticks + " / " + DateTime.Now);
            options.DbgLog.dbg.messageToLog("", "Диалог запроса о бездействии закрыт в " + DateTime.Now.Ticks + " / " + DateTime.Now);
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                button2.Focus();
            else
            if (e.KeyCode == Keys.Left)
                increaseButton.Focus();
            else
            if (e.KeyCode == Keys.Down)
                button4.Focus();
            else
            if (e.KeyCode == Keys.Up)
                increaseButton.Focus();
            else
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                increaseButton.Focus();
            else
            if (e.KeyCode == Keys.Left)
                button1.Focus();
            else
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
                button3.Focus();
            else
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void increaseButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                button1.Focus();
            else
            if (e.KeyCode == Keys.Left)
                button2.Focus();
            else
            if (e.KeyCode == Keys.Up)
                button3.Focus();
            if (e.KeyCode == Keys.Down)
                button4.Focus();
            else
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void button3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                increaseButton.Focus();
            else
            if (e.KeyCode == Keys.Left)
                button1.Focus();
            else
            if (e.KeyCode == Keys.Down)
                button2.Focus();
            else
            if (e.KeyCode == Keys.Right )
                button4.Focus();
            else
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        
        private void button4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                increaseButton.Focus();
            else
            if (e.KeyCode == Keys.Left)
                button3.Focus();
            else
            if (e.KeyCode == Keys.Down)
                increaseButton.Focus();
            else
            if (e.KeyCode == Keys.Right )
                button1.Focus();
            else
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // Это нужно, чтобы стрелки обрабатывались так, как я указал
        private void PreviewKeyDownAll(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            form.ПерейтиВСмотрюФильм();

            Close();
        }
    }
}
