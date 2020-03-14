using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using keccak;
using System.Threading;
using System.IO;

namespace BlackDisplay
{
    public partial class SimplePasswordBox : UserControl
    {
        public SimplePasswordBox()
        {
            InitializeComponent();
            box.Size = this.Size;
        }

        protected virtual void SimplePasswordBox_Resize(object sender, EventArgs e)
        {
            box.Size = this.Size;
        }

        public static string printBytes(BytesBuilder bb, bool masked)
        {
            var sb = new StringBuilder();

            var bytes = bb.getBytes();

            if (masked)
                for (int i = 0; i < bytes.LongLength; i += 2)
                {
                    if (bytes[i] != 13 && bytes[i] != 10)
                        #if forLinux
                        sb.Append("*");
                        #else
                        sb.Append(/*"•"*//*"®"*/ "●");
                        #endif
                    else
                        if (bytes[i] == 10)
                            sb.Append("\r");
                        /*else
                            sb.Append("\n");*/
                }
            else
                sb.Append(Encoding.Unicode.GetString(bytes));

            return sb.ToString();
        }

        static readonly SHA3         sha     = new SHA3(1024);
        static readonly BytesBuilder bbh     = new BytesBuilder();

        BytesBuilder        bb      = new BytesBuilder();

        public    bool isHaotic = false;
        protected bool isMasked = false;
        public virtual bool Masked
        {
            get
            {
                return isMasked;
            }
            set
            {
                if (isMasked != value)
                {
                    isMasked = value;
                    showPasswordInBox();
                }
            }
        }

        public string text
        {
            get
            {
                var bytes = bb.getBytes();
                var result = Encoding.Unicode.GetString(bytes);

                BytesBuilder.ToNull(bytes);

                return result;
            }
        }

        public string textReversed
        {
            get
            {
                var bytes = bb.getBytes();
                for (int i = 0, j = bytes.Length-1; i+1 < j-1; i += 2, j -= 2)
                {
                    if (i+1 >= bytes.Length || j < 1)
                        break;

                    var a = bytes[i+1];
                    bytes[i+1] = bytes[j+0];
                    bytes[j+0] = a;

                    a = bytes[i+0];
                    bytes[i+0] = bytes[j-1];
                    bytes[j-1] = a;

                    a = 0;
                }
                var result = Encoding.Unicode.GetString(bytes);

                BytesBuilder.ToNull(bytes);

                return result;
            }
        }

        public byte[] inputBytes
        {
            get
            {
                return bb.getBytes();
            }
        }

        public static byte[] inputBytesHaotic
        {
            get
            {
                lock (bbh)
                    return bbh.getBytes();
            }
        }

        public static long countOfHaoticBytes
        {
            get
            {
                lock (bbh)
                    return bbh.Count;
            }
        }

        protected virtual void box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isHaotic)
            {
                return;
            }

            e.Handled = true;
            var kc = (ushort) e.KeyChar;
            if (kc < 32/* && kc != 13*/ /* && kc != 9*/)
            {
                return;
            }

            bb.addUshort(kc, box.SelectionStart);

            showPasswordInBox();
            box.SelectionStart = box.SelectionStart + 1;
        }

        protected virtual void showPasswordInBox()
        {
            int i = box.SelectionStart;
            box.Text = printBytes(bb, Masked);
            // box.Select(box.Text.Length, 0);

            if (i <= box.Text.Length)
                box.SelectionStart = i;
            else
                box.SelectionStart = box.Text.Length;
        }

        private static string getTextFromBuffer()
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

            return pwdToCheck;
        }

        private void box_KeyDown(object sender, KeyEventArgs e)
        {
            if (isHaotic)
            {
                e.SuppressKeyPress = true;
                return;
            }
            
            if (e.Control && e.KeyCode == Keys.V)
            {
                var str = getTextFromBuffer();
                if (!String.IsNullOrEmpty(str))
                {
                    var strc = str.ToCharArray();
                    for (int i = 0; i < strc.Length; i++)
                    {
                        bb.addUshort((ushort) strc[i], box.SelectionStart + i);
                    }

                    showPasswordInBox();
                    box.SelectionStart = box.SelectionStart + str.Length;
                }

                e.SuppressKeyPress = true;
                return;
            }

            // Клавиши удаления
            if (!isHaotic && (e.KeyValue == 8 || e.KeyValue == 46))
            {
                e.SuppressKeyPress = true;

                // Можно использовать backspace и delete, если ввод не маскирован
                //if (!isMasked)
                {
                    // backspace, но удаляем только один символ
                    if (e.KeyValue == 8 && box.SelectionLength == 0)
                    {
                        if (box.SelectionStart > 0)
                        {
                            var n = box.SelectionStart - 1;
                            /*if (bb.getBlock(n)[1] == 13 && n > 0 && bb.getBlock(n - 1)[1] == 10)
                            {
                                bb.RemoveBlocks(n - 1, n);
                                box.SelectionStart = box.SelectionStart - 2;
                            }
                            else
                            {*/
                                bb.RemoveBlocks(n, n);
                                box.SelectionStart = n;
                            //}
                        }
                        showPasswordInBox();
                    }
                    else
                    if (e.KeyValue == 46 || e.KeyValue == 8)
                    {
                        if (box.SelectionStart < box.Text.Length)
                        {
                            var len = box.SelectionLength;
                            if (len == 0)
                                len = 1;

                            bb.RemoveBlocks(box.SelectionStart, box.SelectionStart + len - 1);
                        }
                        showPasswordInBox();
                    }
                }

                return;
            }

            if (e.KeyValue == 13)
            {
                e.SuppressKeyPress = true;
                bb.addUshort(10, box.SelectionStart);
                showPasswordInBox();
                box.SelectionStart = box.SelectionStart + 1;
            }
            else
            // Очистка при нажатии клавиши Esc
            if (e.KeyValue == 27)
            {
                box.Text = "";
                bb.clear();
                GC.Collect();

                return;
            }
        }

        public virtual void Clear()
        {
            box.Text = "";
            bb.clear();
            GC.Collect();
        }

        public virtual void ClearHaotic()
        {
            box.Text = "";
            bb.clear();
            lock (bbh)
                bbh.clear();

            GC.Collect();
        }

        private void box_KeyUp(object sender, KeyEventArgs e)
        {
            if (isHaotic && !isGlobalBackground)
            {
                KeyboardHaotic(e.KeyValue, 100 * 10000);
                setBitsCountToBox();
                return;
            }
        }

        delegate void voidDelegate();
        voidDelegate vd = null;
        void hookPwd_HookProcEvent(int vkCode)
        {
            if (isHaotic && isGlobalBackground)
            {
                if (vd == null)
                    vd = setBitsCountToBox;

                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        try
                        {
                            if (!KeyboardHaotic(vkCode, /*584*/ 100 * 10000)) // иначе не-фон получается дебильным
                                return;

                            if (!this.IsDisposed)
                            if (this.InvokeRequired)
                                this.Invoke(vd);
                            else
                                setBitsCountToBox();
                        }
                        catch
                        {}
                    }
                );
            }
        }

        static SHA3.SHA3Random rnd;
        static SimplePasswordBox()
        {
            var sha = new SHA3(0);
            var b   = sha.CreateInitVector(72, 4096, 40);
            rnd     = new SHA3.SHA3Random(b);

            BytesBuilder.ToNull(b);
            sha.Clear(true);

        }

        public static bool KeyboardHaotic(int vkCode, long time)
        {
            var dt = DateTime.Now.Ticks;

            lock (sync)
            {
                if (dt - lastMouseTick > time)
                {
                    byte[] bytes, data = new byte[5];
                    var msg = new byte[4 + 8];
                    lock (rnd)
                    {
                        BytesBuilder.UIntToBytes ((uint)  vkCode             + rnd.nextInt() , ref msg, 0);
                        BytesBuilder.ULongToBytes((ulong) DateTime.Now.Ticks + rnd.nextLong(), ref msg, 4);
                    }

                    lock (sha)
                    {
                        bytes = sha.getDuplex(msg, true);
                    }
                    BytesBuilder.CopyTo(bytes, data);

                    BytesBuilder.ToNull(msg);
                    BytesBuilder.ToNull(bytes);

                    lock (bbh)
                    {
                        bbh.add(data);
                    }

                    /*
                    var t1 = vkCode >> 16;
                    var t2 = vkCode & 0xFFFF;

                    lock (bbh)
                    {
                        bbh.addByte((byte)((t1 >> 8) + t1 + t2 + (t2 >> 8)));
                        bbh.addInt((int)((DateTime.Now.Ticks >> 32) + dt));
                    }
                    */
                    //bbh.addULong((ulong) DateTime.Now.Ticks);
                    lastMouseTick = DateTime.Now.Ticks;

                    return true;
                }
                else
                    return false;
            }
        }


        private void box_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (isHaotic)
                e.IsInputKey = true;
            else
                base.OnPreviewKeyDown(e);
        }

        private void box_MouseUp(object sender, MouseEventArgs e)
        {
            if (isHaotic && !isMouseAllow && !isGlobalBackground)
            {
                byte[] bytes, data = new byte[5];
                var msg = new byte[8 + 8];
                lock (rnd)
                {
                    BytesBuilder.UIntToBytes ((uint)  e.X                + rnd.nextInt() , ref msg, 0);
                    BytesBuilder.UIntToBytes ((uint)  e.Y                + rnd.nextInt() , ref msg, 4);
                    BytesBuilder.ULongToBytes((ulong) DateTime.Now.Ticks + rnd.nextLong(), ref msg, 8);
                }

                lock (sha)
                {
                    bytes = sha.getDuplex(msg, true);
                }
                BytesBuilder.CopyTo(bytes, data);

                BytesBuilder.ToNull(msg);
                BytesBuilder.ToNull(bytes);

                lock (bbh)
                {
                    //bbh.addInt(e.X + (e.Y << 18) + (int) e.Button);
                    //bbh.addInt((int) ((DateTime.Now.Ticks >> 32) + DateTime.Now.Ticks));
                    //bbh.addULong((ulong) DateTime.Now.Ticks);
                    bbh.add(data);
                }

                setBitsCountToBox();
                return;
            }
        }

        private void setBitsCountToBox()
        {
            lock (bbh)
                box.Text = "" + (bbh.Count / 9) + " битов (" + (bbh.Count / (9 * 8)) + " байтов)";
        }

        public bool isMouseAllow = false;
        public bool hookPwd = false;

#if forLinux
        public bool isGlobalBackground
        {
            get
            {
                return false;
            }
            set
            {}
        }
#else
        public bool isGlobalBackground
        {
            get
            {
                return hookPwd;
            }
            set
            {
                if (value && !hookPwd)
                {
                    hookPwd = true;
                    Form1.register(new Form1.KeyboardHookProcRaise(hookPwd_HookProcEvent), new Form1.MouseHookProcRaise(hookPwd_MouseHookProcEvent));
                }
                else
                if (hookPwd)
                {
                    Form1.unregister(new Form1.KeyboardHookProcRaise(hookPwd_HookProcEvent), new Form1.MouseHookProcRaise(hookPwd_MouseHookProcEvent));
                    hookPwd = false;
                }
            }
        }
#endif
        void hookPwd_MouseHookProcEvent(int x, int y, int wParam)
        {
            if (isHaotic && isGlobalBackground)
            {
                if (vd == null)
                    vd = setBitsCountToBox;

                ThreadPool.QueueUserWorkItem
                (
                delegate
                {
                    try
                    {
                        if (!MouseHaoticProg(x, y, /*1589*/ 100 * 10000, wParam))
                            return;

                        if (!this.IsDisposed)
                        if (this.InvokeRequired)
                            this.Invoke(vd);
                        else
                            setBitsCountToBox();
                    }
                    catch
                    {
                    }
                }
                );
            }
        }

        public static bool MouseHaoticProg(int x, int y, long time, int wParam)
        {
            lock (sync)
            {
                if (lastMouseTick == 0)
                {
                    mouseX = x;
                    mouseY = y;
                    lastMouseTick = DateTime.Now.Ticks;

                    return false;
                }
                else
                if (Math.Abs(mouseX - x) + Math.Abs(mouseY - y) > 128)
                {
                    var dt = DateTime.Now.Ticks;
                    if (dt - lastMouseTick > time)
                    {
                        mouseX = x;
                        mouseY = y;

                        byte[] bytes, data = new byte[4];
                        var msg = new byte[8 + 8 + 4];
                        lock (rnd)
                        {
                            BytesBuilder.UIntToBytes ((uint)  mouseX + rnd.nextInt() , ref msg, 0);
                            BytesBuilder.UIntToBytes ((uint)  mouseY + rnd.nextInt() , ref msg, 4);
                            BytesBuilder.ULongToBytes((ulong) dt     + rnd.nextLong(), ref msg, 8);
                            BytesBuilder.UIntToBytes ((uint)  wParam + rnd.nextInt() , ref msg, 16);
                        }

                        lock (sha)
                        {
                            bytes = sha.getDuplex(msg, true);
                        }
                        BytesBuilder.CopyTo(bytes, data);

                        BytesBuilder.ToNull(msg);
                        BytesBuilder.ToNull(bytes);

                        lock (bbh)
                        {
                            // bbh.addInt((x + (y << 17) + wParam) ^ (int)(dt - lastMouseTick));
                            bbh.add(data);
                        }

                        lastMouseTick = dt;
                    }
                }
            }

            return true;
        }

        static volatile int  mouseX = 0;
        static volatile int  mouseY = 0;
        static          long lastMouseTick = 0;
        static        object sync   = new object();
        private void box_MouseMove(object sender, MouseEventArgs e)
        {
            if (isHaotic && isMouseAllow && !isGlobalBackground)
            {
                MouseHaoticProg(e.X, e.Y, 100 * 10000, 0);
                setBitsCountToBox();
            }
        }

        public static void ClearFirstInputBytes(long generated)
        {
            lock (bbh)
            {
                long count = 0;
                while (count < generated * 72)
                {
                    if (bbh.bytes.Count > 0)
                        count += bbh.RemoveBlockAt(0);
                    else
                        break;
                }
            }
        }

        public void ClearFirstInputBytesInBox(long generated)
        {
            ClearFirstInputBytes(generated);
            setBitsCountToBox();
        }
    }
}
