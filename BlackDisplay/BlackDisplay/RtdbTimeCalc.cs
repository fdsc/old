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

namespace BlackDisplay
{
    public partial class Form1
    {
        // ------------------------ Подсчёт времени -----------------------------
        public class TimeRecord
        {
            public readonly string type;

            /// <summary>
            /// Время по часам пользователя
            /// </summary>
            public readonly long   timeMark1;
            /// <summary>
            /// Используется как второе время при переводе пользователем часов. Это старое время
            /// </summary>
            public readonly long   timeMark2;
            /// <summary>
            /// Истинное время, приведённое с учётом пользовательских переводов времени
            /// </summary>
            public          long   timeMark = 0;

            public readonly bool   isOffAction;
            public readonly bool   isOnAction;
            public          long   shifted;

            public TimeRecord()
            {
                type      = TimeRecord.@continue;
                timeMark1 = DateTime.Now.Ticks;
                timeMark  = DateTime.Now.Ticks;
                timeMark2 = 0;

                isOffAction = false;
                isOnAction  = false;
            }

            /// <summary>
            /// Используется при коррекции лога в случае, если произошёл вылет программы
            /// </summary>
            /// <param name="endedTime">Добавляемое время относительно timeMark последнего срабатывания</param>
            /// <param name="shifted">shifted - время, на которое сдвинуто это срабатывание (см. поле в записи лога)</param>
            public TimeRecord(long endedTime, long shifted)
            {
                type        = ended;
                timeMark    = endedTime;
                timeMark1   = endedTime - shifted;
                timeMark2   = 0;

                isOffAction = true;
                isOnAction  = false;
            }

            /// <summary>
            /// Используется для создания записи лога из строки лог-файла
            /// </summary>
            /// <param name="timeRecord">Строка лог-файла</param>
            public TimeRecord(string timeRecord)
            {
                var spl     = timeRecord.Split(new string[] {":", "/", "("}, StringSplitOptions.RemoveEmptyEntries);

                type        = spl[0].Trim();
                timeMark1    = Int64.Parse(spl[1].Trim());
                if (type == timeChanged)
                {
                    timeMark2 = Int64.Parse(spl[2].Trim());
                }
                else
                    timeMark2 = 0;

                isOffAction = offActions.Contains(type);
                isOnAction  = onActions .Contains(type);
            }

            public static readonly string timeChanged = "time";
            public static readonly string started     = "started";
            public static readonly string update      = "update";
            public static readonly string block       = "blacklock";
            public static readonly string tolock      = "tolock";
            public static readonly string bunlock     = "blackunlock";
            public static readonly string ended       = "ended";
            public static readonly string slock       = "lock";
            public static readonly string sunlock     = "unlock";
            public static readonly string logoff      = "logoff";
            public static readonly string logon       = "logon";
            public static readonly string @continue   = "continue";
            public static readonly string Screen      = "event.Screen.On";
            public static readonly string ScreenOff   = "event.Screen.Off";
            public static readonly string MonOff      = "event.Monitor.PowerOff";
            public static readonly string MonOn       = "event.Monitor.PowerOn";
            public static readonly string PowOn       = "event.PowerOn";
            public static readonly string PowOff      = "event.PowerOff";

            public static readonly List<string> offActions  = new List<string>();
            public static readonly List<string> onActions   = new List<string>();
            static TimeRecord()
            {
                offActions.Add(block);
                offActions.Add(tolock);
                offActions.Add(ended);
                offActions.Add(slock);
                offActions.Add(logoff);
                offActions.Add(Screen);
                offActions.Add(MonOff);
                offActions.Add(PowOff);

                onActions.Add(started);
                onActions.Add(bunlock);
                onActions.Add(sunlock);
                onActions.Add(logon);
                onActions.Add(PowOn);
                onActions.Add(MonOn);
                onActions.Add(ScreenOff);
            }

            public class status
            {
                public enum st {No = -1, Unknown = 0, Yes = 1};

                public static st toSt(bool v)
                {
                    return v ? st.Yes : st.No;
                }

                public st powerOn;
                public st monitorOn;
                public st screenOff;
                public st blockOff;
                public st work;
                public st end;
                public st workOldState;

                public status()
                {
                    reset();
                }

                private void reset()
                {
                    powerOn   = st.Yes;
                    monitorOn = st.Yes;
                    screenOff = st.Yes;
                    blockOff  = st.Yes;
                    end       = st.Unknown;
                    work      = st.Unknown;
                }

                public bool modify(TimeRecord rec)
                {
                    workOldState = work;

                    // Любая следующая команда, идущая за end является командой обнуления статуса, если это не команда start
                    // Это может быть только в том случае, если start пропущена или end вставлено из-за слишком большого промежутка (т.е. в любом случае, если есть ошибка)
                    if (end == st.Yes)
                    {
                        end = st.Unknown;
                        if (!rec.isOffAction && rec.type != started)
                            reset();
                    }


                    if (rec.type == MonOff)
                        monitorOn = toSt(false);
                    else
                    if (rec.type == MonOn)
                    {
                        monitorOn = toSt(true);
                        powerOn   = toSt(true);
                    }
                    else
                    if (rec.type == PowOff)
                        powerOn = toSt(false);
                    else
                    if (rec.type == PowOn)
                        powerOn = toSt(true);
                    else
                    if (rec.type == Screen)
                        screenOff = toSt(false);
                    else
                    if (rec.type == ScreenOff)
                        screenOff = toSt(true);
                    else
                    if (rec.type == bunlock)
                        blockOff = st.Yes;
                    else
                    if (rec.type == block)
                        blockOff = st.No;

                    if (rec.isOnAction)
                    {
                        work = toSt(true);
                    }
                    else
                    if (rec.isOffAction)
                    {
                        work = toSt(false);
                        if (rec.type == ended)
                            end = st.Yes;
                    }

                    if (rec.type == ended || rec.type == started)
                    {
                        blockOff  = st.Yes;  // На случай, если есть проблемы (аварийное завершение)
                    }

                    if (powerOn == st.No || monitorOn == st.No || screenOff == st.No || blockOff == st.No)
                        work = toSt(false);

                    return work != workOldState;
                }
            }
        }

        public class TimeRecordsList: List<TimeRecord>
        {
            public TimeRecordsList(int initCount): base(initCount)
            {
            }

            public TestMethodResult testInvariant()
            {
                if (Count < 2)
                    return new TestMethodResult(TestMethodResult.generalTestResult.skipped);

                long lastTime = this[0].timeMark;
                for (int i = 1; i < Count; i++)
                {
                    if (lastCTime < this[i].timeMark)
                        return new TestMethodResult(TestMethodResult.generalTestResult.errorFound);
                }

                return new TestMethodResult(TestMethodResult.generalTestResult.success);
            }

            public new TimeRecord this[int i]
            {
                get
                {
                    if (i == -1)
                        return new TimeRecord();

                    return base[i];
                }
                set
                {
                    base[i] = value;
                }
            }
        }


        static Ask ask = null; static long RelaxNeeded = 0;
        static TimeRecordsList timeRecords = null;
        static int trimmedCount = -1;
        static long nextDialogTime = 0;
        static long EndRelaxTime = 0;
        static long askIsVisible = 0;
        static int  askCancelCount = 0;

        private void analize()
        {
            lock (this)
            lock (timeRecords)
                analize1();
        }

        private void analize1()
        {
            ParseAndTrimLog();

            if (workInterval <= 0)
                throw new Exception("RtdbTimeCalc.cs: fatal algorithm error workInterval <= 0");

            RelaxNeeded = 0;
            if (timeRecords.Count <= 1)
                return;

            ShiftAndTrim(timeRecords);

            long relax, work, toRelax; int relaxSimpleStatus;
            double relaxState;
            toRelax = checkToRelax(timeRecords, out relax, out work, out RelaxNeeded, out relaxSimpleStatus, out relaxState);

            notifyIcon1.Text = "Экран учёта отдыха (" + RelaxNeeded / minute + ": " + relax / minute + "/" + ((double) work / (double) hour).ToString("f1")  + ")";

            if (locked || currentStatus.work == TimeRecord.status.st.No)
            {
                return;
            }

            var NOW = DateTime.Now.Ticks;
            EndRelaxTime = NOW + toRelax;

            if (askIsVisible != 0 && askIsVisible + 20 * second <  NOW) // окно видно, но игнорируется более 20 секунд
            {
                if (ask != null)
                {
                    ask.Close();
                    ask.Dispose();
                    ask = null;
                    askIsVisible = 0;

                    blackVisible();
                    return;
                }
                else
                    Program.ToLogFile("Экран запроса на отдых должен быть активен, однако это не так [чёрный экран при превышении ожидания ответа пользователя]");
            }

#if DEBUG
            if (Form1.locked && currentStatus.work != TimeRecord.status.st.No)
            {
                Program.toLogFile("Form1.locked && currentStatus.work != TimeRecord.status.st.No");
                MessageBox.Show("В программе Relaxtime Black Display возникла ошибка: Form1.locked && currentStatus.work != TimeRecord.status.st.No; сообщите разработчику по e-mail, указанному в меню 'О программе'");
                return;
            }
#endif

            if (currentStatus.work == TimeRecord.status.st.No)
                return;

            // Если пора отдыхать
            if (
                (askIsVisible == 0 || askIsVisible + continueInterval < NOW)
                && toRelax > 0 && nextDialogTime < NOW && (isFullScreen <= 0 || !opts[optsName[15], true])
                )       // если окно видно продолжительное время или оно скрыто
            {
                if (ask != null)
                {
                    ask.Close();
                    ask.Dispose();
                    ask = null;
                }

                if (noReaction || noRelaxTime)
                    return;

                #if forLinux
                #else
                if (lastCheckedScreen != 0) // если активна экранная заставка, хотя это должно проверяться выше режимом работы
                    return;
                #endif

                if (shortAskForm != null)
                {
                    shortAskForm.Focus();
                    return;
                }

                tagLASTINPUTINFO p; int result; long dwTime;
                GetDelayTime(out p, out result, out dwTime, opts[optsName[25], true]);

                long k32 = opts[optsName[32], 0];
                DbgLog.dbg.messageToLog("Ask", "dwTime " + dwTime + ", onlySiren = " + onlySiren + ", k32 = " + k32);

                if (!onlySiren && k32 > 1 && dwTime < 1000 * 2.5 * k32)
                {
                    if (askCancelCount < 2 * k32)
                    {
                        askCancelCount++;
                        return;
                    }
                    else
                    {
                        Ask.SoundSiren(1, 2);
                        askCancelCount = 0;
                        return;
                    }
                }

                if (!onlySiren && (k32 == 1 && dwTime < 1000 * 7.5))
                {
                    if (askCancelCount < 2)
                    {
                        askCancelCount++;
                        return;
                    }
                }

                if (  onlySiren || (k32 == 1 && askCancelCount > 1)  )
                    ask = new Ask(this, relaxState);
                else
                    ask = new Ask(this, relaxSimpleStatus);

                askCancelCount = 0;

                askIsVisible = DateTime.Now.Ticks;

                ask.relaxTimeLabel.Text = "К отдыху " + toRelax / minute + " минут (" + relax / minute + "/" + ((double) work / (double) hour).ToString("f1") + ")";
                ask.ShowOrSiren();
            }
        }

        private void ParseAndTrimLog()
        {
            if (timeRecords == null || trimmedCount < 0)
            {
                timeRecords = readAndParse();
                for (int i = timeRecords.Count - 1; i >= 0; i--)
                    currentStatus.modify(timeRecords[i]);

                if (timeRecords.Count == 0)
                {
                    File.WriteAllText(logFileName, "");
                }
                else
                if (timeRecords.Count == 1 && timeRecords[0].type == TimeRecord.started)
                {
                    File.WriteAllText(logFileName, "");
                    logServiceTime(timeRecords[0].type, timeRecords[0].timeMark);
                }
            }
        }

        public void askDialogResult()
        {
            nextDialogTime = ask.nextTime;
            askIsVisible = 0;
            ask.Dispose();
            ask = null;

            var dtt = DateTime.Now.Ticks;
            if (nextDialogTime < 0)
            {
                long maxRelaxTime, relaxByHour, relaxEventInterval, RelaxTime, minShortWorkInterval;
                getTimes(out maxRelaxTime, out workInterval, out relaxByHour, out relaxEventInterval, out RelaxTime, out minShortWorkInterval);

                nextDialogTime = dtt + relaxEventInterval - RelaxTime;
                var ndt = dtt + minShortWorkInterval;
                if (ndt > nextDialogTime)
                    nextDialogTime = ndt;
            }

            if (nextDialogTime < dtt && RelaxNeeded > 0)
            {
                EndRelaxTime = dtt + RelaxNeeded;
                blackVisible();
            }
        }

        private long checkToRelax(TimeRecordsList records, out long relax, out long work, out long toRelax, out int relaxSimpleStatus, out double relaxState)
        {
            long maxRelaxTime, relaxByHour, relaxEventInterval, RelaxTime, minShortWorkInterval;
            getTimes(out maxRelaxTime, out workInterval, out relaxByHour, out relaxEventInterval, out RelaxTime, out minShortWorkInterval);

            relax = calcRelaxTime      (timeRecords, workInterval, maxRelaxTime);
            work  = calcWorkTime       (timeRecords, workInterval, maxRelaxTime);

            relaxState = calcRelaxState(timeRecords, relaxByHour, maxRelaxTime);
            long lastr = calcLastRelaxTime  (timeRecords, workInterval, maxRelaxTime);

            var oneRelax  = (double) relaxEventInterval / (double) hour / 2.0;  // что даёт одна релаксация (считая полный цикл за два часа)
            var WallRelax = 1.0 - oneRelax;

            toRelax = (long) (  2 * relaxByHour * (1.0 - relaxState)  ); // отдых от relaxState=0 занимает два часа общего времени (включая рабочее)

            relaxSimpleStatus = 0;

            long now = DateTime.Now.Ticks;
            DbgLog.dbg.dataToLog("checkToRelax", "relax data", new {toRelax = toRelax, RelaxTime = RelaxTime, relaxEventInterval = relaxEventInterval,
                                                                    work = work, relax = relax, oneRelax = oneRelax, WallRelax = WallRelax,
                                                                    relaxState = relaxState, lastr = lastr, minShortWorkInterval = minShortWorkInterval,
                                                                    now = now, maxRelaxTime = maxRelaxTime, minFullRelaxInterval = workInterval,
                                                                    relaxByHour = relaxByHour});

            if (toRelax < 0 || RelaxTime < 0)
            {
                Program.ToLogFile("toRelax < 0 || RelaxTime < 0");
                throw new Exception("Программа произвела неверный расчёт времени отдыха. Сообщите об этом разработчику!");
            }

            if (toRelax > relaxByHour * 2)
                relaxSimpleStatus = 1;
            if (toRelax > relaxByHour * 3)
                relaxSimpleStatus = 2;

            var trlt = Math.Max(RelaxTime, toRelax) / 10000 + 1.0;
            var s    = ((double) (RelaxTime / 10000) / trlt);

            if (
                work < (relaxEventInterval - RelaxTime) // || lastr + (relaxEventInterval - RelaxTime) * s > now
                )
                return 0;

            if (lastr + minShortWorkInterval * s > now)
                return 0;

            // rc/wc = (r + x)/w => w*rc/wc - r = x
            if (toRelax >= RelaxTime)
                return toRelax;

            return 0;
        }

        private long calcWorkTime(List<TimeRecord> records, long minFullRelaxInterval, long maxRelaxTime)
        {
            int  errorOccur = 0;
            long result     = 0;
            long lastMark   = records[records.Count - 1].timeMark;

            var work        = new TimeRecord.status();

            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (work.work == TimeRecord.status.st.Yes)
                {
                    var sres = records[i].timeMark - lastMark;
                    if (sres < 0)
                    {
                        errorOccur++;
                    }
                    result += sres;
                }

                work.modify(records[i]);

                lastMark = records[i].timeMark;
            }

            if (work.work == TimeRecord.status.st.Yes)
                result += DateTime.Now.Ticks - records[0].timeMark;

            if (errorOccur > 0)
                Program.ToLogFile("Произошла ошибка: в функции calcWorkTime найдена отрицательная длительность " + errorOccur + " раз");

            return result;
        }

        public static double exp(double x)
        {
            return Math.Exp(x);
        }

        public static double log(double x)
        {
            return Math.Log(x);
        }

        private long getLogicRelaxTime(long sres, long maxRelaxTime, long minFullRelaxInterval)
        {
            return sres;
            /*
            if (sres <= maxRelaxTime)
                return sres;

            if (sres >= maxRelaxTime * 2)
                return maxRelaxTime + (maxRelaxTime >> 1);

            return maxRelaxTime + (sres - maxRelaxTime) / 2;
             * */
        }

        static long minRelaxInterval = 20 * second;
        private long calcRelaxTime(List<TimeRecord> records, long minFullRelaxInterval, long maxRelaxTime)
        {
            long result     = 0;
            long sres       = 0;
            long lastMark   = records[records.Count - 1].timeMark;
            var  relax      = new TimeRecord.status();

            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (relax.work == TimeRecord.status.st.No)
                    sres += records[i].timeMark - lastMark;

                relax.modify(records[i]);
                if (relax.work == TimeRecord.status.st.Yes)
                {
                    if (sres > minRelaxInterval)
                        result += getLogicRelaxTime(sres, maxRelaxTime, minFullRelaxInterval); //MIN(sres, maxRelaxTime);
                    sres  = 0;
                }

                lastMark = records[i].timeMark;
            }

            if (relax.work == TimeRecord.status.st.No)
            {
                sres += DateTime.Now.Ticks - lastMark;
            }

            if (sres > minRelaxInterval)
                result += getLogicRelaxTime(sres, maxRelaxTime, minFullRelaxInterval);

            return result;
        }
    

        double calcRelaxState(TimeRecordsList records, long relaxByHour, long maxRelaxTime)
        {
            double result   = 1.0;
            long   lastTime = 0;
            var    relax    = new TimeRecord.status();

            for (int i = records.Count - 1; i >= -1; i--)
            {

                if (relax.modify(records[i]) || (i < 0 && lastTime != 0))
                {
                    if (relax.workOldState == TimeRecord.status.st.No)
                    {
                        if (lastTime == 0)
                            throw new Exception("В расчёте времени отдыха зафиксирована невозможная ситуация типа TimeRecord.status.st.No==TimeRecord.status.st.Unknown");

                        long relaxTime = records[i].timeMark - lastTime;
                        if (relaxTime < minRelaxInterval)
                            relaxTime = 0;

                        result += (double) relaxTime /(double) relaxByHour/2.0; // в 0 за два часа
                        if (result > 1.0)
                            result = 1.0;   // нельзя отдохнуть более, чем на 100%
                    }
                    else
                    if (relax.workOldState == TimeRecord.status.st.Yes)
                    {
                        if (lastTime == 0)
                            throw new Exception("В расчёте времени отдыха зафиксирована невозможная ситуация типа TimeRecord.status.st.Yes==TimeRecord.status.st.Unknown");

                        result -= (double) (records[i].timeMark - lastTime)/(double) (hour - relaxByHour)/2.0;  // на 1 за два часа (работы и отдыха в заданном режиме)
                    }

                    if (relax.work != TimeRecord.status.st.Unknown)
                        lastTime = records[i].timeMark;
                    else
                        lastTime = 0;
                }

            }

            if (result > 1.0)
                result = 1.0;   // нельзя отдохнуть более, чем на 100%

            return result;
        }


        int getTimeRecordWithAction(TimeRecordsList records, bool onAction, int startI)
        {
            for (int i = startI; i < records.Count; i++)
            {
                if (onAction)
                {
                    if (records[i].isOnAction)
                       return i;
                }
                else
                if (records[i].isOffAction)
                    return i;
            }

            return -1;
        }

        private long calcLastRelaxTime(TimeRecordsList records, long minFullRelaxInterval, long maxRelaxTime)
        {
            long lastTime = DateTime.Now.Ticks;
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].isOffAction)
                {
                    /*int k = getTimeRecordWithAction(records, true, i + 1);
                    if (k < 0)
                        return lastTime;
                    if (lastTime - records[k - 1].timeMark > minRelaxInterval) */
                    if (lastTime - records[i].timeMark > minRelaxInterval)
                        return lastTime;
                }
                else
                if (records[i].isOnAction)
                    lastTime = records[i].timeMark;
            }

            return records[records.Count - 1].timeMark;
        }

        static long MAX(long a1, long a2)
        {
            if (a1 > a2)
                return a1;

            return a2;
        }

        static long MIN(long a1, long a2)
        {
            if (a1 < a2)
                return a1;

            return a2;
        }

        private TimeRecordsList readAndParse()
        {
            string[] t = new String[0];
            if (File.Exists(logFileName))
                t = File.ReadAllLines(logFileName);

            var result = new TimeRecordsList(Math.Min(t.Length, 1024));

            // Берём из настроек workInterval, т.к. этот метод запускается ещё до анализа и workInterval ещё не инициализирован
            long maxRelaxTime, relaxByHour, relaxEventInterval, RelaxTime, minShortWorkInterval;
            getTimes(out maxRelaxTime, out workInterval, out relaxByHour, out relaxEventInterval, out RelaxTime, out minShortWorkInterval);

            ParseTimeRecords(t, result);
            ShiftAndTrim(result);

            return result;
        }

        private static void ShiftAndTrim(TimeRecordsList result)
        {
            ShiftTimeRecords(result);
            TrimTimeRecords(result, workInterval);
        }

        private static void ShiftTimeRecords(TimeRecordsList result)
        {
            long timeShift = 0;
            for (int i = 0; i < result.Count; i++)
            {
                var rec = result[i];

                rec.timeMark = rec.timeMark1 + timeShift;
                rec.shifted  = timeShift;

                if (rec.type == TimeRecord.timeChanged)
                {
                    timeShift += rec.timeMark1 - rec.timeMark2;
                }
            }
        }

        private static void TrimTimeRecords(TimeRecordsList result, long workInterval)
        {
            if (trimmedCount == result.Count)
                return;

            correctTimeRecordsFromAutoBlack(result);
            correctTimeRecordsFromBreakdown(result);

            trimmedCount = 0;
            if (result.Count <= 1)
                return;

            long lastMarkTime = 0;
            //if (result[0].isOffAction)
            lastMarkTime = DateTime.Now.Ticks;

            for (int i = 0; i < result.Count; i++)
            {
                long timeMark = result[i].timeMark;
                if (lastMarkTime - timeMark > workInterval)
                {
                    result.RemoveRange(i, result.Count - i);
                    break;
                }

                lastMarkTime = timeMark;
            }

            trimmedCount = result.Count;
        }

        /// <summary>
        /// Эта функция служит для переупорядочивания записей в том случае, когда чёрный экран всплывает по бездействию,
        /// т.к. запись "continue" может быть уже после того, как программа убедится в бездействии пользователя и зачтёт часть времени бездействия в минус от текущего времени.
        /// </summary>
        private static void correctTimeRecordsFromAutoBlack(TimeRecordsList result)
        {
            lock (result)
            for (int i = 1; i < result.Count; i++)  // 0 - самый недавний элемент
            {
                if (result[i - 1].timeMark - result[i].timeMark < 0)
                {
                    if (!result[i - 1].isOffAction)
                        throw new Exception("Фатальная ошибка в функции correctTimeRecordsFromAutoBlack. Сообщите разработчику prg@8vs.ru. " + result[i - 1].type + "/" + result[i].type + "//" + result[i - 1].timeMark + "/" + result[i].timeMark + "-" + i);

                    if (result[i].isOnAction)
                    {
                        result.RemoveAt(i);
                        i--;

                        currentStatus.modify(result[i]);
                    }
                    else
                    {
                        var o = result[i];
                        result.RemoveAt(i);
                        result.Insert(i - 1, o);
                    }
                }
            }
        }

        /// <summary>
        /// В случае, если продолжительное время не было сообщений от программы, вставить сообщение, что программа была завершена
        /// </summary>
        private static void correctTimeRecordsFromBreakdown(TimeRecordsList result)
        {
            var lastTime = DateTime.Now.Ticks;
            for (int i = 0; i < result.Count; i++)  // 0 - самый недавний элемент
            {
                if (!result[i].isOffAction && lastTime - result[i].timeMark > continueInterval + 6 * second)
                {
                    var time = result[i].timeMark + (continueInterval >> 1);
                    if (time > lastTime)
                        throw new Exception("Фатальная ошибка в функции correctTimeRecordsFromBreakdown. Сообщите разработчику prg@8vs.ru");

                    var tr   = new TimeRecord(time, result[i].shifted);
                    result.Insert(i, tr);
                }

                lastTime = result[i].timeMark;
            }
        }

        public const long hour   = minute * 60L;
        public const long minute = second * 60L;
        public const long second = ms * 1000L;
        public const long ms     = 10000L;
        public static long workInterval = 0;
        private void ParseTimeRecords(string[] t, TimeRecordsList result)
        {
            for (int i = t.Length - 1; i >= 0; i--)
            {
                var rec = new TimeRecord(t[i]);
                result.Add(rec);
            }
        }

        public static TestMethodResult test()
        {
            return new TestMethodResult(TestMethodResult.generalTestResult.skipped);
        }
    }
}
