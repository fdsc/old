using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VinPlaning;
using System.IO;

namespace AttentionTests
{
    // http://www.vashpsixolog.ru/psychodiagnostic-school-psychologist/61/480-test-black-and-red-table-gorbov-schulte-score-switching-attention
    // http://vsetesti.ru/314/
    // http://vsetesti.ru/313/
    public partial class RedBlack : Form
    {
        public static RedBlack opened = null;
        public RedBlack()
        {
            opened = this;

            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            vw = new VirtualWindow(this);
        }

        VirtualWindow vw;

        private void RedBlack_Shown(object sender, EventArgs e)
        {
            MessageBox.Show("Черно-красная таблица Горбова-Шульте\r\n\r\n" + 
                "На таблице 25 красных цифр от 1 до 25 и 24 чёрные цифры от 1 до 24.\r\n" + 
                "Нужно выбирать левой кнопкой мыши красные цифры в возрастающем порядке от 1 до 25, а черные - в убывающем порядке от 24 до 1.\r\n" + 
                "Причём необходимо вести счет попеременно: сначала выбирать красную цифру, потом черную, затем вновь красную, а за ней черную - до тех пор, пока счет не будет окончен. Выполнять задание нужно быстро и без ошибок\r\n\r\n" + 
                // "При правильном выборе число будет зачёркнуто, при неправильном выборе необходимо найти верное число. Отсчёт времени начинается с нажатия кнопки 'OK'.\r\n\r\n" + 
                "При неправильном выборе сетка таблицы станет жёлтой, необходимо исправиться - найти верное число. Отсчёт времени начинается с нажатия кнопки 'OK'.\r\n\r\n" + 
                "Первое число - красное на чёрном 1, далее чёрное на красном - 24!");

            startTest();
        }

        public struct input
        {
            public input(int inputNumber, int wellNumber)
            {
                number = inputNumber;
                time   = DateTime.Now.Ticks;
                well   = wellNumber;
            }

            public readonly long time;
            public readonly int  number;
            public readonly int  well;
        }

        List<input> inputs = new List<input>(64);


        int [,] numbers = new int[7, 7];

        int  h0, x0, y0;
        int  red = 1, black = 24;
        bool isRed = true;
        private void startTest()
        {
            h0 = vw.vw.Height - 10;
            y0 = 5;
            x0 = (vw.vw.Width - h0) / 2;

            using (var g = Graphics.FromImage(vw.vw))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.DrawRectangle(new Pen(Color.Black, 8), x0 - 3, y0 - 3, h0, h0);

                /*
                int cnt = 0;
                for (int i = 0; i < 100; i++)
                {
                    initNumbers     (numbers);
                    RandomNumbers   (numbers);
                    if (!isNoWell(numbers))
                        cnt++;
                }
                cnt++;
                */

                initNumbers     (numbers);
                RandomNumbers   (numbers);
                FillRectangles  (numbers, g);
            }

            vw.repaint();
            inputs.Add(new input(0, 0));
        }

        Brush Black = new SolidBrush(Color.Black), Red = new SolidBrush(Color.FromArgb(255, 0x99, 00, 00)), RedText = new SolidBrush(Color.FromArgb(255, 0xAA, 00, 00)), Gray = new SolidBrush(Color.FromArgb(255, 77, 77, 55)), Yellow = new SolidBrush(Color.Yellow);
        Font  font  = new Font("Arial", 32, FontStyle.Bold);
        private void FillRectangles(int[,] numbers, Graphics g)
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                {
                    g.FillRectangle(numbers[i, j] > 0 ? Black : Red, x0 + (h0 * i) / 7, y0 + (h0 * j) / 7, h0 / 7, h0 / 7);
                    var s = g.MeasureString("" + Math.Abs(numbers[i, j]), font);

                    g.DrawString("" + Math.Abs(numbers[i, j]), font, numbers[i, j] > 0 ? RedText : Black, x0 + (h0 * i) / 7 + h0 / 14 - s.Width / 2, y0 + (h0 * j) / 7 + h0 / 14 - s.Height / 2);
                }

            drawCells(g, Gray);
        }

        private void drawCells(Graphics g, Brush brush)
        {
            for (int i = 1; i < 7; i++)
            {
                g.FillRectangle(brush, x0 + (i * h0) / 7 - 3, y0 - 3               , 6,  h0);
                g.FillRectangle(brush, x0 - 3               , y0 + (i * h0) / 7 - 3, h0, 6);
            }
        }

        Random rnd = new Random();
        private void RandomNumbers(int[,] numbers)
        {
            for (int i = 0; i < 7*7*1024*16 && !isNoWell(numbers); i++)
            {
                var x1 = rnd.Next(0, 7);
                var y1 = rnd.Next(0, 7);
                var x2 = rnd.Next(0, 7);
                var y2 = rnd.Next(0, 7);
                var x3 = rnd.Next(0, 7);
                var y3 = rnd.Next(0, 7);

                if (x1 == x2 && y1 == y2)
                    continue;
                if (x1 == x3 && y1 == y3)
                    continue;
                if (x3 == x2 && y3 == y2)
                    continue;

                int k1 = numbers[x1, y1];
                int k2 = numbers[x2, y2];
                int k3 = numbers[x3, y3];
                numbers[x1, y1] = k3;
                numbers[x2, y2] = k1;
                numbers[x3, y3] = k2;
            }
        }

        private bool isNoWell(int[,] numbers)
        {
            int r = 1;
            int black = -24;

            int cnt = 0, cntk = 0;
            int x1, y1, x2, y2;
            int k;
            while (r < 25)
            {
                find(r + 0, numbers, out x1, out y1);
                find(r + 1, numbers, out x2, out y2);
                if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) < 3)
                {
                    cnt += 3 - Math.Abs(x1 - x2) - Math.Abs(y1 - y2);
                    if (cnt > 8)
                        return false;
                }

                r++;
            }

            while (black < -1)
            {
                find(black + 0, numbers, out x1, out y1);
                find(black + 1, numbers, out x2, out y2);


                if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) < 4)
                {
                    cnt += 4 - Math.Abs(x1 - x2) - Math.Abs(y1 - y2);
                    if (cnt > 8)
                        return false;
                }

                black++;
            }

            r = 1;
            black = -24;
            while (r < 25 && black <= -1)
            {
                find(r    , numbers, out x1, out y1);
                find(black, numbers, out x2, out y2);

                if (x1 - 1 <= 0 || x2 - 1 <= 0 || 6 - x1 <= 0 || 6 - x2 <= 0 || y1 - 1 <= 0 || y2 - 1 <= 0 || 6 - y1 <= 0 || 6 - y2 <= 0)
                    k = 1;
                else
                    k = 0;

                if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) < 3 + k)
                {
                    cnt += 3 + k - Math.Abs(x1 - x2) - Math.Abs(y1 - y2);
                    cntk++;
                    if (cnt > 14 + cntk / 2)
                        return false;
                }

                find(r + 1, numbers, out x1, out y1);

                if (x1 - 1 <= 0 || x2 - 1 <= 0 || 6 - x1 <= 0 || 6 - x2 <= 0 || y1 - 1 <= 0 || y2 - 1 <= 0 || 6 - y1 <= 0 || 6 - y2 <= 0)
                    k = 1;
                else
                    k = 0;

                if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) < 3 + k)
                {
                    cnt += 3 + k - Math.Abs(x1 - x2) - Math.Abs(y1 - y2);
                    cntk++;
                    if (cnt > 14  + cntk / 2)
                        return false;
                }

                r++;
                black++;
            }

            return true;
        }

        private void find(int r, int[,] numbers, out int x, out int y)
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    if (numbers[i, j] == r)
                    {
                        x = i;
                        y = j;
                        return;
                    }

            throw new Exception("Фатальная ошибка RedBlack.find не может найти заданное число " + r);
        }

        private void initNumbers(int[,] numbers)
        {
            int r = 25, b = 24;
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    if (r > 0)
                        numbers[i, j] = r--;
                    else
                        numbers[i, j] = -(b--);
        }

        bool drawed = false;
        private void RedBlack_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                contextMenuStrip1.Show(e.Location);

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if (red > 25)
            {
                if (!drawed)
                {
                    drawTestResult();
                    drawed = true;
                }
                return;
            }

            int j = (e.Y - y0) * 7 / h0;
            int i = (e.X - x0) * 7 / h0;

            // Проверять координаты надо, т.к. есть округление
            if (e.Y - y0 < 0 || e.X - x0 < 0 || i < 0 || j < 0 || i >= 7 || j >= 7)
            {
                using (var g = Graphics.FromImage(vw.vw))
                {
                    drawCells(g, Gray);
                    g.DrawRectangle(new Pen(Color.Black, 8), x0 - 3, y0 - 3, h0, h0);
                }

                vw.repaint();
                return;
            }

            int inputNumber = numbers[i, j];
            int wellNumber  = isRed ? red : -black;
            inputs.Add(new input(inputNumber, wellNumber));

            using (var g = Graphics.FromImage(vw.vw))
            if (wellNumber == inputNumber)
            {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    if (isRed)
                    {
                        red++;
                        isRed = false;
                        /*
                        g.DrawLine(new Pen(Color.Red, 5), x0 + i * h0 / 7 + 5, y0 + j * h0 / 7 + 5, x0 + (i + 1) * h0 / 7 - 5, y0 + (j + 1) * h0 / 7 - 5);
                        g.DrawLine(new Pen(Color.Red, 5), x0 + i * h0 / 7 + 5, y0 + (j + 1) * h0 / 7 - 5, x0 + (i + 1) * h0 / 7 - 5, y0 + j * h0 / 7 + 5);*/
                    }
                    else
                    {
                        black--;
                        isRed = true;
                        /*
                        g.DrawLine(new Pen(Color.Black, 5), x0 + i * h0 / 7 + 5, y0 + j * h0 / 7 + 5, x0 + (i + 1) * h0 / 7 - 5, y0 + (j + 1) * h0 / 7 - 5);
                        g.DrawLine(new Pen(Color.Black, 5), x0 + i * h0 / 7 + 5, y0 + (j + 1) * h0 / 7 - 5, x0 + (i + 1) * h0 / 7 - 5, y0 + j * h0 / 7 + 5);*/
                    }

                drawCells(g, Gray);
                g.DrawRectangle(new Pen(Color.Black, 8), x0 - 3, y0 - 3, h0, h0);
                g.DrawRectangle(new Pen(Color.Green, 2), x0 + i * h0 / 7, y0 + j * h0 / 7, h0 / 7 + 1, h0 / 7 + 1);
            }
            else
            {
                drawCells(g, Yellow);
                g.DrawRectangle(new Pen(Color.Yellow, 7), x0 - 3, y0 - 3, h0, h0);
            }

            vw.repaint();

            if (red > 25)
            {
                drawTestResult();
                drawed = true;
            }
        }

        long[] times = new long[5];
        long minTime = int.MaxValue; long maxTime = 0;
        int[] colorError = new int[5], numberError = new int[5], sequenceError = new int[5];
        int colorErrors = 0, numberErrors = 0, sequenceErrors = 0, allErrors = 0;
        int stage = 0;
        private void drawTestResult()
        {
            var dtt = DateTime.Now;
            //File.WriteAllText("redblacktable.log", dtt.ToShortDateString() + " " + dtt.ToLongTimeString() +  "\r\n");

            long startTest = inputs[0].time;
            long endTest   = 0;

            for (int i = 1; i < inputs.Count; i++)
            {
                //File.AppendAllText("redblacktable.log", inputs[i].number + ":" + inputs[i].well + ":");

                if (inputs[i].time - inputs[i - 1].time < minTime)
                    minTime = inputs[i].time - inputs[i - 1].time;

                if (inputs[i].time - inputs[i - 1].time > maxTime)
                    maxTime = inputs[i].time - inputs[i - 1].time;

                if (inputs[i].number != inputs[i].well)
                {
                    bool flag = false;
                    if (inputs[i].number == inputs[i].well - 2 || inputs[i].number == -(inputs[i].well - 2))
                    {
                        // В начале теста может засчитать обычную ошибку цвета в ошибку последовательности
                        if (inputs[i].well != 1)
                        {
                            sequenceError[stage]++;
                            sequenceErrors++;
                            flag = true;

                            //File.AppendAllText("redblacktable.log", "seq");
                        }
                    }

                    if (inputs[i].number * inputs[i].well < 0)
                    {
                        colorError[stage]++;
                        colorErrors++;

                        //File.AppendAllText("redblacktable.log", "col");
                    }

                    if (!flag && inputs[i].number != -inputs[i].well)
                    {
                        numberError[stage]++;
                        numberErrors++;

                        //File.AppendAllText("redblacktable.log", "num");
                    }

                    allErrors++;
                }
                //File.AppendAllText("redblacktable.log", "\r\n");

                if (inputs[i].well > 0 && inputs[i].number == inputs[i].well)
                {
                    if (inputs[i].well == 5)
                    {
                        times[stage] = inputs[i].time;
                        stage++;
                    }
                    else
                    if (inputs[i].well == 10)
                    {
                        times[stage] = inputs[i].time;
                        stage++;
                    }
                    else
                    if (inputs[i].well == 15)
                    {
                        times[stage] = inputs[i].time;
                        stage++;
                    }
                    else
                    if (inputs[i].well == 20)
                    {
                        times[stage] = inputs[i].time;
                        stage++;
                    }
                    else
                    if (inputs[i].well == 25)
                    {
                        times[stage] = inputs[i].time;
                        stage++;
                        endTest = inputs[i].time;
                    }
                }
            }

            if (endTest <= 0)
                throw new Exception("endTest <= 0");

            draw();
        }

        private void draw()
        {
            using (var g       = Graphics.FromImage(vw.vw))
            {
                drawGraphic(g);

                drawText(g);
            }

            vw.repaint();
        }

        // Приблизительное баллирование времени
        int[,] timeBulls = { {16, 117, 44, 22}, {29, 139, 44, 21}, {32, 196, 44, 23}, {28, 151, 45, 21}, {30, 122, 44, 20}};

        // Баллы: время, цвета, числа, порядок
        float[,] Bulls     = { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0} };

        // Приблизительные баллы
        // От точных отличаются
        // см. http://www.vashpsixolog.ru/psychodiagnostic-school-psychologist/61/480-test-black-and-red-table-gorbov-schulte-score-switching-attention
        float[,,] errorsBulls = { // цвета      // числа        // порядок
                                  {{2, 2},      {2, 4},         {4, 4}},
                                  {{1.5f, 1.5f},{1.5f, 1.5f},   {4.5f, 1.5f}},
                                  {{1, 1},      {1, 1},         {1, 3}},
                                  {{2, 3},      {1.5f, 1.5f},   {2, 1}},
                                  {{2, 3},      {1.5f, 1.5f},   {2, 1}}
                              };

        private void drawText(Graphics g)
        {
            var timeSec = new float[6];
            timeSec[0] = (float) (times[0] - inputs[0].time)    / 1000f / 10000f;
            timeSec[1] = (float) (times[1] - times[0])          / 1000f / 10000f;
            timeSec[2] = (float) (times[2] - times[1])          / 1000f / 10000f;
            timeSec[3] = (float) (times[3] - times[2])          / 1000f / 10000f;
            timeSec[4] = (float) (times[4] - times[3])          / 1000f / 10000f;
            timeSec[5] = (float) (times[4] - inputs[0].time)    / 1000f / 10000f;

            var stages = String.Format("Первый этап {0:f1} с, второй этап {1:f1} с, третий этап {2:f1} с, четвёртый этап {3:f1} с, пятый этап {4:f1} с; всего {5:f1} с", 
                timeSec[0],
                timeSec[1],
                timeSec[2],
                timeSec[3],
                timeSec[4],
                timeSec[5]
                );

            var errors = String.Format("Ошибки порядка, цвета, номера: первый этап {0} / {6} / {12}, второй этап {1} / {7} / {13}, третий этап {2} / {8} / {14}, четвёртый этап {3} / {9} / {15}, пятый этап {4} / {10} / {16}; всего {5} / {11} / {17} / {18}", 
                sequenceError[0],
                sequenceError[1],
                sequenceError[2],
                sequenceError[3],
                sequenceError[4],
                sequenceErrors,
                colorError   [0],
                colorError   [1],
                colorError   [2],
                colorError   [3],
                colorError   [4],
                colorErrors,
                numberError  [0],
                numberError  [1],
                numberError  [2],
                numberError  [3],
                numberError  [4],
                numberErrors,
                allErrors
                );

            var results = "Баллы (время, цвет, числа, порядок): ";
            float bulls = 0;
            for (int i = 0; i < 5; i++)
            {
                float curTime = timeSec[i];
                if (curTime - timeBulls[i, 0] <= 0)
                    Bulls[i, 0] =  timeBulls[0, 2];
                /*else
                if (curTime - timeBulls[i, 1] >= 0)
                    Bulls[i, 0] = timeBulls[0, 2] - (curTime - timeBulls[i, 0]) / 5f;//timeBulls[0, 3];*/
                else
                    Bulls[i, 0] = timeBulls[0, 2] - (curTime - timeBulls[i, 0]) * (float)  (timeBulls[0, 2] - timeBulls[0, 3]) / (float) (timeBulls[i, 1] - timeBulls[i, 0]);

                bulls += Bulls[i, 0];

                var k = 1;
                Bulls[i, k] = errorsBulls[i, k - 1, 1] * colorError[i];
                k = 2;
                Bulls[i, k] = errorsBulls[i, k - 1, 1] * numberError[i];
                k = 3;
                Bulls[i, k] = errorsBulls[i, k - 1, 1] * sequenceError[i];

                for (k = 1; k <= 3; k++)
                    if (Bulls[i, k] > 0)
                        Bulls[i, k] += errorsBulls[i, k - 1, 0];

                results += String.Format("{0:f1}/{1:f1}/{2:f1}/{3:f1}/{4:f1}. ", Bulls[i, 0], Bulls[i, 1], Bulls[i, 2], Bulls[i, 3], Bulls[i, 0] - Bulls[i, 1] - Bulls[i, 2] - Bulls[i, 3]);
                bulls += -Bulls[i, 1] - Bulls[i, 2] - Bulls[i, 3];
            }

            Bulls[5, 0] = bulls;

            var Эффективность = timeSec[5] / 5.0;
            var ЭффективностьСтрока = "Нет оценки";
            if (Эффективность <= 30f)
                ЭффективностьСтрока = "Отлично";
            else
            if (Эффективность <= 35f)
                ЭффективностьСтрока = "Хорошо";
            else
                if (Эффективность <= 45f)
                ЭффективностьСтрока = "Удовлетворительно";
            else
                if (Эффективность <= 55f)
                ЭффективностьСтрока = "Плохо";
            else
                ЭффективностьСтрока = "Очень плохо";


            var fnt    = new Font("Arial", 12);
            var fnt2   = new Font("Arial", 14, FontStyle.Bold);
            g.DrawString(stages, fnt, Black, 5, 5);
            var h = g.MeasureString(stages, fnt).Height;

            g.DrawString(errors, fnt, Black, 5, 5 + h);
            g.DrawString(results, fnt, Black, 5, 5 + h * 2);
            g.DrawString("Максимум задержки " + maxTime / 10000 + " мс, минимум задержки " + minTime / 10000 + " мс", fnt, Black, 5, 5 + h * 3);
            g.DrawString("Всего " + bulls + " из 221 возможных (менее 200 - очень плохо; менее 208 - плохо; более 216 - хорошо).", fnt2, Black, 15, 5 + 3 + h * 4);
            g.DrawString("Эффективность " + ЭффективностьСтрока + " (" + Эффективность.ToString("F1") + "). Врабатываемость (T1/T; выше 1,0 - плохо): " + (timeSec[0]/Эффективность).ToString("F2") + ". Устойчивость (T4/T; выше 1,0 - плохо): " + (timeSec[3]/Эффективность).ToString("F2"), fnt, Black, 5, 5 + h * 6);

            var dtt = DateTime.Now;
            File.AppendAllText(Application.StartupPath + "/redblacktable.txt", "" + dtt.Ticks + ":" + bulls + ":"  + timeSec[0] + ":"
                                                                                        + timeSec[1] + ":"
                                                                                        + timeSec[2] + ":"
                                                                                        + timeSec[3] + ":"
                                                                                        + timeSec[4] + ":"
                                                                                        + timeSec[5] + ":"
                                                                                        + sequenceErrors + ":"
                                                                                        + colorErrors + ":"
                                                                                        + numberErrors + ":"
                                                                                        + allErrors + "::" +
                                                                                        dtt.ToShortDateString() + " " + dtt.ToLongTimeString() + 
                                                                                        ":баллы:времяэтапов:общее время:ошибки порядка:цвета:номера:всего(не сумма)\r\n\r\n"
                                                                                        );
        }

        private void drawGraphic(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.WhiteSmoke), 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            var V0 = h0 * 1 / 3;
            var VMax = this.ClientSize.Height;
            var VRange = VMax - V0;
            var TMin = 0; //minTime;
            var TMax = maxTime;
            var TRange = TMax - TMin;
            var p = new Pen(Color.Black);
            var grPen = new SolidBrush(Color.Green);
            var red = new SolidBrush(Color.Red);
            var scale = this.ClientSize.Width / inputs.Count;
            var st = 3.0f;

            double V = 0;
            List<float> vs = new List<float>();
            vs.Add(1.0f);
            for (int i = 1; i < inputs.Count; i++)
            {
                var x1 = i * scale;
                var x0 = x1 - scale;

                var reactionTime = (inputs[i].time - inputs[i - 1].time);
                V = // (double) (TMax - reactionTime) / (double) TRange;
                    (double)reactionTime / TMax;

                V = VMax - V * VRange;

                // g.DrawLine(p, x0, (int) (VMax - oldV), x1, (int) (VMax - V));
                float y = (float)V;//(VMax - V);
                vs.Add(y);

                //g.FillRectangle(red, x1, 0, scale, VRange);

                if (inputs[i].number != inputs[i].well)
                    g.FillEllipse(red, x1 - st, y - st, st * 2, st * 2);
                else
                    g.FillEllipse(grPen, x1 - st, y - st, st * 2, st * 2);
            }

            cb = null;
            float oldY = VMax;
            for (int x = 1; x < this.ClientSize.Width; x++)
            {
                //float y = interpol(x, vs, scale, 5);
                float y = spline2(x, vs, scale);
                if (y < 0)
                    y = 0;
                if (y > this.ClientSize.Height)
                    y = this.ClientSize.Height;

                g.DrawLine(p, x - 1, oldY, x, y);
                oldY = y;
            }
        }

        CubicSpline cb = null;
        private float spline2(float x, List<float> vs, int scale)
        {
            if (cb == null)
            {
                cb = new CubicSpline();
                var xs = new double[vs.Count];
                var ys = new double[vs.Count];
                for (int i = 0; i < vs.Count; i++)
                {
                    xs[i] = i*scale;
                    ys[i] = vs[i];
                }

                cb.BuildSpline(xs, ys, xs.Length);
            }

            return (float) cb.Func(x);
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RedBlack_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            opened = null;
        }

        private void RedBlack_KeyDown(object sender, KeyEventArgs e)
        {
            if (red > 25 && e.KeyCode == Keys.Escape)
                if (MessageBox.Show("Хотите закрыть окно?", "Результаты теста", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    Close();
        }
    }
}
