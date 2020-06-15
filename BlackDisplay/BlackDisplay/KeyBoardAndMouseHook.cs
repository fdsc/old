using System.Runtime.InteropServices;
using System;
using options;
using System.Threading;

// ms-help://MS.MSDNQTR.v90.en/enu_kbnetframeworkkb/netframeworkkb/318804.htm
namespace BlackDisplay
{
    public partial class Form1
    {
#if forLinux
        public static readonly int registerHooksResult = -1;

        internal static void CloseHandle(int bin)
        {
            throw new NotImplementedException();
        }
#else
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        //Declare the hook handle as an int.
        static int hHook1 = 0, hHook2 = 0;

        //Declare MouseHookProcedure as a HookProc type.
        static HookProc mproc = null, kproc = null;

        //Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public class POINT 
        {
	        public int x;
	        public int y;

            public POINT(int x, int y)
            {
            }
        }

        static readonly int WH_MOUSE_LL = 14, WH_KEYBOARD_LL = 13;

        //Declare the wrapper managed MouseHookStruct class.
        [StructLayout(LayoutKind.Sequential)]
        public class MSLLHOOKSTRUCT 
        {
	        /*public POINT pt;
	        public int hwnd;
	        public int wHitTestCode;
	        public int dwExtraInfo;*/
            public POINT pt;
            int mouseData;
            int flags;
            int time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBLLHOOKSTRUCT 
        {
	        public int     vkCode;
            public int     scanCode;
            public int     flags;
            public int     time;
            public IntPtr  dwExtraInfo;
        }

        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll",CharSet=CharSet.Auto,
         CallingConvention=CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //This is the Import for the UnhookWindowsHookEx function.
        //Call this function to uninstall the hook.
        [DllImport("user32.dll",CharSet=CharSet.Auto,
         CallingConvention=CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll",CharSet=CharSet.Auto,
         CallingConvention=CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,  int wParam, IntPtr lParam);

        public static int registerHooks(bool register = true, bool forced = false)
        {
            if (forced && register)
            {
                registerHooksForced(false);
                return registerHooksForced(true);
            }

            if (register)
            {
                RegisterCounter++;
                if (RegisterCounter <= 1)
                    return registerHooksForced(register);
            }
            else
            {
                RegisterCounter--;
                if (RegisterCounter <= 0)
                    return registerHooksForced(register);
            }

            return 0;
        }

        public static int registerHooksResult = 0;
        public static int registerHooksForced(bool register = true)
        {
            DbgLog.dbg.dataToLog("registerHooks", "", new {register = register, registerHooksResult = registerHooksResult});

            /*if (register && registerHooksResult < 0)
                return -3;
            */
            // DbgLog.dbg.messageToLog("registerHooks", "register = " + register);

            if ((hHook1 == 0 || hHook2 == 0) && register)
	        {
	                // Create an instance of HookProc.
                if (mproc == null)
		        mproc = new HookProc(Form1.LowLevelMouseHookProc);
                if (kproc == null)
                kproc = new HookProc(Form1.LowLevelKeyboardHookProc);

                if (hHook1 == 0)
		            hHook1 = SetWindowsHookEx(  WH_MOUSE_LL,
					                            mproc,
					                            (IntPtr)0,
					                            0 //AppDomain.GetCurrentThreadId()
                                                );

                if (hHook2 ==0)
                    hHook2 = SetWindowsHookEx(  WH_KEYBOARD_LL,
					                            kproc,
					                            (IntPtr)0,
					                            0 //AppDomain.GetCurrentThreadId()
                                                );

		        if(hHook1 == 0 || hHook2 == 0)
		        {
                    Program.toLogFile("Не удалось зарегистрировать хуки! " + hHook1 + " / " + hHook2 + " " + GetLastError());

			        registerHooksResult = -1;
			        return registerHooksResult;
		        }

                lastMouseMoveTime = DateTime.Now.Ticks;
                lastKeyDownTime   = lastMouseMoveTime;

                DbgLog.dbg.messageToLog("registerHooks", "registered");

                registerHooksResult = 1;
                return 1;
	        }
	        else
            if (/*(hHook1 != 0 || hHook2 != 0) && */!register)
	        {
                bool ret1 = true, ret2 = true;
                if (hHook1 != 0)
                {
		            ret1 = UnhookWindowsHookEx(hHook1);
                    if (ret1 == false)
		            {
                        Program.toLogFile("Не удалось снять хуки с регистрации! " + ret1 + " " + GetLastError());

                        registerHooksResult = -2;
		            }
                }
                if (hHook2 != 0)
                    ret2 = UnhookWindowsHookEx(hHook2);

                hHook1 = 0;
                hHook2 = 0;

		        if (ret1 == false || ret2 == false)
		        {
                    Program.toLogFile("Не удалось снять хуки с регистрации! " + ret1 + " / " + ret2 + " " + GetLastError());

                    registerHooksResult = -2;
			        return registerHooksResult;
		        }

                DbgLog.dbg.messageToLog("registerHooks", "unregistered");

                registerHooksResult = 2;
                return 2;
	        }

            return 0;
        }


        static readonly int WM_LBUTTONDOWN   = 0x0201;
        static readonly int WM_LBUTTONUP     = 0x0202;
        static readonly int WM_LBUTTONDBLCLK = 0x0203;
        static readonly int WM_RBUTTONDOWN   = 0x0204;
        static readonly int WM_RBUTTONUP     = 0x0205;
        static readonly int WM_RBUTTONDBLCLK = 0x0206;
        static readonly int WM_MBUTTONDOWN   = 0x0207;
        static readonly int WM_MBUTTONUP     = 0x0208;
        static readonly int WM_MOUSEHWHEEL   = 0x020E;
        static readonly int WM_MOUSEMOVE     = 0x0200;
        static readonly int WM_KEYDOWN       = 0x0100;
        static readonly int WM_SYSKEYDOWN    = 0x0104;
        static readonly int WM_KEYUP         = 0x0101;
        static readonly int WM_SYSKEYUP      = 0x0105;

        public static long  lastMouseMoveTime = 0;
        // public static POINT lastPoint = new POINT(0, 0);
        public static int LowLevelMouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            try
            {
	            MSLLHOOKSTRUCT MouseHookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

	            if (nCode < 0)
	            {
		            return CallNextHookEx(0, nCode, wParam, lParam);
	            }
	            else
	            {
                    if (   wParam == WM_LBUTTONDOWN
                        || wParam == WM_RBUTTONDOWN
                        || wParam == WM_MBUTTONDOWN
                        || wParam == WM_LBUTTONUP
                        || wParam == WM_RBUTTONUP
                        || wParam == WM_MBUTTONUP
                        || wParam == WM_LBUTTONDBLCLK
                        || wParam == WM_RBUTTONDBLCLK
                        || wParam == WM_MOUSEHWHEEL
                        )
                    {
                        lastKeyDownTime = DateTime.Now.Ticks;

                        if (MouseHookProcEvent != null && (wParam == WM_LBUTTONUP || wParam == WM_RBUTTONUP || wParam == WM_MBUTTONUP || wParam == WM_MOUSEHWHEEL))
                        {
                            MouseHookProcEvent(MouseHookStruct.pt.x, MouseHookStruct.pt.y, wParam);
                        }
                    }

                    if (wParam == WM_MOUSEMOVE)
                    {
                        /*if (Math.Abs(lastPoint.x - MouseHookStruct.pt.x) > 16 || Math.Abs(lastPoint.y - MouseHookStruct.pt.y) > 16)
                            lastMouseMoveTime = DateTime.Now.Ticks;
                            */
                        lastMouseMoveTime = DateTime.Now.Ticks;

                        if (MouseHookProcEvent != null)
                            MouseHookProcEvent(MouseHookStruct.pt.x, MouseHookStruct.pt.y, wParam);
                    }

		            try
                    {
                        return CallNextHookEx(0, nCode, wParam, lParam);
                    }
                    catch
                    {
                        return 0;
                    }
	            }
            }
            catch
            {
                try
                {
                    return CallNextHookEx(0, nCode, wParam, lParam);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static long lastKeyDownTime = 0;
        private static int lastScanCode = -1;
        public static int LowLevelKeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            try
            {
	            KBLLHOOKSTRUCT KeyboardHookStruct = (KBLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBLLHOOKSTRUCT));

	            if (nCode < 0)
	            {
		            return CallNextHookEx(0, nCode, wParam, lParam);
	            }
	            else
	            {
                    if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                    {
                        if (KeyboardHookStruct.scanCode != lastScanCode) // иначе запавшая клавиша тут тоже будет считаться работой пользователя
                        {
                            lastKeyDownTime = DateTime.Now.Ticks;
                            lastScanCode = KeyboardHookStruct.scanCode;
                        }
                    }

                    if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                    {
                        if (KeyboardHookProcEvent != null)
                            KeyboardHookProcEvent(KeyboardHookStruct.scanCode);

                        lastKeyDownTime = DateTime.Now.Ticks;
                        lastScanCode = -1;
                    }

		            return CallNextHookEx(0, nCode, wParam, lParam);
	            }
            }
            catch
            {
                try
                {
                    return CallNextHookEx(0, nCode, wParam, lParam);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public delegate void KeyboardHookProcRaise(int vkCode);
        public delegate void MouseHookProcRaise(int x, int y, int wParam);

        protected static event KeyboardHookProcRaise KeyboardHookProcEvent;
        protected static event MouseHookProcRaise    MouseHookProcEvent;

        protected volatile static int RegisterCounter = 0;
        public static void register(KeyboardHookProcRaise khp, MouseHookProcRaise mhp)
        {
            /*if (RegisterCounter == 0)
            {*/
                registerHooks();
            /*}
            else
                RegisterCounter++;*/

            KeyboardHookProcEvent += khp;
            MouseHookProcEvent    += mhp;
        }

        protected static bool isDisposed = false;
        public static void unregister(KeyboardHookProcRaise khp, MouseHookProcRaise mhp)
        {
            /*if (RegisterCounter <= 0 && !isDisposed)
            {
                isDisposed = true;*/
                registerHooks(false);
            /*}
            else
            if (!isDisposed)
                RegisterCounter--;*/

            KeyboardHookProcEvent -= khp;
            MouseHookProcEvent    -= mhp;
        }
#endif
    }
}
