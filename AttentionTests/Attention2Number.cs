using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AttentionTests
{
    //public class SelectableLabel: 

    public partial class Attention2Number : Form
    {
        public Attention2Number()
        {
            InitializeComponent();
            maxCount = 51;
        }

        private void Attention2Number_Shown(object sender, EventArgs e)
        {
            if (MessageBox.Show("Экспресс-тест \"Два числа\"."
                + "\r\nИзмеряется способность выполнять монотонную простую задачу."
                + "\r\nОбщая задача теста: ввести все числа без ошибок и задержек. Скорость прохождения теста не оценивается, важно отсутствие задержек (пауз) при вводе."
                + "\r\nВ случае если тест проходится впервые, рекомендуется пройти тест для ознакомления не менее двух раз."
                + "\r\nТест расчитан на использование цифрового блока клавиатуры (справа клавиатуры) вслепую, то есть нажатие клавиш должно происходить без отрыва внимания от экрана (если вы плохо владеете цифровой клавиатурой, потренируйтесь, установив флажок \"Вводить что видишь\")."
                + "\r\nТест \"Три числа\" менее чувствителен к владению цифровой клавиатурой, но требует больше усилий для прохождения и более сильно зависит от индивидуальных особенностей."
                + "\r\n1. Каждый раунд вам отображается некоторая цифра, которую вы запоминаете."
                + "\r\n2. Задача раунда - ввести цифру, запомненную на предыдущем раунде."
                + "\r\n3. При вводе цифры автоматически начинается следующий раунд (отображается новая цифра)."
                + "\r\n4. Тест начинается с запоминания отображающегося числа и нажатия клавиши '0'. В варианте \"Три числа\" необходимо запомнить сразу две цифры."
                + "\r\n5. Если цифра раунда отображается белым на чёрном фоне - перед вводом этого числа вы должны нажать клавишу Enter."
                + "\r\nВариант с тремя числами требует большего напряжения внимания испытуемого. Задача раунда - ввести цифру, отображённую не раунд назад, а два раунда назад"
                + "\r\nПример для варианта \"Два числа\"."
                + "\r\nНа экране последовательно отображаются цифры: 3 5 8 1 0 7;"
                + "\r\nвы вводите цифры - 0 3 5 8 1 0 7"
                + "\r\nПример для варианта \"Два числа\"."
                + "\r\nНа экране последовательно отображаются цифры: 3 5 (белое 8) 1 0 7;"
                + "\r\nвы вводите - 0 3 5 Enter 8 1 0 7"
                + "\r\nПример для варианта \"Три числа\"."
                + "\r\nНа экране последовательно отображаются цифры: 87 5 8 1 0 7;"
                + "\r\nвы вводите цифры - 0 8 7 5 8 1 0 7",
                "Экспресс-тест \"Два числа\"", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != System.Windows.Forms.DialogResult.OK)
            {
                Close();
                return;
            }

            //main.Text = "" + testNumbers[shifter];
            setMainText(testNumbers[shifter]);
        }

        private void setMainText(int val, int val0 = int.MinValue)
        {
            if (val < 0 && val0 == int.MinValue)
            {
                main.Text = "" + (-val);
                main.ForeColor = Color.White;
                main.BackColor = Color.Black;
            }
            else
            {
                if (val0 == int.MinValue)
                    main.Text = "" + val;
                else
                    main.Text = "" + val0 + "" + val;

                main.ForeColor = Color.Black;
                main.BackColor = Color.FromName("Control");
            }
        }

        private void Attention2Number_Load(object sender, EventArgs e)
        {
            generateRandom();
        }

        private void generateRandom()
        {
            var rnd = new Random();
            testNumbers[0] = 0;
            for (int i = 1; i < testNumbers.Length; i++)
            {
                testNumbers[i] = rnd.Next(0, 10);
                if (shifter < 2 && rnd.Next(0, 15) == 0)
                    testNumbers[i] *= -1;
            }

            progressBar1.Maximum = maxCount;
        }

        public struct input
        {
            public input(int inputNumber, bool lastIsEnter)
            {
                number = inputNumber;
                time   = DateTime.Now.Ticks;
                this.lastEnter = lastIsEnter;
            }

            public readonly long time;
            public readonly int  number;
            public readonly bool lastEnter;
        }

        public bool keyUpped = true;

        public static readonly int _maxCount = /*51*/ 251;
        public static int maxCount = 51;    // 251
        public int[]   testNumbers  = new int  [_maxCount];
        public input[] inputNumbers = new input[_maxCount];
        public int counter = 0;
        public bool lastIsEnter = false;
        private void Attention2Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (counter >= maxCount)
            {
                helper.Text = GetEndHelperText();
                return;
            }

            if (!keyUpped)      // Иначе одно долгое нажатие может засчитаться за два
                return;

            // Этот код для того, чтобы не было звуков при нажатии (а то иначе звучит "колокольчик", как будто осуществляется ввод в неактивную форму)
            e.Handled = true;e.SuppressKeyPress = true;

            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            checkBox3.Enabled = false;

            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    inputNumbers[counter] = new input(0, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    inputNumbers[counter] = new input(1, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    inputNumbers[counter] = new input(2, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    inputNumbers[counter] = new input(3, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    inputNumbers[counter] = new input(4, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    inputNumbers[counter] = new input(5, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    inputNumbers[counter] = new input(6, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    inputNumbers[counter] = new input(7, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    inputNumbers[counter] = new input(8, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    inputNumbers[counter] = new input(9, lastIsEnter);
                    lastIsEnter = false;
                    break;
                case Keys.Enter:
                    inputNumbers[counter] = new input(int.MinValue, false);
                    lastIsEnter = true;
                    break;
                default:
                    /*inputNumbers[counter] = new input(-1);
                    break;*/
                    return;
            }

            keyUpped = false;

            if (testNumbers[counter] < 0)
            {
                if (inputNumbers[counter].number == int.MinValue)
                {
                    helper.Text = "верно";
                }
                else
                {
                    helper.Text = "неверно (надо было нажать Enter)";
                }

                testNumbers[counter] *= -1;
                return;
            }
            else
            {
                if (inputNumbers[counter].number == testNumbers[counter])
                {
                    helper.Text = "верно";
                }
                else
                {
                    helper.Text = "неверно";
                }
            }

            if (NumPadTest != 0)
            {
                if (counter + shifter + 1 < maxCount)
                {
                    var val = testNumbers[counter + shifter + 1];
                    setMainText(val);
                }
                else
                    main.Text = "";
            }
            else
                main.Text = "";

            counter++;
        }

        private void Attention2Number_KeyUp(object sender, KeyEventArgs e)
        {
            // Этот код для того, чтобы не было звуков при нажатии (а то иначе звучит "колокольчик", как будто осуществляется ввод в неактивную форму)
            e.Handled = true;e.SuppressKeyPress = true;
            keyUpped = true;

            if (counter >= maxCount)
            {
                helper.Text = GetEndHelperText();
                this.TopMost = false;
                return;
            }

            progressBar1.Value = counter;

            if (counter + shifter < maxCount)
            {
                if (NumPadTest == 0)
                {
                    //main.Text = "" + testNumbers[counter + shifter];
                    var val = testNumbers[counter + shifter];
                    setMainText(val);
                }
                /*else
                {
                    main.Text = "" + testNumbers[counter + shifter];// + ((counter + shifter + 1 < maxCount) ? "" + testNumbers[counter + shifter + 1] : "");
                }*/
            }
            else
            {
                progressBar1.Value = progressBar1.Maximum;
                main.Text = "X";

                main.ForeColor = Color.Black;
                main.BackColor = Color.FromName("Control");
            }
        }

        private string GetEndHelperText()
        {
            var a = GetErrorsCount();
            var b = GetTestTime();
            var c = GetTimeSigma();
            int e1, e2;
            var d = getGrade(out e1, out e2);
            return String.Format("Тест закончен с оценкой \"{3}\".\r\nОшибок {0},\r\nВыходов за 1,5σ {6},\r\nВыходов за 2σ на белых числах {7}\r\nсреднее время {1} мс,\r\nсреднеквадратическое отклонение {2} мс,\r\nмаксимум{4},\r\nминимум {5}", a, b, c, d, maxTime, minTime, e1, e2);
        }

        public int result_errorCount = 0;
        private int GetErrorsCount()
        {
            int counter = 0;
            for (int i = 0; i < maxCount; i++)
            {
                if (testNumbers[i] != inputNumbers[i].number)
                    counter++;
            }

            result_errorCount = counter;
            return counter;
        }

        public long result_time = 0;
        private long GetTestTime()
        {
            result_time = (inputNumbers[maxCount - 1].time - inputNumbers[0].time) / (10000) / (maxCount - 1);
            return result_time;
        }

        public int result_sigma = 0;

        public double maxTime;
        public double minTime;
        public double avgTime;
        private int GetTimeSigma()
        {
            long   testTime = (inputNumbers[maxCount - 1].time - inputNumbers[0].time) / 10000;   // в миллисекундах
                   avgTime  = testTime / (maxCount - 1);
            double sigma    = 0;

            maxTime  = 0;
            minTime  = testTime;
            for (int i = 1; i < maxCount; i++)
            {
                double time = (inputNumbers[i].time - inputNumbers[i-1].time) / 10000.0;
                double s    = time - avgTime;
                sigma      += Math.Pow(s, 2);

                if (maxTime < time)
                    maxTime = time;
                if (minTime > time)
                    minTime = time;
            }

            sigma /= maxCount;
            sigma = (long) Math.Sqrt(sigma);

            result_sigma = (int) sigma;
            return result_sigma;
        }

        public string result_grade;
        public string[] grades = {"хуже некуда", "очень плохо", "плохо", "удовлетворительно", "хорошо", "отлично"};
        public string getGrade(out int e1, out int e2)
        {
            int s = 0;
            e1 = 0;
            e2 = 0;
            /*
            if (result_time > 2100)
                s = -1;
            if (result_time > 1700)
                s = 0;
            if (result_time > 1350)
                s = 1;
            else
            if (result_time > 1000)
                s = 2;
            else
            if (result_time > 773)
                s = 3;
            else
            if (result_time > 550)
                s = 4;
            else
                s = 5;*/
                /*
            if (result_sigma > 300 && result_sigma * 3 / (maxTime - avgTime) >= 1)
                s -= (int) (result_sigma * 3 / (maxTime - avgTime));

            if (maxTime > 1500 + maxCount)
                s -= (int) (maxTime / (1500 + maxCount));
                
            if (maxCount == _maxCount)
                s += 2; // превышение максимума на 1 и снижение скорости или одна ошибка (реально пройти только с превышением максимума)

            if (shifter == 2)
            {
                s += 7; // превышение максимума на 1, 5 ошибок, превышение среднего на 1
                if (maxCount == _maxCount)
                    s += 26; // 5*5 ошибок, дополнительное снижение скорости плюс к учтённому в "s+= 2"
            }
            
            if (maxCount <= 51)
                s -= result_errorCount*2 ;   // делаем -3 даже при одной ошибке

            s -= result_errorCount;
            if (s < 0)
                s = 0;
            if (s > 5)
                s = 5;
                */
            graphButton.Visible = true;
            var rs = result_sigma;
            if (result_sigma < 150)
                rs = 150;

            var sigma15 = (rs*1.5 + this.result_time)*10000;
            var sigma20 = (rs*2.0 + this.result_time)*10000;
            var sigma30 = (rs*3.0 + this.result_time)*10000;
            var sigma40 = (rs*4.0 + this.result_time)*10000;
            for (int i = 1; i < maxCount; i++)
            {
                var reactionTime = inputNumbers[i].time - inputNumbers[i - 1].time;
                if (inputNumbers[i].lastEnter)
                    reactionTime /= 2;

                if (inputNumbers[i].lastEnter)
                {
                    if (reactionTime > sigma40)
                    {
                        s += 2;
                        e2++;
                    }
                    else
                    if (reactionTime > sigma20)
                    {
                        s += 1;
                        e2++;
                    }
                }
                else
                {
                    if (shifter < 2)
                    {
                        if (reactionTime > sigma30)
                        {
                            s += 3;
                            e1++;
                        }
                        else
                        if (reactionTime > sigma15)
                        {
                            s += 2;
                            e1++;
                        }
                    }
                    else
                    {
                        if (reactionTime > sigma40)
                        {
                            s += 3;
                            e1++;
                        }
                        else
                        if (reactionTime > sigma20)
                        {
                            s += 2;
                            e1++;
                        }
                    }
                }
            }

            s += result_errorCount * 7;

            float k = 1f;
            if (shifter == 2)
                k = 1.5f;

            if (maxCount <= 51)
            {
                if (s <= 2*k)
                {
                    s = 5;
                }
                else
                if (s <= 5*k)
                {
                    s = 4;
                }
                else
                if (s <= 8*k)
                {
                    s = 3;
                }
                else
                if (s <= 11*k)
                {
                    s = 2;
                }
                else
                if (s <= 17*k)
                {
                    s = 1;
                }
                else
                    s = 0;
            }
            else
            {
                if (s <= 10)
                {
                    s = 5;
                }
                else
                if (s <= 25)
                {
                    s = 4;
                }
                else
                if (s <= 40)
                {
                    s = 3;
                }
                else
                if (s <= 55)
                {
                    s = 2;
                }
                else
                if (s <= 85)
                {
                    s = 1;
                }
                else
                    s = 0;
            }


            result_grade = grades[s];
            return result_grade;
        }

        int NumPadTest = 0;
        public int shifter = 1;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                NumPadTest = 1;
                shifter = 0;
                generateRandom();
                setMainText(testNumbers[shifter]);
            }
            else
            {
                NumPadTest = 0;
                shifter = 1;
                generateRandom();
                setMainText(testNumbers[shifter]);
            }

            this.Focus();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                maxCount = 251;
                progressBar1.Maximum = maxCount;
            }
            else
            {
                maxCount = 51;
                progressBar1.Maximum = maxCount;
            }
        }

        private void graphButton_Click(object sender, EventArgs e)
        {
            new Attention2Number_Velocity(this).Show();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                checkBox1.Checked = false;

            generateRandom();
            if (checkBox3.Checked)
            {
                shifter = 2;
                generateRandom();
                setMainText(testNumbers[shifter], testNumbers[shifter - 1]);
            }
            else
            {
                shifter = 1;
                generateRandom();
                setMainText(testNumbers[shifter]);
            }

            this.Focus();
        }

        private void Attention2Number_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void main_Click(object sender, EventArgs e)
        {

        }
    }
}
