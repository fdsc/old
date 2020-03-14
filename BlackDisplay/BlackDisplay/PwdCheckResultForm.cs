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
    public partial class PwdCheckResultForm : Form
    {
        public PwdCheckResultForm()
        {
            InitializeComponent();
        }

        public PwdCheckResultForm(keccak.SHA3.PwdCheckResult checkResult): this()
        {
            pwdStrengthBar.Value = (int) ( pwdStrengthBar.Maximum * checkResult.pwdStrength );


            double Ayears = checkResult.maxPwdStrengthInTimes / checkResult.maxAbsoluteCount;
            long years   = (long) ( checkResult.maxPwdStrengthInTimes / checkResult.AbsoluteYearCount );
            float yearsF = (float) ( checkResult.maxPwdStrengthInTimes / checkResult.AbsoluteYearCount );
            long Yyears  = (long) ( checkResult.maxPwdStrengthInTimes / checkResult.minAbsoluteCount );
            long myears  = (long) ( checkResult.maxPwdStrengthInTimes / checkResult.minAbsoluteCount* 30.0 );
            long mdays   = (long) ( checkResult.maxPwdStrengthInTimes / checkResult.minAbsoluteCount * 30.0 * 365.0 );

            if (Ayears >= 1)
                textResultBox.Text += "Сильный пароль (8 из 8 баллов)\r\n\r\n";
            else
            if (years > 32)
                textResultBox.Text += "Хороший пароль (7 из 8 баллов)\r\n\r\n";
            else
            if (years > 8)
                textResultBox.Text += "Неплохой пароль (6 из 8 баллов)\r\n\r\n";
            else
            if (years > 1)
                textResultBox.Text += "Удовлетворительный пароль (5 из 8 баллов)\r\n\r\n";
            else
            if (myears > 1)
                textResultBox.Text += "Допустимый пароль (4 из 8 баллов)\r\n\r\n";
            else
            if (Yyears > 3)
                textResultBox.Text += "Допустимый временный пароль (3 из 8 баллов)\r\n\r\n";
            else
            if (mdays >= 30)
                textResultBox.Text += "Слабый временный пароль (2 из 8 баллов)\r\n\r\n";
            else
                textResultBox.Text += "Это очень слабый пароль (использование недопустимо)\r\n\r\n";


            if (!checkResult.isDigit && !checkResult.isOther)
                textResultBox.Text += "Добавьте в пароль цифры или небуквенные символы (!@#$%^&*()_+|\\=-/'.;:<>{}[]`~)\r\n";

            if (checkResult.doublesCount > 1)
                textResultBox.Text += "Удалите из пароля повторы символов\r\n";

            if (checkResult.MultCount > 0)
                textResultBox.Text += "Удалите из пароля множественные повторы символов\r\n";

            textResultBox.Text += "\r\nНа взлом этого пароля уйдёт приблизительно: \r\n";
            //textResultBox.Text +=  Ayears.ToString("F0") + " лет, если его будут взламывать через 100 лет, но кто знает, что будет...\r\n";
            textResultBox.Text += checkResult.maxPwdStrengthInLogDateK.ToString("F0") + " лет по логарифмической оценке в самом плохом случае с коэффициентом запаса\r\n";
            textResultBox.Text += checkResult.maxPwdStrengthInLogDate .ToString("F0") + " лет по логарифмической оценке в самом плохом случае\r\n";
            textResultBox.Text += checkResult.minPwdStrengthInLogDate .ToString("F0") + " лет по логарифмической оценке в случае спец. защиты от перебора более 100 п/с\r\n";

            if (Ayears < 1000000000)
            {
                if (yearsF < 2)
                {
                    if (yearsF*12 < 3)
                        textResultBox.Text += ((long)(yearsF*12*30)).ToString("N0") + " дней, если его начнут взламывать прямо сейчас организованной группой лиц с кластером специализированных средств\r\n";
                    else
                        textResultBox.Text += ((long)(yearsF*12)).ToString("N0") + " месяцев, если его начнут взламывать прямо сейчас организованной группой лиц с кластером специализированных средств\r\n";
                }
                else
                    textResultBox.Text += years.ToString("N0") + " лет, если его начнут взламывать прямо сейчас организованной группой лиц с кластером специализированных средств\r\n";
            }

            if (Ayears < 1000000000)
                if (Yyears > 35)
                    textResultBox.Text += Yyears.ToString("N0") + " лет, если его начнут взламывать с помощью средств повышенной мощности\r\n";
                else
                {
                    if (Yyears > 1)
                        textResultBox.Text += myears.ToString("N0") + " месяцев, если его начнут взламывать с помощью средств повышенной мощности\r\n";
                    else
                        textResultBox.Text += mdays.ToString("N0") + " дней, если его начнут взламывать с помощью средств повышенной мощности\r\n";

                    double SDays = checkResult.maxPwdStrengthInTimes / checkResult.minAbsoluteYearCount;
                    if (SDays >= 1.0)
                        textResultBox.Text += SDays.ToString("F0") + " лет в худшем для злоумышленников случае (низкопроизводительный хэш, затрудняющий подбор)\r\n";
                    else
                        textResultBox.Text += (SDays * 365).ToString("F0") + " дней в худшем для злоумышленников случае (низкопроизводительный хэш, затрудняющий подбор)\r\n";
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PwdCheckResultForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
        }
    }
}
