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
    public partial class DataSanitizationProgressForm : Form
    {
        private DataSanitizationProgressForm()
        {
            InitializeComponent();
        }

        public readonly Form1.DoDataSanitizationObject ddso;
        public DataSanitizationProgressForm(Form1.DoDataSanitizationObject ddso): this()
        {
            this.ddso = ddso;
            progressBar1.Value = (int) ddso.percent;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
                return;

            var percent = ddso.percent;
#if !DEBUG
            if (percent > 100f)
                percent = 100f;
#endif

            if (ddso.ts > 0)
            {
                try
                {
                    label1.Text = new DateTime((long) (ddso.ts*1000.0*100.0 * (100.0-percent))).ToString("HH:mm:ss");
                }
                catch (Exception ex)
                {
                    Program.toLogFile("DataSanitizationProgressForm.timer1_Tick ошибка: \r\n" + ex.Message + "\r\n" + ex.StackTrace.Replace("\r\n", "\r\n\t"));
                    label1.Text = ex.Message;
                }
            }
            else
                label1.Text = "Оставшееся время не оценено";

            try
            {
                var p = (int) (percent*5f);
                if (p >= progressBar1.Maximum)
                    progressBar1.Value = progressBar1.Maximum;
                else
                    progressBar1.Value = p;

                button1.Visible = true;
                if (!button1.Visible && ddso.prepercent >= 100f)
                {
                    button1.Visible = true;
                    progressBar2.Visible = false;
                }
                else
                {
                    var p2 = (int) (ddso.prepercent*2.5f);
                    if (p2 >= progressBar2.Maximum)
                        progressBar2.Value = progressBar2.Maximum;
                    else
                        progressBar2.Value = p2;
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Ошибка при выводе прогресса " + ex.Message;
            }

            if (ddso.success)
            {
                timer1.Enabled  = false;
                button1.Visible = false;
                Application.DoEvents();
                MessageBox.Show(MsgSuccess);
                Close();
            }
            else
            if (ddso.exited)
            {
                timer1.Enabled  = false;
                button1.Visible = false;
                Application.DoEvents();
                if (ddso.doTerminate && ddso.terminatedByUser)
                {
                    MessageBox.Show(MsgTerminate);
                }
                else
                    MessageBox.Show(MsgFailed + "\r\n" + ddso.errorMessage);
                Close();
            }
        }

        public string MsgSuccess   = "Удаление файла/папки выполнено";
        public string MsgTerminate = "Удаление файла/папки остановлено";
        public string MsgFailed    = "Удаление файла/папки закончилось неудачно. Возможно, часть файлов были удалены успешно";

        private void button1_Click_1(object sender, EventArgs e)
        {
            ddso.doTerminate = true;
            button1.Enabled = false;
            ddso.terminatedByUser = true;
        }

        private void DataSanitizationProgressForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ddso.doTerminate = true;
        }
    }
}
