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
    public partial class Uninstall : Form
    {
        string retext = "В поле ниже можно ввести подробный отзыв или оставить поле пустым.\r\nЕсли вы хотите, чтобы разработчики могли ответить вам, укажите ваш e-mail";
        public Uninstall()
        {
            InitializeComponent();
            label3.Text = retext;

            GetMsgIdString();
#if forLinux
#else
            Form1.registerHooks(false);
#endif

            // Пробиваем сервер яндекса одним письмом заранее, ещё перед написанием сообщения
            // doSendAsync(Program.version + "\r\n" + "Служебно-вспомогательное", msgId, 1, false, 40000);
        }

        public Uninstall(string msg): this()
        {
            if (msg == null)
                label1.Text = "Программа успешно удалена с Вашего компьютера.\r\nПапка программы может быть удалена Вами вручную";
            else
                label1.Text = "Из директории " + AppDomain.CurrentDomain.BaseDirectory + " программу удалить не удалось, ошибка " + msg;
        }

        private string getBoxMessage()
        {
            var sb = new StringBuilder("\r\n\r\n");

            foreach (var line in checkedListBox1.CheckedItems)
            {
                sb.AppendLine(line.ToString());
            }

            return sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        volatile bool mailSended = false;
        volatile bool msResult   = false;
        volatile string msgId = "";
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            msResult   = false;
            try
            {
                label4.Text = "Идёт отправка...";

                doSendAsync(Program.version + "\r\n" + richTextBox1.Text + getBoxMessage(), msgId, 4, true);

                richTextBox1.Enabled = false;
                checkedListBox1.Enabled = false;
                while (!sendingMail && !sendingMailEnded)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }
            }
            finally
            {
                do
                {
                    Application.DoEvents();
                }
                while (!sendMailMutex.WaitOne(100));

                sendMailMutex.ReleaseMutex();
                sendMailMutex.Close();


                button1.Enabled = /*!msResult*/ true;
                if (!msResult)
                    button1.BackColor = Color.Red;

                richTextBox1.Enabled = true;
                checkedListBox1.Enabled = true;

                Application.DoEvents();

                if (msResult)
                {
                    mailSended = true;
                    label4.Text = "Отправлено";
                    MessageBox.Show("Сообщение послано, спасибо за отзыв!\r\n\r\nЕсли вы не указали ваш e-mail, помните, что разработчики не знают его и не смогут с вами связаться.");
                }
                else
                {
                    label4.Text = "Неудачно";
                    MessageBox.Show("К сожалению, сообщение послать не удалось. Вы можете его послать вручную на адрес prg@8vs.ru, указанный в меню 'О программе', либо попробовать второй раз через несколько минут");
                }
            }
        }

        private void GetMsgIdString()
        {
            try
            {
                var updateOptions = new options.OptionsHandler("updator/uopts.txt");
                if (updateOptions.contains("updatorGUID"))
                    msgId = updateOptions["updatorGUID", ""];
            }
            catch
            {}
        }

        System.Threading.Mutex sendMailMutex = new System.Threading.Mutex();
        volatile bool terminateSendMail = false;
        volatile bool sendingMail = false;
        volatile bool sendingMailEnded = false;
        private void doSendAsync(string text, string msgId, int maxCount, bool toGlobalResult, int TimeOut = 0)
        {
            sendingMailEnded = false;
            sendingMail = false;

            System.Threading.ThreadPool.QueueUserWorkItem
            (delegate
            {
                try
                {
                    sendMailMutex.WaitOne();
                    sendingMail = true;

                    bool result = false;
                    for (int i = 0; i < maxCount && !result; i++)
                    {

                        try
                        {
                            result = options.SMTP_YandexMX.send(text, msgId, !toGlobalResult, TimeOut);

                            if (i < maxCount - 1 && !result)
                                for (int j = 0; j < 61; j++)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    if (terminateSendMail)
                                        return;
                                }
                        }
                        catch
                        {}
                    }

                    if (result && toGlobalResult)
                    {
                        msResult = true;
                    }
                }
                finally
                {
                    sendMailMutex.ReleaseMutex();
                    sendingMailEnded = true;
                    sendingMail = false;
                }
            });
        }

        private void Uninstall_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!mailSended)
            {
                if (MessageBox.Show("Вы уверены, что хотите выйти, не послав сообщения?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                    e.Cancel = true;
                else
                    e.Cancel = false;
            }
        }
    }
}
