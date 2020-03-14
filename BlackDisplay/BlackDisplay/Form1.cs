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

/*
 * http://forum.vingrad.ru/forum/topic-337116.html
 * http://blogs.msdn.com/b/nickkramer/archive/2006/03/18/554235.aspx
 * http://zayko.net/post/C-How-To-Disable-Windows-Screensaver-programmatically.aspx
 * http://www.codenet.ru/progr/delphi/quest073.php
 * http://stackoverflow.com/questions/2208595/c-sharp-how-to-get-the-events-when-the-screen-display-goes-to-power-off-or-on
 * */

/*
 * Время неактивности
 * http://www.dore.ru/perl/nntp.pl?f=1&gid=24&mid=26173
 * http://freebasic.justforum.net/t397-topic
 * http://msdn.microsoft.com/en-us/library/windows/desktop/ms646302%28v=vs.85%29.aspx
    LASTINPUTINFO lpi;
    lpi.cbSize = sizeof(lpi);
    GetLastInputInfo(&lpi);
 * */


namespace BlackDisplay
{
    public partial class Form1 : Form
    {
        protected static string logFileName
        {
            get
            {
                return Application.StartupPath + "/logs/" + GetLoginUserName() + "-" + GetLoginUserId() + ".log";
            }
        }

        protected static string wndLogFileName
        {
            get
            {
                return Application.StartupPath + "/logs/" + GetLoginUserName() + "-" + GetLoginUserId() + "_wnd.log";
            }
        }

        public static   bool locked  = false;
        public readonly bool toClose = false;
        public Form1()
        {
            InitializeComponent();
            // this.WindowState = FormWindowState.Minimized;
            // this.WindowState = FormWindowState.Maximized;
            // this.Hide();

            /*SetStyle(ControlStyles.Selectable, true);
            DbgLog.dbg.varToLog("Form1", "focused", new {focused = this.Focused, canSelect = this.CanSelect});*/

            #if forLinux
            удалениеФайлаToolStripMenuItem.Visible = false;
            режимToolStripMenuItem.Visible = false;
            this.ContextMenuStrip = this.testsMenu;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.TopMost = false;
            this.ShowInTaskbar = true;
            this.MinimizeBox = true;
            #endif

            SystemEvents.SessionSwitch          += SystemEvents_SessionSwitch;  // :логирование
            SystemEvents.TimeChanged            += TimeChanged;
            SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;

            if (!Directory.Exists("logs"))                                      // :логирование
            {
                Directory.CreateDirectory("logs");
            }

            truncateLogFile();

            lastTime = DateTime.Now.Ticks;

            createOrParseIni();
            ParseAndTrimLog();
            logTime(TimeRecord.started);

            setRunFromBoot();

            setLastTime();

            opts.writeToFile(Application.StartupPath + iniFileName);

            CheckReactionMenu();

            GC.Collect();
        }

#if forLinux
        static Int32 RegisterPowerSettingNotification(IntPtr hWnd, ref Guid PowerSettingGuid, Int32 Flags) {return 0;}
        static bool UnregisterPowerSettingNotification(Int32 powerNotify) {return false;}
#else
        [DllImport("user32.dll")]
        static extern Int32 RegisterPowerSettingNotification(IntPtr hWnd, ref Guid PowerSettingGuid, Int32 Flags);

        [DllImport("user32.dll")]
        static extern bool UnregisterPowerSettingNotification(Int32 powerNotify);
#endif
#if forLinux
#else
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == MonitorPowerMsgFilter.WM_POWERBROADCAST)
                new MonitorPowerMsgFilter().PreFilterMessage(ref m);
        }
#endif
        struct powerInfo
        {
            public Guid   PowerSetting;
            public Int32  DataLength;
            public byte[] Data;
            public Int32  DWord_Data;
        }

        class MonitorPowerMsgFilter: IMessageFilter
        {
            public static readonly Guid[] statuses = new Guid[] 
                                                        {
                                                            new Guid("02731015-4510-4526-99e6-e5a17ebd1aea"),   // GUID_MONITOR_POWER_ON
                                                         /* new Guid("2B84C20E-AD23-4ddf-93DB-05FFBD7EFCA5"),   // GUID_SESSION_DISPLAY_STATUS
                                                            new Guid("3C0F4548-C03F-4c4d-B9F2-237EDE686376"),   // GUID_SESSION_USER_PRESENCE
                                                            new Guid("98a7f580-01f7-48aa-9c0f-44352c29e5C0")    // GUID_SYSTEM_AWAYMODE*/
                                                        };

            public static readonly Int32 WM_POWERBROADCAST      = 0x0218;
            public static readonly Int32 PBT_POWERSETTINGCHANGE = 0x8013;
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_POWERBROADCAST)
                    return false;

//                 Program.toLogFileMessage("POW enter: " + m.LParam.ToInt32() + " / " + m.WParam.ToInt32());

                if (PBT_POWERSETTINGCHANGE != m.WParam.ToInt32())
                {
//                    Program.toLogFileMessage("POW exited without process: " + m.LParam.ToInt32() + " / " + m.WParam.ToInt32());
                    if (m.LParam.ToInt32() == 0)
                    {
                        var status = m.WParam.ToInt32();
                        if (status == 4)    // PBT_APMSUSPEND
                        {
                            checkForScreenSaver();
                            logTime(TimeRecord.PowOff);
                            return true;
                        }
                        else
                        if (status == 18 || status == 7)                    // PBT_APMRESUMEAUTOMATIC || PBT_APMRESUMESUSPEND   // 7 после 18, если это юзер
                        {
                            logTime(TimeRecord.PowOn);
                            checkForScreenSaver();
                            return true;
                        }
                    }
                    return false;
                }

                byte[] sa = Guid.Empty.ToByteArray();   // 16 элементов
                byte[] dt = null;
                powerInfo pi = new powerInfo();
                unsafe
                {
                    byte * s = (byte *) m.LParam.ToPointer();
                    for (int i = 0; i < 16; i++)
                        sa[i] = *(s + i);

                    int    len      = *((int  * ) (m.LParam.ToInt32() + 16));
                    byte * rdt      =   (byte * ) (m.LParam.ToInt32() + 16 + 4);
                    pi.DWord_Data   = *((int  * ) (m.LParam.ToInt32() + 16 + 4));
                    dt = new byte[len];
                    for (int i = 0; i < len; i++)
                    {
                        dt[i] = *(rdt + i);
                    }

                    pi.DataLength = len;
                }
                pi.PowerSetting = new Guid(sa);
                pi.Data         = dt;


                Int32 monStatus = pi.DWord_Data;
                // Program.toLogFileMessage("POW process: " + m.LParam.ToInt32() + " / " + m.WParam.ToInt32() + " -> " + monStatus + " len" + pi.DataLength + "(" + pi.PowerSetting.ToString().ToUpper() + ")");

                if (monStatus == 0/* || monStatus == 4*/)
                {
                    checkForScreenSaver();
                    logTime(TimeRecord.MonOff);
                }
                else
                if (monStatus == 1/* || monStatus == 5*/)
                {
                    checkForScreenSaver();
                    logTime(TimeRecord.MonOn);
                }

                return true;

            }

            public static List<Int32> notificationHandles = new List<int>(statuses.Length);
#if forLinux
            public static void register(Form window) {}
#else
            public static void register(Form window)
            {
                if (Program.WinVersion < 6)
                    return;

                var crash = false;
                var msg = "Не удалось зарегистрировать обработчик события изменения статуса питания ";

                foreach (var e in statuses)
                {
                    var s = e;
                    var notificationHandle = RegisterPowerSettingNotification(window.Handle,  ref s, 0 /* DEVICE_NOTIFY_WINDOW_HANDLE */);
                    if (notificationHandle == 0)
                    {
                        Program.toLogFile(msg + GetLastError() + " " + e.ToString());
                        crash = true;
                    }
                    notificationHandles.Add(notificationHandle);
                    /*else
                        Program.toLogFileMessage("Обработчик статуса монитора зарегистрирован: " + notificationHandle);*/
                }

                if (crash)
                    MessageBox.Show(msg, "Relax time black display");
            }
#endif
            public static void unregister()
            {
                if (Program.WinVersion < 6)
                    return;

                foreach (var notificationHandle in notificationHandles)
                {
                    UnregisterPowerSettingNotification(notificationHandle);
                }
                notificationHandles.Clear();
            }
        }

        bool isCreated = false;
        private void Form1_Shown(object sender, EventArgs e)
        {
            #if forLinux
            #else
                SetWatcherForUpdates();
            #endif

            if (!opts.contains(optsName[7]) || !opts[optsName[7], true] || opts[optsName[9], 0] != RtdbOptions.LicenseVersion)
            {
                var L = new License();
                if (L.dialogView() != System.Windows.Forms.DialogResult.Yes)
                {
                    Close();
                    return;
                }
                else
                {
                    opts.add(optsName[7], true, "лицензия принята");
                    opts.add(optsName[9], RtdbOptions.LicenseVersion, "");
                    saveOptionsToFile();
                }
            }

            УстановитьРежимИМодификаторыПоумолчанию();

            if (opts[optsName[16], true] && opts[optsName[17], 0] != RtdbOptions.lastNewFuncversion)
            {
                var dlgResult = MessageBox.Show
                        (
                            RtdbOptions.lastNewFuncversion + "\r\nОткрыть страницу описаний в браузере?\r\n\r\nНажмите кнопку 'Отмена', если хотите получить это диалоговое окно ещё раз",
                            "Relaxtime black display: информация после обновления",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Information
                        );
                if (dlgResult == System.Windows.Forms.DialogResult.Yes
                    )
                {
                    System.Diagnostics.Process.Start("http://relaxtime.8vs.ru/new" + RtdbOptions.lastNewFuncversion + ".html");
                }

                if (dlgResult != System.Windows.Forms.DialogResult.Cancel)
                {
                    opts.add(optsName[17], RtdbOptions.lastNewFuncversion, "");
                    saveOptionsToFile();
                }
            }

            if (!isCreated)
            {
                #if forLinux
                this.WindowState = FormWindowState.Minimized;

                isCreated = true;
                #else
                this.WindowState = FormWindowState.Maximized;
                this.Hide();
                isCreated = true;

                MonitorPowerMsgFilter.register(this);
                // Application.AddMessageFilter(new MonitorPowerMsgFilter()); // Почему-то не работает, пришлось переопределить WndProc
                #endif
            }

#if forLinux
            Console.WriteLine("форма создана");
#endif

            if (opts[Form1.optsName[34], true])
                PasswordGeneration.newPasswordGeneration().Hide();
        }

        private void УстановитьРежимИМодификаторыПоумолчанию()
        {
            if (opts[optsName[22], true])
                ОтключитьНапоминания();

            onlySiren  = opts[Form1.optsName[27], true];
            shortSiren = false;

            switch (Form1.opts[Form1.optsName[21], 0])
            {
                case 1:
                    ПерейтиВБыстрый();
                    break;
                case 2:
                    ПерейтиВЗамедленный();
                    break;
                case 3:
                    ПерейтиВМедленный();
                    break;
                case 4:
                    ПерейтиВМедленныйРучной();
                    break;
                case 5:
                    ПерейтиВСмотрюФильм();
                    break;
                case 6:
                    ПерейтиВНебеспокоить();
                    break;
                default:
                    throw new Exception("Не распознан режим по-умолчанию, полученный из настроек. Номер режима " + Form1.opts[Form1.optsName[21]].ToString());
            }

            #if forLinux
                ОтключитьНапоминания();
                ПерейтиВНебеспокоить();
            #endif
        }

        const int maxLogLine = 20000;
        private void truncateLogFile()
        {
            if (!File.Exists(logFileName))
                return;

            lock (logFileName)
            {
                var t = File.ReadAllLines(logFileName);
                if (t.Length > maxLogLine)
                {
                    var L = new String[maxLogLine >> 1];
                    var mll = maxLogLine >> 1;
                    for (int i = t.Length - 1, j = mll - 1; i >= t.Length - mll; i--, j--)
                    {
                        L[j] = t[i];
                    }
                    File.WriteAllLines(logFileName, L);
                }
            }
        }

        private void SetWatcherForUpdates()                                     // :обновление
        {
            if (Program.WinVersion < 0) // если это Linux
                return;

            Application.DoEvents();

            if (opts[optsName[5], ""].Length <= 0)
            {
                File.AppendAllText(Application.StartupPath + Program.errorLogFileName, DateTime.Now + "\r\nОбновление пропущено");
                return;
            }

            var ep = Application.ExecutablePath;
            var wd = Path.GetDirectoryName(ep);

            // Проверяем обновления
            var watcher = new FileSystemWatcher();
            watcher.Path = wd;
            watcher.IncludeSubdirectories = false;
            watcher.Filter = "update.flag";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastAccess;

            watcher.Created += UpdateFlagSetted;
            watcher.Renamed += UpdateFlagSetted;
            watcher.EnableRaisingEvents = true;

            if (File.Exists("update.flag"))
                UpdateFlagSetted(null, null);
            else
            try
            {
                // Запускаем после включения контроля за файлом update.flag, иначе включение контроля теоретически может опоздать
                var arguments = "relaxtime " + Program.version +  " \"" + wd + "\"";
                var updName   = Path.GetFullPath(  opts[optsName[5], ""]  );

                System.Diagnostics.Process.Start(updName, arguments);
            }
            catch (Exception ex)             // :логирование, :ошибки
            {
                File.AppendAllText(Application.StartupPath + Program.errorLogFileName, DateTime.Now + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                MessageBox.Show("Не удалось запустить установщик '" + opts[optsName[5], ""] + "' скачанных файлов обновления с сообщением об ошибке '" + ex.Message + "'", "Ошибка обновления");
            }
        }

        // Аргументы могут быть переданы null оба
        public void UpdateFlagSetted(object sender, FileSystemEventArgs e)      // :обновление
        {
            try
            {
                if (File.Exists(Application.StartupPath + "/update.flag"))
                {
                    // Чтобы не прервать длительный тест на внимание или другую операцию из-за обновления
                    if (AttentionTests.RedBlack.opened != null || PasswordGeneration.openedPwdForm != null || SimplePasswordBox.countOfHaoticBytes >= 512 || Application.OpenForms.Count > 1 || locked) // > 1, потому что главная форма есть всегда
                    {
                        if (AttentionTests.RedBlack.opened != null)
                            AttentionTests.RedBlack.opened.FormClosed += new FormClosedEventHandler(opened_FormClosed);

                        if (PasswordGeneration.openedPwdForm != null)
                            PasswordGeneration.openedPwdForm.FormClosed += new FormClosedEventHandler(openedPwdForm_FormClosed);

                        return;
                    }

                    try
                    {
                        updateProcessStart();

                        terminated = true;
                        this.Invoke(new CloseDelegate(Close));
                    }
                    catch (Exception ex)        // :логирование, :ошибки
                    {
                        File.AppendAllText(Application.StartupPath + Program.errorLogFileName, DateTime.Now + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        MessageBox.Show("Не удалось запустить установщик umove.exe скачанных файлов обновления с сообщением об ошибке '" + ex.Message + "'", "Ошибка обновления");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + Program.errorLogFileName, DateTime.Now + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                MessageBox.Show("Какие-то проблемы с обновлением. Попробуйте обратится к разработчику или скачать новую версию вручную", "Ошибка обновления");
            }
        }

        void openedPwdForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateFlagSetted(null, null);
        }

        delegate void CloseDelegate();

        void opened_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateFlagSetted(null, null);

            if (AttentionTests.RedBlack.opened != null)
            {
                AttentionTests.RedBlack.opened.FormClosed -= new FormClosedEventHandler(opened_FormClosed);
            }
        }

        public static void updateProcessStart()
        {
            var ep  = Application.ExecutablePath;
            var wd  = Path.GetDirectoryName(ep);
            var wdm = Path.GetDirectoryName(opts[optsName[5], ""]);

            var arguments = String.Format(" \"{0}\" \"{1}\" {2} \"{3}\"", Path.Combine(wd, "update.flag"), Program.MainMutexName, "1", ep);
            var umovePath = Path.Combine(wdm, "umove.exe");

            Program.toLogFileMessage("start update: " + arguments);
            logTime(TimeRecord.update);

            System.Diagnostics.Process.Start(umovePath, arguments);

            Program.toLogFileMessage("update started");
        }


        // Смотреть так же очищение при анинсталляции в Program.cs
        public static readonly string prgName = "Relax Time Black Display";
        #if forLinux
            private void setRunFromBoot() {}
        #else
        private void setRunFromBoot()       // :анинсталляция :инсталляция
        {
            bool notRun;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
            {
                notRun = rk.GetValue(prgName) == null;
            }

            if (opts[optsName[4], true] == notRun)
            using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (opts[optsName[4], true])
                {
                    rkApp.SetValue(prgName, "\"" + Application.ExecutablePath.ToString() + "\"");
                }
                else
                {
                    rkApp.DeleteValue(prgName, false);
                }
            }
        }
        #endif

        static string iniFileName = "/options.txt";             // :настройки
        public static RtdbOptions opts;
        public static string[] optsName = {"01maxOldTime", "02oneHTime", "03messageTime", "04computerLock", "05startuprun", "06updatorpath", "01-1maxRelaxTime", "07lic",
                                 /* 8 */   "08SaveWndText", "07-1LicenseVersion", "10MinWorkTime", "11shortTime", "12TimeToFast", "13shortActiveTime", "14DoBlackAlways",
                                 /* 15 */  "15NoDialogInFullScreen", "16ShowNewFunctionallity", "17LastNewFunctionDate",
                                 /* 18 */  "18NoNothing", "11shortTime1", "11shortTime2", "11DefaultRegime", "22noAlerts", "23TimeToDecelerate", "24shortDecelerateActiveTime",
                                 /* 25 */  "25NoSmallMouse", "26IntervalInputTimeCoefficient", "27Siren", "28SirenVolume", "29SirenAlways",
                                 /* 30 */  "30Siren1Duration", "31Siren2Duration", "32WaitForInaction", "33ShortSirenMaxCount", "34CollectPasswordGenerationData"
                                          };

        private void createOrParseIni() // :обновление :анинсталляция :инсталляция :безопасность
        {
            if (!File.Exists(Application.StartupPath + iniFileName))
            {
                var opt = new RtdbOptions();
                opt.writeToFile(Application.StartupPath + iniFileName);
            }

            refreshOptions();
        }

        public void refreshOptions(bool isOptionForm = false)
        {
            opts = new RtdbOptions(Application.StartupPath + iniFileName);

            // ComputerLockToolStripMenuItem.Checked = opts[optsName[3], true];
            setRunFromBoot();

            // Обновляем workInterval
            long maxRelaxTime, relaxByHour, relaxEventInterval, RelaxTime, minShortWorkInterval, newWorkInterval;
            getTimes(out maxRelaxTime, out newWorkInterval, out relaxByHour, out relaxEventInterval, out RelaxTime, out minShortWorkInterval);
            if (workInterval != newWorkInterval)
                trimmedCount = -1;   // Даём сигнал функции analize() в следующий раз обрезать записи по-другому и при этом заново считать лог-файл

            if (isOptionForm || OptionForm.self == null || OptionForm.self.IsDisposed)
                return;

             OptionForm.self.showOptions();
        }

        public void resetOptionsToDefault()
        {
            var a = opts.options[optsName[9]];
            var b = opts.options[optsName[7]];

            opts = new RtdbOptions();
            opts.saveExecute = false;
            opts.options[optsName[7]] = b;
            opts.options[optsName[9]] = a;
            opts.saveExecute = true;

            saveOptionsToFile();
            refreshOptions();
        }

        public static void log(string logType, string text, bool noToTimeRecordArray = false)
        {
            if (timeChangedFlag)
            {
                timeChangedFlag = false;
                LogTimeChanged();
            }

            var logString = logType + "\t: " + text + "\r\n";
            if (timeRecords != null && !noToTimeRecordArray)
            {
                var rec = new TimeRecord(logString);
                timeRecords.Insert(0, rec);
                currentStatus.modify(rec);

                if (rec.isOffAction)
                {
                    // nextDialogTime = 0;
                }

                ShiftAndTrim(timeRecords);
            }

            lock (logFileName)
            {
                File.AppendAllText(logFileName, logString);
            }
        }

        // subTime != 0 при записи времени автоматического затемнения экрана с поправкой на автоматику
        public static void logTime(string logType, long subMilliseconds = 0)
        {
            var n = DateTime.Now;

            if (subMilliseconds != 0)
                n = n.AddMilliseconds(-subMilliseconds);

            if (logType == TimeRecord.started || logType == TimeRecord.ended)
                log(logType, n.Ticks.ToString() + "(" + n.ToLocalTime() + ")");
            else
                log(logType, n.Ticks.ToString() + "(" + n.ToShortTimeString() + ")");

            setLastCTime();
        }

        // Используется только при очистке лога
        public static void logServiceTime(string logType, long time)
        {
            var n = new DateTime(time);

            if (logType == TimeRecord.started || logType == TimeRecord.ended)
                log(logType, n.Ticks.ToString() + "(" + n.ToLocalTime() + ")");
            else
                log(logType, n.Ticks.ToString() + "(" + n.ToShortTimeString() + ")");
        }

#if forLinux
        public static void LockWorkStation() {}
#else
        [DllImport("user32.dll")]
        public static extern void LockWorkStation();
#endif

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        protected static List<Form> showed = new List<Form>();
        public static void cursorShow(bool toShow = true)
        {
            Application.DoEvents();

            var hidden = true;
            foreach (Form form in Application.OpenForms)
            {
                if (toShow)
                {
                    if (showed.Contains(form))
                    {
                        Cursor.Show();
                        break;
                    }
                }
                else
                {
                    if (!showed.Contains(form))
                    {
                        showed.Add(form);
                        hidden = false;
                    }
                }
            }

            if (toShow)
            {
                showed.Clear();
            };

            if (!hidden)
                Cursor.Hide();

            Application.DoEvents();
        }

        public void blackVisible()
        {
            cursorShow(false);

            // this.ShowInTaskbar = true;
            locked              = true;
            visible             = false;
            this.WindowState    = FormWindowState.Maximized;
            this.Location       = new Point(0, 0);

            this.Show();

            this.Activate();
            this.BringToFront();

            tagLASTINPUTINFO p;
            int result;
            long dwTime;
            GetDelayTime(out p, out result, out dwTime, opts[optsName[25], true]);

            if (dwTime > 15000)
            {
                DbgLog.dbg.messageToLog("blackVisible", "dwTime > 15000");

                logTime(TimeRecord.block, (dwTime * opts[optsName[26], 0]) / 100);
                correctTimeRecordsFromAutoBlack(timeRecords);
            }
            else
            {
                DbgLog.dbg.messageToLog("blackVisible", "dwTime <= 15000");

                logTime(TimeRecord.block);
            }

            // nextDialogTime = 0;

            askIsVisible   = 0;
            if (ask != null)
            {
                ask.Close();
                ask.Dispose();
                ask = null;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            blackVisible();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshOptions();
            OptionForm.showForm(this);
        }

        public bool terminated = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!locked)
            {
                terminated = true;
            }

            if (terminated)
            {
                MonitorPowerMsgFilter.unregister();
                #if forLinux
                #else
                    registerHooks(false);
                    DbgLog.dbg.messageToLog("blackFormClose", "registerHooks(false)");
                #endif
            }

            e.Cancel = !terminated;
            blackFormClose();
        }

        
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


        public void blackFormClose(bool KeyPressed = false)
        {
            if (terminated)
            {
                logTime(TimeRecord.ended);
                toWndLog();

                DbgLog.dbg.varToLog("blackFormClose", "terminated", terminated);
                return;
            }

            if (!KeyPressed && !opts[optsName[3], true] && opts[optsName[14], true])  // Если неактивно и блокировать компьютер не нужно, то попытаться снова стать окном верхнего уровня
            {
                DbgLog.dbg.varToLog("blackFormClose", "focused", this.Focused);
                DbgLog.dbg.messageToLog("blackFormClose", "!KeyPressed && !opts[optsName[3], true] && opts[optsName[14], true]");

                #if forLinux
                    this.Activate();
                    DbgLog.dbg.varToLog("blackFormClose", "focused", new {focused = this.Focused, canSelect = this.CanSelect});
                #else
                var a = SetForegroundWindow(this.Handle);
                //var b = BringWindowToTop(this.Handle);
                var b = SetWindowPos(this.Handle, 0, 0, 0, 0, 0, 0x0002 | 0x0001);
                DbgLog.dbg.varToLog("blackFormClose", "focused", new {focused = this.Focused, canSelect = this.CanSelect, a = a, b = b});
                #endif

                return;
            }

            EndRelaxTime = 0;
            cursorShow();

            DbgLog.dbg.varToLog("blackFormClose", "locked", locked);

            if (locked)
            {/* // Это не нужно, так как любой отдых учитывается при задержке открытия следующего окна запроса
                long maxRelaxTime, minFullRelaxInterval, relaxByHour, relaxEventInterval, RelaxTime;
                getTimes(out maxRelaxTime, out minFullRelaxInterval, out relaxByHour, out relaxEventInterval, out RelaxTime);
                nextDialogTime = DateTime.Now.Ticks + relaxEventInterval - RelaxTime;       // Следующий диалог должен появиться не ранее, чем через заданный интервал
                */

                if (opts[optsName[3], true])
                {
                    LockWorkStation();
                    logTime(TimeRecord.tolock);
                        // this.Hide(); // это убираем, т.к. лучше скрывать окно после блокировки (иначе оно скрывается ранее, чем начинается блокиовка)
                        // this.ShowInTaskbar = false;
                }
                else
                {
                    logTime(TimeRecord.bunlock);
                    #if forLinux
                    #else
                    this.Hide();
                    #endif
                    this.label1.Visible = false;
                    cursorShow();
                    // this.WindowState = FormWindowState.Minimized;
                }

                locked = false;
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            terminated = true;
            Close();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.Shift || e.Control || e.Alt)
                if (locked)
                    blackFormClose(true);
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (locked)
            {
                blackFormClose();
            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            if (locked)
            {
                blackFormClose();
            }
        }

        static bool timeChangedFlag = false;
        static long timer1_Interval = 0;
        void TimeChanged(object sender, EventArgs e)
        {
            timer1_Interval = timer1.Interval;
            timeChangedFlag = true;
        }

        static private void LogTimeChanged()
        {
            timeChangedFlag = false;

            log(TimeRecord.timeChanged, DateTime.Now.Ticks - timer1_Interval * ms + "/" + lastTime);
            setLastTime();
            setLastCTime();
        }

        void DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (locked && opts[optsName[3], true])
                blackFormClose();
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    logTime(TimeRecord.slock);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    logTime(TimeRecord.sunlock);
                    #if forLinux
                    #else
                    this.Hide();                                         // скрываем окно, если оно есть, так как если пользователь вошёл, то логично, что он хочет работать
                    #endif
                    cursorShow();
                    // this.WindowState = FormWindowState.Minimized;
                    break;
                case SessionSwitchReason.SessionLogoff:
                    logTime(TimeRecord.logoff);
                    break;
                case SessionSwitchReason.SessionLogon:
                    logTime(TimeRecord.logon);
                    break;
                case SessionSwitchReason.ConsoleConnect:
                    break;
                case SessionSwitchReason.ConsoleDisconnect:
                    break;
                case SessionSwitchReason.RemoteConnect:
                    break;
                case SessionSwitchReason.RemoteDisconnect:
                    break;
                case SessionSwitchReason.SessionRemoteControl:
                    break;
                default:
                    return;
            }
        }

        public static string GetLoginUserName()
        {
            #if forLinux
            return "noName";
            #else
            var name = WindowsIdentity.GetCurrent().Name;
            int k = name.LastIndexOfAny(new Char[] {'\\', '/'});
            if (k == -1)
                return name;

            return name.Substring(k + 1);
            #endif
        }

        public static string GetLoginUserId()
        {
            #if forLinux
            return "noID";
            #else
            return WindowsIdentity.GetCurrent().User.Value;
            #endif
        }

        static long lastTime  = 0;
        static long lastCTime = 0;
        Random rnd = new Random();
        bool visible = false;
        static long continueInterval = 10000L * 1000 * 60 * 7;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                tick();
            }
            catch (Exception ex) // :логирование :ошибки
            {
                File.AppendAllText(Application.StartupPath + Program.errorLogFileName, DateTime.Now + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        static TimeRecord.status currentStatus = new TimeRecord.status();
        private void tick()
        {
            var now = DateTime.Now;
            var dtt = now.Ticks;

            if (timeChangedFlag)
            {
                timeChangedFlag = false;
                LogTimeChanged();
            }

            setLastTime();

            if ((dtt - lastCTime) > continueInterval)    // 7 минут
            {
                logTime(TimeRecord.@continue);
                setLastCTime();
            }

            checkForScreenSaver();

            analize();
            visible ^= true;
            if (locked/* && visible*/ && /*EndRelaxTime < dtt*/ RelaxNeeded <= 0)  // visible - выводит через раз метку времени на экран
            {
                DbgLog.dbg.varToLog("tick", "RelaxNeeded / label1.Visible = true", RelaxNeeded);

                label1.Text = " " + now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2") + " (время отдыха истекло)";
                label1.Location = new Point(rnd.Next(this.Width - label1.Width), rnd.Next(this.Height - label1.Height));
                label1.Visible  = true;
            }
            else
                label1.Visible = false;

            shortTimeCheck();

            DbgLog.dbg.dataToLog("tick", "currentStatus", currentStatus);
            if (currentStatus.work == TimeRecord.status.st.No)
            {
                if (notifyIcon1.Icon == yellow)
                    notifyIcon1.Icon = yellow;
            }
            else
            if (currentStatus.work == TimeRecord.status.st.Yes)
            {
                if (notifyIcon1.Icon != gray)
                    notifyIcon1.Icon = gray;
            }
            else
            {
                if (notifyIcon1.Icon == red)
                    notifyIcon1.Icon = red;
            }

            DbgLog.dbg.varToLog("tick", "locked", locked);
            if (!locked)
            {

            }
            else
            {
                #if forLinux
                if (!this.Focused)
                    blackFormClose();
                #else
                var fw = GetForegroundWindow();
                var tw = GetTopWindow(IntPtr.Zero);
                DbgLog.dbg.varToLog("tick.topwnd", "fw/tw", new {foreground = fw, top = tw, focused = this.Focused, hwnd = this.Handle, isChildOfWindow = isChildOfWindow(tw, this.Handle), block = opts[optsName[3], true]});
                
                if (!this.Focused || this.Handle != fw || (!isChildOfWindow(tw, this.Handle) && opts[optsName[3], true]))
                    blackFormClose();
                #endif
            }
        }

        #if forLinux
        #else
        public bool isChildOfWindow(IntPtr hwnd, IntPtr parent)
        {
            var current = hwnd;
            while (current != IntPtr.Zero)
            {
                if (current == parent)
                    return true;

                current = GetParent(current);
            }

            return false;
        }
        #endif

        long lastSlowReaction       = 0;
        long lastDwTime             = 0;
        long lastDecelerateReaction = 0;
        private void shortTimeCheck()
        {
            string optName = currentShort == 1 ? optsName[19] : optsName[20];

            long shortTimeToBlack = (long) opts[optName, 0] * minute / 10;
            long toFastBackTime   = (long) opts[optsName[12], 0] * minute;
            long maxActiveTime    = (long) opts[optsName[13], 0] * second;
            long toDecelBackTime  = (long) opts[optsName[23], 0] * minute;
            long maxActiveTimeDec = (long) opts[optsName[24], 0] * second;

            if (shortTimeToBlack <= 0)
            {
                DbgLog.dbg.messageToLog("shortTimeCheck", "shortTimeToBlack <= 0");
                return;
            }

            tagLASTINPUTINFO p;
            int result;
            long dwTime;
            GetDelayTime(out p, out result, out dwTime, opts[optsName[25], true]);
            /*
            try
            {
                if (lastDwTime > 15000 && dwTime < lastDwTime)     // логировать, если больше 15 секунд
                    File.AppendAllText(Application.StartupPath + "/lastinput.log", (lastDwTime/1000).ToString() + "\r\n");
            }
            catch
            {
            }
            */
            lastDwTime = dwTime;

            dwTime *= 10000;

            var dtt = DateTime.Now.Ticks;

            DbgLog.dbg.varToLog("shortTimeCheck", "vars", new {dwTime = dwTime, GetLastInputInfo_Result = result, currentShort = currentShort, slowReaction = slowReaction, lastSlowReaction = lastSlowReaction, lastDecelerateReaction = lastDecelerateReaction, dtt = dtt, maxActiveTime = maxActiveTime, maxActiveTimeDec = maxActiveTimeDec, shortTimeToBlack = shortTimeToBlack, locked = locked, workStatus = currentStatus.work});

            if (currentStatus.work == TimeRecord.status.st.No)
            {
                lastSlowReaction = dtt;
                lastDecelerateReaction = dtt;
                return;
            }

#if DEBUG
            if (Form1.locked && currentStatus.work != TimeRecord.status.st.No)
            {
                Program.toLogFile("Form1.locked && currentStatus.work != TimeRecord.status.st.No");
                MessageBox.Show("В программе Relaxtime Black Display возникла ошибка: Form1.locked && currentStatus.work != TimeRecord.status.st.No; сообщите разработчику по e-mail, указанному в меню 'О программе'");
                return;
            }
#endif

            // Проверка на переход в быстрый режим
            if (slowReaction || currentShort == 2)
            {
                if (dwTime > maxActiveTime)
                {
                    lastSlowReaction = dtt;
                }
                else
                {
                    if (dtt - lastSlowReaction > toFastBackTime && (!notWindows && !ManualSlow) && currentStatus.work == TimeRecord.status.st.Yes)  // Снимаем медленный режим, но только если нет режима "Не беспокоить" или режима "Медленный ручной"
                        toFastReaction(1);
                }
            }

            // Проверка на переход в замедленный режим
            if (slowReaction)
            {
                if (dwTime > maxActiveTimeDec)
                {
                    lastDecelerateReaction = dtt;
                }
                else
                {
                    if (dtt - lastDecelerateReaction > toDecelBackTime && (!notWindows && !ManualSlow) && currentStatus.work == TimeRecord.status.st.Yes)  // Сменяем медленный режим на замедленный, но только если нет режима "Не беспокоить" или режима "Медленный ручной"
                        toFastReaction(2);
                }
            }

            if (dwTime > shortTimeToBlack && !slowReaction && !locked)
            {
                if (currentStatus.work == TimeRecord.status.st.No)
                    return;

                // Нет формы и не отключена выдача форм
                // Не полноэкранное, при TopMost вывод стандартный, либо программа настроена выдавать окна всегда
                // Не активна экранная заставка (lastCheckedScreen == 0)
                if (shortAskForm == null && ask == null &&
                    !noReaction && lastCheckedScreen == 0 &&
                    (isFullScreen <= 0 || isFullScreen == 2 || !opts[optsName[15], true])
                    )
                {
                    shortAskForm = new shortAsk(this);
                    showShortAsk(null);
                    /*try
                    {
                        var owner = new shortAsk.shortAskOwner();
                        showShortAsk(owner);
                    }
                    catch (Exception e)
                    {
                        Program.toLogFile("shortAsk exception: " + e.Message + "\r\n\t" + e.StackTrace.Replace("\n", "\n\t"));

                        shortAskForm = new shortAsk(this);
                        showShortAsk(null);
                    }*/

                    shortAskForm.BringToFront();
                    shortAskForm.Activate();

                    DbgLog.dbg.setLogRegime(DbgLog.DATA, 0);
                    DbgLog.dbg.varToLog("shortTimeCheck", "vars", new {dwTime = dwTime, GetLastInputInfo_Result = result, currentShort = currentShort, slowReaction = slowReaction, lastSlowReaction = lastSlowReaction, lastDecelerateReaction = lastDecelerateReaction, dtt = dtt, maxActiveTime = maxActiveTime, maxActiveTimeDec = maxActiveTimeDec, shortTimeToBlack = shortTimeToBlack, locked = locked, workStatus = currentStatus.work});


                    DbgLog.dbg.messageToLog("shortTimeCheck", "shortAskForm.Show();");

                    System.Threading.Thread.Sleep(0);
                    DbgLog.dbg.setLogRegime(DbgLog.DATA, -1);
                }
            }
        }

        private void showShortAsk(shortAsk.shortAskOwner owner)
        {
            if (owner == null || owner.Handle == IntPtr.Zero)
                shortAskForm.Show();
            else
                shortAskForm.Show(owner);
        }

#if forLinux
        public static void GetDelayTime(out tagLASTINPUTINFO p, out int result, out long dwTime, bool ignoreSmallMouse)
        {
            p = new tagLASTINPUTINFO();
            unsafe
            {
                p.cbSize = (uint) sizeof(tagLASTINPUTINFO);
            }
            p.dwTime = 0;
            dwTime = 0;
            result = 0;
        }
#else
        public static void GetDelayTime(out tagLASTINPUTINFO p, out int result, out long dwTime, bool ignoreSmallMouse)
        {
            p = new tagLASTINPUTINFO();

            if (ignoreSmallMouse && registerHooksResult != 1)
                registerHooks();

            if (ignoreSmallMouse && registerHooksResult >= 0)
            {
                //registerHooks(true, true);
                var dtt = DateTime.Now.Ticks;

                long keyTime   = dtt - lastKeyDownTime;
                long mouseTime = dtt - lastMouseMoveTime;

                DbgLog.dbg.varToLog("shortTimeCheck", "GetDelayTime", new {keyTime = keyTime, mouseTime = mouseTime});

                if (keyTime > mouseTime)
                    dwTime = mouseTime / 10000;
                else
                    dwTime = keyTime   / 10000;

                result = 1;
            }
            else
            {
                unsafe
                {
                    p.cbSize = (uint)sizeof(tagLASTINPUTINFO);
                }

                result = GetLastInputInfo(ref p);
                dwTime = GetTickCount() - p.dwTime;
            }
        }
#endif

        static readonly Icon yellow = new Icon(typeof(Form1), "blackSleep.ico");
        static readonly Icon gray   = new Icon(typeof(Form1), "black.ico");
        static readonly Icon red    = new Icon(typeof(Form1), "blackUnknown.ico");


        [DllImport("user32.dll")]
        public static extern int GetLastInputInfo(ref tagLASTINPUTINFO p);
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        public struct tagLASTINPUTINFO
        {
            public uint  cbSize;
            public uint  dwTime;
        }

        protected bool slowReaction = false;
        public    bool noReaction   = false;
        public    bool notWindows   = false;
        protected bool ManualSlow   = false;
        public    bool noRelaxTime  = false;
        protected bool onlySiren    = false;
        public    bool shortSiren   = false;
        public shortAsk shortAskForm = null;
        public    int  currentShort  = 0;
        public void toSlowReaction(bool toManualSlow = false)
        {
            slowReaction = true;
            notWindows   = false;
            ManualSlow   = toManualSlow;

            CheckReactionMenu();
            lastSlowReaction = DateTime.Now.Ticks;
            lastDecelerateReaction = DateTime.Now.Ticks;
        }

        private void CheckReactionMenu()
        {
            if (!slowReaction)
                ManualSlow = false;

            неБеспокоитьВообщеToolStripMenuItem.Checked = notWindows;

            быстрыйToolStripMenuItem    .Checked = БыстрыйРежимВключён();
            замедленныйToolStripMenuItem.Checked = ЗамедленныйРежимВключён();
            медленныйToolStripMenuItem  .Checked = МедленныйРежимВключён();
            неВыдаватьToolStripMenuItem .Checked = РежимСмотрюФильмВключён();

            медленныйРучнойToolStripMenuItem     .Checked = МедленныйРучнойРежимВключён();
            отключитьНапоминанияToolStripMenuItem.Checked = noRelaxTime;
            толькоСиренаToolStripMenuItem        .Checked = onlySiren;
            игровойToolStripMenuItem             .Checked = ИгровойРежимВключён();
            сокращённаяСиренаToolStripMenuItem   .Checked = shortSiren;

            медленныйToolStripMenuItem      .Enabled = !notWindows;
            медленныйРучнойToolStripMenuItem.Enabled = !notWindows;
            неВыдаватьToolStripMenuItem     .Enabled = !notWindows;

            DbgLog.dbg.varToLog("ReactionType", "vars", new {currentShort = currentShort, slowReaction = slowReaction, noReaction = noReaction, ManualSlow = ManualSlow, noRelaxTime = noRelaxTime, notWindows = notWindows, onlySiren = onlySiren});
        }

        public bool РежимСмотрюФильмВключён()
        {
            return noReaction && slowReaction;
        }
        private bool ЗамедленныйРежимВключён()
        {
            return !slowReaction && (currentShort == 2);
        }
        private bool БыстрыйРежимВключён()
        {
            return !slowReaction && (currentShort == 1);
        }
        private bool МедленныйРежимВключён()
        {
            return slowReaction && !ManualSlow && !noReaction;
        }
        private bool МедленныйРучнойРежимВключён()
        {
            return slowReaction && ManualSlow;
        }
        private bool ИгровойРежимВключён()
        {
            return shortSiren && onlySiren && slowReaction && ManualSlow;
        }

        public void toFastReaction(int shortRegime)
        {
            currentShort = shortRegime;
            noReaction   = false;
            slowReaction = false;
            notWindows   = false;
            ManualSlow   = false;

            CheckReactionMenu();
            var dtt = DateTime.Now;
            lastSlowReaction = dtt.Ticks;
            lastDecelerateReaction = dtt.Ticks;
        }

        #if forLinux
        public static int GetLastError()
        {
            return -1;
        }
        #else
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int BringWindowToTop(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("Psapi.dll")]
        public static extern Int32 GetModuleFileNameExA(Int32 hProcess, IntPtr module, byte[] lpszFileName, Int32 cchFileNameMax);

        [DllImport("Kernel32.dll")]
        public static extern Int32 QueryFullProcessImageNameA(Int32 hProcess, Int32 dwFlags, byte[] lpszFileName, ref Int32 cchFileNameMax);

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowTextA(IntPtr hWnd, byte[] lpszFileName, Int32 cchFileNameMax);

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hwnd, out Int32 ProcessId);

        [DllImport("Kernel32.dll")]
        public static extern Int32 OpenProcess(Int32 access, bool inheritedhandle, Int32 ProcessId);

        [DllImport("Kernel32.dll")]
        public static extern Int32 CloseHandle(Int32 handle);

        [DllImport("kernel32")]
		public extern static int GetLastError();
        #endif

        #if forLinux
        const long lastCheckedScreen = 0;
        private static void checkForScreenSaver()
        {
                return;
        }
        #else
        static long lastCheckedScreen = 0;
        private static void checkForScreenSaver()
        {

            var fw = GetForegroundWindow();
            if (fw.ToInt32() == 0)
                fw = GetTopWindow(IntPtr.Zero);

            isFullScreen = checkForFullScreen(fw);

            if (fw.ToInt32() == 0)
            {
                lastCheckedScreen = 0;
                logTime(TimeRecord.Screen);
                toWndLog();
                return;
            }

            Int32 pId = 0;
            Int32 ph = 0;
            byte[] b;
            int l;
            GetWindowThreadProcessId(fw, out pId);
            try
            {
                int flag = 0x1000;              /*PROCESS_QUERY_LIMITED_INFORMATION*/
                if (Program.WinVersion < 6)
                    flag = 0x0400;              /*PROCESS_QUERY_INFORMATION*/

                ph = OpenProcess(flag /*0x1000*/ /*PROCESS_QUERY_LIMITED_INFORMATION*/ /*0x0400*/ /*PROCESS_QUERY_INFORMATION*/ /*| 0x0010*/ /*PROCESS_VM_READ*/, false, pId);

                if (ph == 0)
                {
                    Program.toLogFile("OpenProcess return 0 для окна " + fw + " и идентификатора процесса " + pId + " LastError " + GetLastError());
                    toWndLog();

                    if (lastCheckedScreen != 0)
                    {
                        logTime(TimeRecord.ScreenOff);
                        lastCheckedScreen = 0;
                    }

                    return;
                }

                int L = 1024 * 10;
                b = new byte[L];
                // l = GetModuleFileNameExA(ph, IntPtr.Zero, b, L);
                l = QueryFullProcessImageNameA(ph, 0 /* не PROCESS_NAME_NATIVE */, b, ref L);
                if (l == 0 || L == 0)
                {
                    Program.toLogFile("GetModuleFileNameExA return 0 для окна " + fw + " и процесса " + ph + " LastError " + GetLastError());

                    if (lastCheckedScreen != 0)
                    {
                        logTime(TimeRecord.ScreenOff);
                        lastCheckedScreen = 0;
                    }

                    toWndLog();
                    return;
                }

                l = L;
            }
            finally
            {
                CloseHandle(ph);
            }

            var B = new byte[l];
            for (int i = 0; i < l; i++)
                B[i] = b[i];

            var processName = Encoding.GetEncoding("windows-1251").GetString(B);
            if (processName.EndsWith(".scr")) // или SystemParametersInfo с флагом SPI_GETSCREENSAVEACTIVE
            {
                if (DateTime.Now.Ticks - lastCheckedScreen > continueInterval)
                {
                    logTime(TimeRecord.Screen);
                    lastCheckedScreen = DateTime.Now.Ticks;
                }
            }
            else
                if (lastCheckedScreen != 0)
                {
                    logTime(TimeRecord.ScreenOff);
                    lastCheckedScreen = 0;
                }

            string wndText = "";
            if (opts[optsName[8], true])
            {
                byte[] wndTextB_ = new byte[1024];
                int k = GetWindowTextA(fw, wndTextB_, wndTextB_.Length);
                byte[] wndText_  = new byte[k];
                if (k > 0)
                {
                    for (int i = 0; i < k; i++)
                        wndText_[i] = wndTextB_[i];

                    wndText = Encoding.GetEncoding("windows-1251").GetString(wndText_);
                }
            }

            toWndLog(processName, isFullScreen, lastCheckedScreen != 0, wndText);
        }
        #endif

        #if forLinux
        public static int isFullScreen = -1;        // 1 - некоторое приложение полностью заняло экран
        private static int checkForFullScreen(IntPtr hWnd)
        {
            var fw = hWnd; //GetForegroundWindow();
            if (fw.ToInt32() == 0)
                return 0;

            var tm = false;

            var workingArea = Screen.PrimaryScreen.Bounds;

            Rectangle wRect = Program.mainForm.ClientRectangle;

            if (wRect.Width == workingArea.Width && wRect.Height == workingArea.Height)
            {
                return 1;
            }

            if (tm)
                return 2;           // Возвращаем, что приложение является TopMost, но не полноэкранное
            return -1;
        }
        #else
        [DllImport("user32.dll")]
        private extern static int GetClientRect(IntPtr hWnd, out Rectangle clientRect);

        [DllImport("user32.dll")]
        private extern static uint GetWindowLong(IntPtr hWnd, int flags);

        public static readonly int  GWL_STYLE       = -16;
        public static readonly int  GWL_EXSTYLE     = -20;
        public static readonly uint WS_POPUP        = 0x80000000;
        public static readonly uint WS_EX_TOPMOST   = 0x00000008;
        public static readonly uint WS_CAPTION      = 0x00C00000;

        public static int isFullScreen = -1;        // 1 - некоторое приложение полностью заняло экран
        private static int checkForFullScreen(IntPtr hWnd)
        {
            var fw = hWnd; //GetForegroundWindow();
            if (fw.ToInt32() == 0)
                return 0;

            var tm = false;
            // Program.toLogFileMessage(String.Format("{0}/{1} == {2}/{3}; {4}", wRect.Width, wRect.Height, workingArea.Width, workingArea.Height, GetWindowLong(fw, GWL_STYLE)));
            uint exFlags = GetWindowLong(fw, GWL_EXSTYLE);
            if (  (exFlags & WS_EX_TOPMOST) > 0  )              // если наверху окно, которое всегда наверху, так же считать это уважительным поводом не мешать работе пользователя
                tm = true;

            var workingArea = Screen.PrimaryScreen.Bounds;
            Rectangle wRect;
            GetClientRect(fw, out wRect);

            if (wRect.Width == workingArea.Width && wRect.Height == workingArea.Height)
            {
                uint flags   = GetWindowLong(fw, GWL_STYLE);
                DbgLog.dbg.varToLog("checkForFullScreen", "fw", fw);
                DbgLog.dbg.varToLog("checkForFullScreen", "flags", flags);

                if (/*(flags & WS_POPUP) > 0*/ (flags & WS_CAPTION) == 0)
                    // WS_EX_TOPMOST - может быть и не использован; WS_POPUP - стандартный стиль для полноэкранных Direct3D
                    // WS_CAPTION - стиль с заголовком
                    return 1;
                else
                {
                    if (tm)
                       return 1;    // Возвращаем 1, т.к. topMost и окно на весь экран автоматом означает, что приложение полноэкранное
                    return 0;
                }
            }

            if (tm)
                return 2;           // Возвращаем, что приложение является TopMost, но не полноэкранное
            return -1;
        }
        #endif

        static string lastProcessName = "";
        static string lastWndText     = "";
        static long   lastWriteToWndLog = 0;
        public static void toWndLog()
        {
            var dt = DateTime.Now;

            if (lastProcessName == null && lastWndText == null && lastWriteToWndLog + continueInterval > dt.Ticks)
                return;

            lastProcessName     = null;
            lastWndText         = null;
            lastWriteToWndLog   = dt.Ticks;

            File.AppendAllText(  wndLogFileName, String.Format("{0}::none\r\n", dt.Ticks.ToString())  );
        }

        public static void toWndLog(string processName, int isFullScreen, bool isScr, string windowText)
        {
            var dt = DateTime.Now;
            if (processName == lastProcessName && windowText == lastWndText && lastWriteToWndLog + continueInterval > dt.Ticks)
                return;

            lastProcessName     = processName;
            lastWndText         = windowText;
            lastWriteToWndLog   = dt.Ticks;

            string isFullScreenText, isScrText;
            isScrText = isScr ? "SS" : "App";       // ScreenSaver/Application
            if (isFullScreen == 1)
                isFullScreenText = "FS";    // полноэкранное приложение со стилем WS_POPUP или topMost (стандартный для Direct3D)
            else
            if (isFullScreen == -1)
                isFullScreenText = "W";     // оконное приложение
            else
            if (isFullScreen == 2)
                isFullScreenText = "TM";    // всегда наверху
            else
                isFullScreenText = "US" + isFullScreen;    // неизвестный скрин

            File.AppendAllText(wndLogFileName, String.Format("{0}::{1}/{2}::{3}::[[[{4}]]]::{5}\r\n", dt.Ticks.ToString(), isFullScreenText, isScrText, processName, windowText, dt.Hour + ":" + dt.Minute));
        }

        static private void setLastTime()
        {
            lastTime = DateTime.Now.Ticks;
        }

        private static void setLastCTime()
        {
            lastCTime = DateTime.Now.Ticks;
        }

        private void блокироватьКомпьютерToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            /*opts.add(optsName[3], ComputerLockToolStripMenuItem.Checked, opts.options[optsName[3]].comment);

            if (OptionForm.self != null && !OptionForm.self.IsDisposed)
                OptionForm.self.checkBox1.Checked  = opts[Form1.optsName[ 3], true];*/
        }

        public static void saveOptionsToFile()
        {
            opts.writeToFile(Application.StartupPath + iniFileName);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.DoEvents();

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

            new License().Show();
        }

        private void быстрыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВБыстрый();
        }

        private void ПерейтиВБыстрый()
        {
            toFastReaction(1);
        }

        private void замедленныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВЗамедленный();
        }

        public void ПерейтиВЗамедленный()
        {
            toFastReaction(2);
        }

        private void медленныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВМедленный();
        }

        private void ПерейтиВМедленный()
        {
            toSlowReaction();
        }

        private void экспресстест2ЧислаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AttentionTests.Attention2Number().Show();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(Application.StartupPath + "/help/index.html"))
                    System.Diagnostics.Process.Start(Path.GetFullPath(Application.StartupPath + "/help/index.html"));
                else
                    System.Diagnostics.Process.Start("http://relaxtime.8vs.ru/");
            }
            catch (Exception ex)
            {
                Program.toLogFile(ex.Message + "\r\n" + ex.StackTrace.Replace("\r\n", "\r\n\t"));
                MessageBox.Show("Произошла ошибка при вызове справки: " + ex.Message, "Не удалось вызвать справку",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void неВыдаватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВСмотрюФильм();
        }

        public void ПерейтиВСмотрюФильм()
        {
            noReaction = !noReaction;

            if (noReaction)
                toSlowReaction();
            else
                if (currentShort == 1 || currentShort == 2)
                    toFastReaction(currentShort);
                else
                    toFastReaction(1);
        }

        private void неБеспокоитьВообщеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВНебеспокоить();
        }

        private void ПерейтиВНебеспокоить()
        {
            notWindows = !notWindows;
            noReaction = notWindows;
            slowReaction = false;
            CheckReactionMenu();
        }

        private void медленныйРучнойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ПерейтиВМедленныйРучной();
        }

        private void ПерейтиВМедленныйРучной()
        {
            toSlowReaction(true);
        }

        private void отключитьНапоминанияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noRelaxTime = !noRelaxTime;
            CheckReactionMenu();
        }

        public void ОтключитьНапоминания()
        {
            noRelaxTime = true;
            CheckReactionMenu();
        }

        private void толькоСиренаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onlySiren)
                ВыключитьРежимТолькоСирена();
            else
                ВключитьРежимТолькоСирена();
        }

        public void ВключитьРежимТолькоСирена()
        {
            onlySiren = true;
            CheckReactionMenu();
        }

        public void ВыключитьРежимТолькоСирена()
        {
            onlySiren = false;
            CheckReactionMenu();
        }


        private void игровойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ИгровойРежимВключён())
            {
                УстановитьРежимИМодификаторыПоумолчанию();
            }
            else
            {
                ПерейтиВИгровойРежим();
            }
        }

        private void ПерейтиВИгровойРежим()
        {
            shortSiren = true;
            ПерейтиВМедленныйРучной();
            ВключитьРежимТолькоСирена();
        }

        private void сокращённаяСиренаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            shortSiren = !shortSiren;
            CheckReactionMenu();
        }

        private void планироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Tasks().Show();
        }

        private void красночёрнаяТаблицаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AttentionTests.RedBlack().Show();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                #if forLinux
                #else
                contextMenuStrip1.Show(e.Location);
                #endif
            }
        }

        private void закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            blackFormClose(true);
        }

        private void генерироватьПарольToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

            var pwdForm = PasswordGeneration.newPasswordGeneration();
            pwdForm.Show();

            if (pwdForm.WindowState == FormWindowState.Minimized)
                pwdForm.WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            
        }

        private void шифроватьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cryptFile();
        }

        private void вычислениеХэшейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HashCalcerForm.ShowForm();
        }

        private void хранилищеПаролейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new PasswordManager().Show();
        }

        private void расшифроватьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            decryptFile();
        }

        private void проконтролироватьНеизменностьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileHashes.@new().Show();
        }

        private void сравнитьХешиДляКонтроляToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
