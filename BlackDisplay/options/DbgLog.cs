using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace options
{
    // Требует [MTAThread] над main
    public class DbgLog: IDisposable
    {
        public readonly string FileName;
        public readonly bool   base64Format;
        private         volatile bool   disabled = true;
        protected       volatile bool   terminated = false;

        protected       AutoResetEvent   newTask     = new AutoResetEvent(false);
        protected       ManualResetEvent newDiskTask = new ManualResetEvent(false);

        public bool Disabled
        {
            get
            {
                return disabled;
            }
            set
            {
                if (disabled == value)
                    return;


                if (value)
                {
                    this.toLog("LOG_INFO", "SYSTEM.LOG", "log to disabled\r\n" + getStackTrace());
                    disabled = true;
                }
                else
                {
                    disabled = false;
                    this.toLog("LOG_INFO", "SYSTEM.LOG", "log to enabled\r\n" + getStackTrace());
                }
            }
        }

        protected int defRegime = 0;
        protected readonly SortedList<string, int> logRegime = new SortedList<string, int>(4);
        public void setLogRegime(string nameOfErrorType, int regime)
        {
            lock (logRegime)
            {
                if (nameOfErrorType == null)
                {
                    defRegime = regime;
                }
                else
                {
                    if (logRegime.ContainsKey(nameOfErrorType))
                        logRegime[nameOfErrorType] = regime;
                    else
                        logRegime.Add(nameOfErrorType, regime);
                }
            }
        }

        public void clearAllLogRegimes()
        {
            lock (logRegime)
                logRegime.Clear();
        }

        public bool getLogRegime(string nameOfErrorType, out int regime)
        {
            lock (logRegime)
            {
                if (nameOfErrorType != null && logRegime.ContainsKey(nameOfErrorType))
                {
                    regime = logRegime[nameOfErrorType];
                    return true;
                }

                regime = defRegime;
                return false;
            }
        }

        public static string getStackTrace()
        {
            return getStackTrace(  getStackFrames()  );
        }

        public static string getStackTrace(StackFrame[] frames)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < frames.Length; i++)
            {
                sb.AppendLine
                    (
                        GetStackFrameTrace(frames[i], i)
                    );
            }

            return sb.ToString();
        }

        public static string getString(object obj)
        {
            if (obj == null)
                return "null";
            else
                return getString(  obj.ToString()  );
        }

        public static string getString(string str)
        {
            if (str == null)
                return "null";
            else
                return "\"" + str + "\"";
        }

        public static string GetStackFrameTrace(StackFrame frame, int i)
        {
            var lb = new StringBuilder();
            var m  = frame.GetMethod();

            var t = "\t\t";

            string methodName = "null", methodType = "null", assemblyName = "null", fileName, fileLine;
            if (m != null)
            {
                methodName  = getString(m.Name);

                if (m.ReflectedType != null)
                    methodType  = getString(m.ReflectedType.FullName);

                if (m.Module != null && m.Module.Assembly != null)
                    assemblyName = getString(m.Module.Assembly.FullName);

                // var parameters = m.GetParameters();
            }

            fileName = getString(  frame.GetFileName()  );
            fileLine = frame.GetFileLineNumber().ToString();

            lb.Append(fileName);
            lb.Append(":");
            lb.Append(fileLine);
            lb.Append("\\" + assemblyName);
            lb.Append("\\" + methodType);
            lb.Append("\t" + methodName);

            return t + lb.ToString();
        }

        public static StackFrame[] getStackFrames()
        {
            var stackTrace = new StackTrace(true);
            return stackTrace.GetFrames();
        }

        public static readonly SortedList<string, DbgLog> logs = new SortedList<string,DbgLog>();

        protected delegate void threadDelegate();
        protected delegate void workDelegate(object obj);

        public readonly Thread[] mainThreads;
        public readonly Thread   diskThread;
        protected DbgLog(string logFileName, bool base64 = false)
        {
            if (isDisposed)
                return;

            FileName     = Path.GetFullPath(logFileName);
            base64Format = base64;

            main = new Mutex(false, "vs8.ru options.DbgLog " + Path.GetFileName(logFileName));

            setLogRegime(null, 0);

            diskThread = new Thread(new ThreadStart( new threadDelegate(disklogging)));
            diskThread.IsBackground = true;
            diskThread.Start();

            mainThreads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < mainThreads.Length; i++)
            {
                mainThreads[i] = new Thread(new ThreadStart( new threadDelegate(logging)));
                diskThread.IsBackground = true;
                mainThreads[i].Start();
            }
        }

        public static DbgLog getLog(string logFileName, bool base64 = false)
        {
            lock(logs)
            {
                if (logs.ContainsKey(logFileName))
                    return logs[logFileName];

                var logFileWrapper = new DbgLog(logFileName, base64);
                logs.Add(logFileName, logFileWrapper);

                return logFileWrapper;
            }
        }

        public static DbgLog dbg
        {
            get
            {
                return getLog("dbg.log", false);
            }
        }

        protected static volatile bool isDisposed = false;
        public static void allLogsDispose()
        {
            lock (logs)
            {
                isDisposed = true;
                foreach (var log in logs)
                {
                    log.Value.Dispose();
                }

                logs.Clear();
            }
        }

        public int hours = 2;
        public int maxSizeLog = 1024 * 1024 * 2;

        public static string toBase64(string str)
        {
            if (String.IsNullOrEmpty(str))
                return "";

            return Convert.ToBase64String(  Encoding.UTF8.GetBytes(str)  );
        }

        public static string fromBase64(string base64)
        {
            if (String.IsNullOrEmpty(base64))
                return "";

            return Encoding.UTF8.GetString(  Convert.FromBase64String(base64)  );
        }

        volatile int locked = 0;
        public void @lock()
        {
            main.WaitOne();
            try
            {
                locked = Thread.CurrentThread.ManagedThreadId;
            }
            catch
            {
                main.ReleaseMutex();
                throw;
            }
        }

        public void unlock()
        {
            locked = 0;
            main.ReleaseMutex();
        }

        public readonly Mutex main;

        public long lastTruncateCheck = 0;
        public bool truncateLog(int hours)
        {
            if (hours <= 0)
                return false;

            if (locked != Thread.CurrentThread.ManagedThreadId)
                throw new Exception("options.DbgLog class truncateLog: locked is false");

            try
            {
                if (lastTruncateCheck + 30L * 60 * 1000 * 10000L > DateTime.Now.Ticks) // раз в пол-часа
                    return false;

                lastTruncateCheck = DateTime.Now.Ticks;
                var fi = new FileInfo(FileName);
                if (fi.Exists && fi.Length > maxSizeLog && fi.CreationTime.AddHours(hours) < DateTime.Now)
                {
                    var lines = new List<string>(  File.ReadAllLines(FileName)  );
                    if (lines.Count <= 1)
                        return false;

                    var firstLine = (lines.Count << 2)/5;           // удаляем до первой строки-разделителя
                    for (; firstLine < lines.Count; firstLine++)
                    {
                        if (lines[firstLine].Trim().Length <= 0)
                            break;
                    }

                    lines.RemoveRange(0, firstLine);

                    fi.Delete();

                    File.WriteAllLines(FileName, lines);

                    fi = new FileInfo(FileName);
                    fi.CreationTime = DateTime.Now;

                    return true;
                }
            }
            finally
            {
            }

            return false;
        }

        protected StringBuilder                     diskStringBuilder = new StringBuilder(32636);
        protected List<String>                      toLogInformation  = new List<string>();
        protected ConcurrentQueue<workDelegate>     worksItems        = new ConcurrentQueue<workDelegate>();

        public    volatile Exception logError        = null;
        protected volatile int       loggingIsEnded  = 0;
        protected volatile bool      loggingIsEndedD = false;
        protected void disklogging()
        {
            bool flag;
            do
            {
                newDiskTask.WaitOne();
                newDiskTask.Reset();

                lock (toLogInformation)
                {
                    flag = loggingIsEnded != 0;
                    foreach (var line in toLogInformation)
                    {
                        diskStringBuilder.AppendLine(line);
                    }

                    toLogInformation.Clear();
                }

                if (diskStringBuilder.Length > 0)
                {
                    @lock();
                    try
                    {
                        truncateLog(hours);
                        File.AppendAllText(FileName, diskStringBuilder.ToString());
                        diskStringBuilder.Clear();
                    }
                    catch (Exception e)
                    {
                        logError = e;
                    }
                    finally
                    {
                        unlock();
                    }
                }
            }
            while (flag || !terminated);

            loggingIsEndedD = true;
            newDiskTask.Set();  // Это для Dispose
        }

        protected void logging()
        {
            loggingIsEnded++;

            try
            {
                do
                {
                    if (worksItems.Count == 0)
                        newTask.WaitOne();

                    if (worksItems.Count > 0)
                    try
                    {
                        workDelegate workItem;
                        if (worksItems.TryDequeue(out workItem))
                            workItem(null);

                        newDiskTask.Set();
                    }
                    catch (Exception e)
                    {
                        logError = e;
                    }
                }
                while (!terminated || worksItems.Count > 0);
            }
            finally
            {
                loggingIsEnded--;
                newDiskTask.Set();
                newTask.Set();
            }
        }

        public void toLog(SortedList<string, string> messages, SortedList<string, string> nonBase64Messages, string type, string errorType, string message)
        {
            if (disabled || isDisposed)
                return;

            var date   = DateTime.Now;
            var thread = Thread.CurrentThread.ManagedThreadId;

            worksItems.Enqueue
            (
                delegate(object obj)
                {
                    if (errorType == null)
                        errorType = "";
                    if (type == null)
                        type = "";

                    lock (logRegime)
                    {
                        int regime;
                        getLogRegime(errorType, out regime);

                        if (regime < 0)
                            return;
                    }

                    var list = new List<string>();

                    list.Add("\r\n\r\n");
                    list.Add("time: "      + date.ToLongDateString() + " " + date.ToLongTimeString());
                    list.Add("timeTicks: " + date.Ticks.ToString());
                    list.Add("thread: "    + thread);
                    list.Add("etype: "     + errorType);
                    list.Add("type: "      + type);


                    toLinesArray(nonBase64Messages, list, false);
                    toLinesArray(messages,          list, base64Format);

                    if (base64Format && message != null)
                    {
                        message = toBase64(message);
                    }

                    if (message == null)
                       message = "null";

                    list.Add("message" + (base64Format ? ":: " : ": ") + message);

                    lock (toLogInformation)
                    {
                        toLogInformation.AddRange(list);
                    }
                }
            );

            newTask.Set();
        }

        public static void toLinesArray(SortedList<string, string> messages, List<string> list, bool base64Format)
        {
            if (messages == null)
                return;

            foreach (var s in messages)
            {
                var key = s.Key;
                var msg = s.Value;

                key = "_ " + key.Replace("\r", "\\r").Replace("\n", "\\n").Replace(": ", "\\:\\ ");  // чтобы нельзя было затереть предопределённые поля
                if (base64Format)
                    msg = toBase64(msg);

                list.Add(key + (base64Format ? ":: " : ": ") + msg);
            }
        }

        public void toLog(string type, string errorType, string logMessage)
        {
            toLog(null, null, type, errorType, logMessage);
        }

        public static readonly string ERROR     = "ERROR";
        public static readonly string MESSAGE   = "MESSAGE";
        public static readonly string DATA      = "DATA";
        public void errorToLog(string type, string logMessage)
        {
            toLog(type, ERROR, logMessage);
        }

        public void messageToLog(string type, string logMessage)
        {
            toLog(type, MESSAGE, logMessage);
        }

        public void varToLog(string type, string varName, object varValue)
        {
            varToLog(  type, varName, varValue == null ? null : varValue.ToString()  );
        }

        public void dataToLog(string type, string logName, object value, string message = null)
        {
            var list = new SortedList<string, string>();

            if (value == null)
                list.Add("data", "null");
            else
                setFieldsList(value, list);

            toLog(list, null, type, DATA, message);
        }

        private static void setFieldsList(object value, SortedList<string, string> list)
        {
            var members = value.GetType().GetMembers();
            foreach (var member in members)
            {
                if (member.MemberType != System.Reflection.MemberTypes.Field
                    && member.MemberType != System.Reflection.MemberTypes.Property)
                    continue;

                var field = member as FieldInfo;
                var prop = member as PropertyInfo;
                try
                {
                    var obj = field == null ? prop.GetValue(value, null) : field.GetValue(value);
                    list.Add(  "data." + member.Name, getString(obj)  );
                }
                catch (Exception e)
                {
                    list.Add("error." + member.Name, e.Message);
                }
            }
        }

        public void varToLog(string type, string varName, string varValue)
        {
            var l = new SortedList<string, string>(1);

            l.Add("var " + varName, getString(varValue)  );
            toLog(l, null, type, DATA, null);
        }

#if forLinux
        private static int GetLastError() {return -1;}
#else
        [DllImport("kernel32")]
		private extern static int GetLastError();
#endif

        public void funcResultToLog(string type, string funcName, int result, bool getLastError = true)
        {
            var l = new SortedList<string, string>(1);

            l.Add("function " + funcName, result.ToString());
            if (getLastError && result == 0)
            {
                l.Add("getLastError", GetLastError().ToString());
            }

            toLog(l, null, type, DATA, null);
        }

        public void Dispose()
        {
            if (terminated)
                return;

            terminated = true;

            // Завершаем все потоки, посылая в них newTask.Set(). Т.к. нет гарантии, что Set обязательно активирует каждый поток, исполняется в цикле
            while (loggingIsEnded > 0)
            {
                for (int i = 0; i < loggingIsEnded; i++)
                {
                    newTask.Set();
                    WaitHandle.WaitAll(new WaitHandle[] {newDiskTask, newTask}, 100);
                }

                newDiskTask.Set();
            }

            while (!loggingIsEndedD)
            {
                Thread.Sleep(20);
                newDiskTask.WaitOne();
            }

            main.WaitOne();
            locked = 0;
            main.ReleaseMutex();

            main.Close();

            newTask.Close();
            newDiskTask.Close();
        }

        ~DbgLog()
        {
            if (!terminated)
                Dispose();
        }
    }
}
