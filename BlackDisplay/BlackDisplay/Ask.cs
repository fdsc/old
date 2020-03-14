using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlackDisplay
{
    public partial class Ask : Form
    {/*
        [DllImport("CoreDll.DLL", EntryPoint="PlaySound", SetLastError=true)]
        private extern static int WCE_PlaySound(string szSound, IntPtr hMod, int flags);

        [DllImport("CoreDll.DLL", EntryPoint="PlaySound", SetLastError=true)]
        private extern static int WCE_PlaySoundBytes (byte[] szSound, IntPtr hMod, int flags);  // нет такой dll
        */
        readonly int    siren;
        readonly double relaxState;
        public Ask(Form1 f, double relaxState): this()
        {
            this.WindowState = FormWindowState.Minimized;
            this.siren       = relaxState > 0 ? 1 : (int) (   (1 - relaxState) * Form1.opts[Form1.optsName[31], 0]   );
            this.relaxState  = relaxState;
            this.form        = f;
        }

        public Ask()
        {
            siren = 0;
            InitializeComponent();
        }

        public readonly int relaxSimpleStatus;
        public readonly Form1 form = null;
        public readonly long  startTime = DateTime.Now.Ticks;
        public Ask(Form1 f, int status = 0): this()
        {
            form = f;
            timer1.Enabled = true;
            if (status > 0)
                timer2.Enabled  = true;

            /*if (status > 1)
            {
                button9.Enabled = false;
            }
            else
            if (Form1.opts[Form1.optsName[18], true])   // Настройка "Без кнопки "Ни за что!""
                button9.Enabled = false;*/

            relaxSimpleStatus = status;
            btnColor = button1.BackColor;
        }

        public long nextTime = 0;
        public long reactionInterval = 1 * 1000 * 10000;
        private void button1_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks - 1;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 2;
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 3;
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 4;
            Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 5;
            Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 7;
            Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            nextTime = DateTime.Now.Ticks + Form1.minute * 9;
            Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks - startTime < reactionInterval)
                return;

            if (MessageBox.Show("Вы уверены, что хотите отложить отдых на неопределённое время?", "Отложить отдых?", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != System.Windows.Forms.DialogResult.Yes)
                return;

            nextTime = -1;
            Close();
        }

        private void Ask_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClosedForm();
        }

        private void ClosedForm()
        {
            //Program.toLogFileMessage("Диалог предложения отдыха закрыт: " + nextTime + " в " + DateTime.Now.Ticks);
            options.DbgLog.dbg.messageToLog("", "Диалог предложения отдыха закрыт: " + nextTime + " в " + DateTime.Now.Ticks);
            if (nextTime != 0)
                form.askDialogResult();

            if (siren < 1 && Form1.opts[Form1.optsName[29], 0] == 2)
                isSirenSolutionExist = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1 .Enabled = false;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;

            if (relaxSimpleStatus < 2 && !Form1.opts[Form1.optsName[18], true])
                button9.Enabled = true;
        }

        Color btnColor;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (button1.BackColor != btnColor)
            {
                button1.BackColor = btnColor;
            }
            else
                if (relaxSimpleStatus > 1)
                    button1.BackColor = Color.Red;
                else
                    button1.BackColor = Color.Gold;
        }

        internal void ShowOrSiren()
        {
            if (siren < 1)
            {
                if (Form1.opts[Form1.optsName[29], 0] > 0)
                    PlaySiren(Form1.opts[Form1.optsName[29], 0]);

                Show();
                BringToFront();
                Focus();
            }
            else
            {
                // Show();
                PlaySiren();
                nextTime = DateTime.Now.Ticks + Form1.minute;
                ClosedForm();
            }
        }

        private bool isSirenSolutionExist = false;
        public void PlaySiren(int sirenAlways = -1)
        {
            isSirenSolutionExist = false;

            if (siren == 1 || (siren < 1 && sirenAlways >= 0))
            {
                int k = (int)(  (1 - relaxState) * Form1.opts[Form1.optsName[30], 0]  );
                if (k < 1)
                    k = 1;

                if (sirenAlways == 1)
                    k = 2;

                SoundSiren(1, k, this);
            }
            else
            {
                if (sirenAlways == 1)
                    SoundSiren(2, 2, this);
                else
                    SoundSiren(2, siren, this);
                /*
                        var sp = new System.Media.SoundPlayer("Resources/sirenimp.wav");
                        for (int i = 0; i < siren; i++)
                        {
                            if (!Form1.locked)
                                sp.PlaySync();
                        }*/
            }
        }

        public static void SoundSiren(int sirenNumber, int siren, Ask form = null)
        {
            var o = new object();
            System.Threading.ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    System.Windows.Media.MediaPlayer player = new System.Windows.Media.MediaPlayer();
                    player.Volume = Form1.opts[Form1.optsName[28], 0] / 100.0;

                    bool notWindows = form != null && !(!form.form.notWindows && !form.form.noReaction) ? true : false;
                    // var sp = new System.Media.SoundPlayer(s);
                    int i = 0;

                    if (sirenNumber == 1)
                        player.Open(new Uri("Resources/siren.wav", UriKind.Relative));
                    if (sirenNumber == 2)
                        player.Open(new Uri("Resources/sirenimp.wav", UriKind.Relative));

                    if (form != null && form.form.shortSiren && siren > Form1.opts[Form1.optsName[33], 0])
                        siren = Form1.opts[Form1.optsName[33], 0];

                    while (!player.NaturalDuration.HasTimeSpan) Thread.Sleep(20);

                    var ts = player.NaturalDuration.TimeSpan;
                    //ts.Subtract(new TimeSpan(0, 0, 0, 0, 1));
                    for (; i < siren; i++)
                    {
                        if (Program.mainForm.РежимСмотрюФильмВключён() || Program.mainForm.noRelaxTime)
                            return;

                        //player.Open(new Uri("Resources/sirenimp.wav", UriKind.Relative));
                        player.Position = new TimeSpan(0);
                        player.Play();

                        lock (o) Monitor.Wait(o, ts.Milliseconds + 1000 * ts.Seconds - 20);

                        while (player.Position < ts)
                            Thread.Sleep(20);

                        player.Stop();

                        if (Form1.locked || (form != null && form.isSirenSolutionExist))
                        {
                            for (int j = 0; j < 20; j++)
                            {
                                lock (o) Monitor.Wait(o, 500);
                                if (! (Form1.locked || (form != null && form.isSirenSolutionExist)) )
                                    break;
                            }

                            if (
                                Form1.locked
                                || (form != null && form.isSirenSolutionExist)
                                || (notWindows != (form != null && !(!form.form.notWindows && !form.form.noReaction) ? true : false))
                                )
                                return;
                        }
                    }
                }
            );
        }
    }
}
