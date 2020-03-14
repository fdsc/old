using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VinPlaning;

namespace BlackDisplay
{
    public partial class OptionForm : Form
    {
        public OptionForm()
        {
            InitializeComponent();
        }

        public static OptionForm self = null;
        public static Form1 mainForm = null;
        public int saved = 0;

        internal static void showForm(Form1 form1, Form1.RtdbOptions opts = null)
        {
            if (opts == null)
                opts = Form1.opts;

            if (self != null && self.IsDisposed)
                self = null;

            if (self != null)
            {
                self.showOptions(opts);
                self.Focus();
                self.BringToFront();
                return;
            }

            mainForm = form1;

            self = new OptionForm();
            self.showOptions(opts);
            self.Show();
        }

        private void OptionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isOptionsChanged())
            {
                if (MessageBox.Show("Вы уверены, что хотите выйти без сохранения настроек?", "Выход без сохранения", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Form1.saveOptionsToFile();
            mainForm.refreshOptions();

            e.Cancel    = false;
            self        = null;
            mainForm    = null;
        }

        private bool isOptionsChanged()
        {
            if (numericUpDown1 .Value    != Form1.opts[Form1.optsName[ 0], 0])
                return true;
            if (numericUpDown2 .Value    != Form1.opts[Form1.optsName[ 1], 0])
                return true;
            if (numericUpDown3 .Value    != Form1.opts[Form1.optsName[ 2], 0])
                return true;
            if (numericUpDown4 .Value    != Form1.opts[Form1.optsName[10], 0])
                return true;
            if (numericUpDown5 .Value    != Form1.opts[Form1.optsName[19], 0])
                return true;
            if (numericUpDown6 .Value    != Form1.opts[Form1.optsName[13], 0])
                return true;
            if (numericUpDown7 .Value    != Form1.opts[Form1.optsName[12], 0])
                return true;
            if (numericUpDown8 .Value    != Form1.opts[Form1.optsName[20], 0])
                return true;
            if (numericUpDown9 .Value    != Form1.opts[Form1.optsName[24], 0])
                return true;
            if (numericUpDown10.Value    != Form1.opts[Form1.optsName[23], 0])
                return true;
            if (numericUpDown11.Value    != Form1.opts[Form1.optsName[26], 0])
                return true;
            if (numericUpDown12.Value    != Form1.opts[Form1.optsName[28], 0])
                return true;
            if (numericUpDown13.Value    != Form1.opts[Form1.optsName[30], 0])
                return true;
            if (numericUpDown14.Value    != Form1.opts[Form1.optsName[31], 0])
                return true;
            if (numericUpDown15.Value    != Form1.opts[Form1.optsName[33], 0])
                return true;
            if (textBox1       .Text     != Form1.opts[Form1.optsName[ 5], ""])
                return true;
            if (checkBox2      .Checked  != Form1.opts[Form1.optsName[ 4], true])
                return true;
            if (checkBox1      .Checked  != Form1.opts[Form1.optsName[ 3], true])
                return true;
            if (checkBox3      .Checked  != Form1.opts[Form1.optsName[ 8], true])
                return true;
            if (checkBox4      .Checked  != Form1.opts[Form1.optsName[14], true])
                return true;
            if (checkBox5      .Checked  != Form1.opts[Form1.optsName[15], true])
                return true;
            if (checkBox6      .Checked  != Form1.opts[Form1.optsName[16], true])
                return true;
            if (checkBox7      .Checked  != Form1.opts[Form1.optsName[18], true])
                return true;
            if (checkBox8      .Checked  != Form1.opts[Form1.optsName[22], true])
                return true;
            if (checkBox9      .Checked  != Form1.opts[Form1.optsName[25], true])
                return true;
            if (checkBox10     .Checked  != Form1.opts[Form1.optsName[27], true])
                return true;
            if (checkBox34     .Checked  != Form1.opts[Form1.optsName[34], true])
                return true;

            if (comboBox1.SelectedIndex != Form1.opts[Form1.optsName[21], 0] - 1)
                return true;

            if (comboBox2.SelectedIndex != Form1.opts[Form1.optsName[29], 0])
                return true;

            if (comboBox3.SelectedIndex != Form1.opts[Form1.optsName[32], 0])
                return true;

            return false;
        }

        public void showOptions(Form1.RtdbOptions opts = null)
        {
            // Form1.opts так же используется для обновления настроек при сохранении опций, т.к. переданный параметр не сохраняется
            if (opts == null)
                opts = Form1.opts;

            mainForm.refreshOptions(true);

            // При добавлении новых полей изменить так же isOptionsChanged и putOptions
            numericUpDown1 .Value   = opts[Form1.optsName[ 0], 0];
            numericUpDown2 .Value   = opts[Form1.optsName[ 1], 0];
            numericUpDown3 .Value   = opts[Form1.optsName[ 2], 0];
            numericUpDown4 .Value   = opts[Form1.optsName[10], 0];
            numericUpDown5 .Value   = opts[Form1.optsName[19], 0];
            numericUpDown6 .Value   = opts[Form1.optsName[13], 0];
            numericUpDown7 .Value   = opts[Form1.optsName[12], 0];
            numericUpDown8 .Value   = opts[Form1.optsName[20], 0];
            numericUpDown9 .Value   = opts[Form1.optsName[24], 0];
            numericUpDown10.Value   = opts[Form1.optsName[23], 0];
            numericUpDown11.Value   = opts[Form1.optsName[26], 0];
            numericUpDown12.Value   = opts[Form1.optsName[28], 0];
            numericUpDown13.Value   = opts[Form1.optsName[30], 0];
            numericUpDown14.Value   = opts[Form1.optsName[31], 0];
            numericUpDown15.Value   = opts[Form1.optsName[33], 0];
            textBox1      .Text     = opts[Form1.optsName[ 5], ""];
            checkBox2     .Checked  = opts[Form1.optsName[ 4], true];
            checkBox1     .Checked  = opts[Form1.optsName[ 3], true];
            checkBox3     .Checked  = opts[Form1.optsName[ 8], true];
            checkBox4     .Checked  = opts[Form1.optsName[14], true];
            checkBox5     .Checked  = opts[Form1.optsName[15], true];
            checkBox6     .Checked  = opts[Form1.optsName[16], true];
            checkBox7     .Checked  = opts[Form1.optsName[18], true];
            checkBox8     .Checked  = opts[Form1.optsName[22], true];
            checkBox9     .Checked  = opts[Form1.optsName[25], true];
            checkBox10    .Checked  = opts[Form1.optsName[27], true];
            checkBox34    .Checked  = opts[Form1.optsName[34], true];

            comboBox1.SelectedIndex = Form1.opts[Form1.optsName[21], 0] - 1;
            comboBox2.SelectedIndex = Form1.opts[Form1.optsName[29], 0];
            comboBox3.SelectedIndex = Form1.opts[Form1.optsName[32], 0];

            saved = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите сбросить настройки в стандартные?\r\nОтменить операцию будет невозможно!", "Сброс настроек", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                return;

            mainForm.resetOptionsToDefault();
            showOptions(Form1.opts);
            saved = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveOptions();
        }

        private void SaveOptions()
        {
            putOptions();
            saveOptionsToFile();
        }

        private void saveOptionsToFile()
        {
            Form1.saveOptionsToFile();
            mainForm.refreshOptions();
            showOptions(Form1.opts);

            saved = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cancelNewOptions();
        }

        private void cancelNewOptions()
        {
            mainForm.refreshOptions();
            showOptions(Form1.opts);
            saved = 0;
        }

        private void putOptions()
        {
            try
            {
                Form1.opts.saveExecute = false;

                Form1.opts.add(Form1.optsName[ 0], (int) numericUpDown1 .Value);
                Form1.opts.add(Form1.optsName[ 1], (int) numericUpDown2 .Value);
                Form1.opts.add(Form1.optsName[ 2], (int) numericUpDown3 .Value);
                Form1.opts.add(Form1.optsName[10], (int) numericUpDown4 .Value);
                Form1.opts.add(Form1.optsName[19], (int) numericUpDown5 .Value);
                Form1.opts.add(Form1.optsName[13], (int) numericUpDown6 .Value);
                Form1.opts.add(Form1.optsName[12], (int) numericUpDown7 .Value);
                Form1.opts.add(Form1.optsName[20], (int) numericUpDown8 .Value);
                Form1.opts.add(Form1.optsName[24], (int) numericUpDown9 .Value);
                Form1.opts.add(Form1.optsName[23], (int) numericUpDown10.Value);
                Form1.opts.add(Form1.optsName[26], (int) numericUpDown11.Value);
                Form1.opts.add(Form1.optsName[28], (int) numericUpDown12.Value);
                Form1.opts.add(Form1.optsName[30], (int) numericUpDown13.Value);
                Form1.opts.add(Form1.optsName[31], (int) numericUpDown14.Value);
                Form1.opts.add(Form1.optsName[33], (int) numericUpDown15.Value);
                Form1.opts.add(Form1.optsName[ 5], textBox1.Text);
                Form1.opts.add(Form1.optsName[ 4], checkBox2 .Checked);
                Form1.opts.add(Form1.optsName[ 3], checkBox1 .Checked);
                Form1.opts.add(Form1.optsName[ 8], checkBox3 .Checked);
                Form1.opts.add(Form1.optsName[14], checkBox4 .Checked);
                Form1.opts.add(Form1.optsName[15], checkBox5 .Checked);
                Form1.opts.add(Form1.optsName[16], checkBox6 .Checked);
                Form1.opts.add(Form1.optsName[18], checkBox7 .Checked);
                Form1.opts.add(Form1.optsName[22], checkBox8 .Checked);
                Form1.opts.add(Form1.optsName[25], checkBox9 .Checked);
                Form1.opts.add(Form1.optsName[27], checkBox10.Checked);
                Form1.opts.add(Form1.optsName[34], checkBox34.Checked);

                Form1.opts.add(Form1.optsName[21], comboBox1.SelectedIndex + 1);
                Form1.opts.add(Form1.optsName[29], comboBox2.SelectedIndex);
                Form1.opts.add(Form1.optsName[32], comboBox3.SelectedIndex);
            }
            finally
            {
                Form1.opts.saveExecute = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!isOptionsChanged())
            {
                Close();
                return;
            }

            var dlg = MessageBox.Show("Хотите сохранить настройки (\"да\") либо выйти без сохранения (\"Нет\")?", "Выход из формы настроек", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dlg == System.Windows.Forms.DialogResult.Yes)
            {
                SaveOptions();
                Close();
            }
            else
            if (dlg == System.Windows.Forms.DialogResult.No)
            {
                cancelNewOptions();
                Close();
            }
        }

        private void OptionForm_Shown(object sender, EventArgs e)
        {
            this.Text = "Настройки       (" + AppDomain.CurrentDomain.BaseDirectory + ")";

            getTreeOptions();
        }
        

        private void getTreeOptions()
        {
            treeTableView1.Location = new Point(0, 0);
            treeTableView1.Size     = tabPage3.ClientSize;

            treeTableView1.getNodeRequest = new TreeTableView.getNode
            (
                delegate(byte[] nodeId, out Node node)
                {
                    node = new Node("Другие настройки");
                }
            );

            int j = 0;
            treeTableView1.getNodeLinkRequest = new TreeTableView.getLinks
            (
                delegate(Node node)
                {
                    var result = new List<Node>();
                    if (j < 10)
                    {
                        result.Add(new Node("" + j++));
                        result.Add(new Node("" + j++));
                    }
                    return result;
                }
            );
        }


        private void numericUpDown1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт минимальное время бездействия, после которого предыдущая история отдыха и работы не учитывается\r\nОт 40 до 7200\r\nВводите сюда, например, длительность выходных (60-64 часа) или отдыха между рабочими днями (14-15 часов)\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown2_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт время отдыха, приходящееся на один час.\r\nНапример, если вы хотите отдыхать каждый два часа по 20 минут, значит время отдыха на один час - 10 минут\r\nОт 0 до 59\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown3_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт интервал времени, содержащий полный цикл работы и отдыха\r\nНапример, если вы отдыхаете каждые два часа по 20 минут, то время работы в цикле - 1 час 40 минут, время отдыха - 20 минут, рабочий интервал - 120 минут (необходимо ввести значение 120)\r\nОт 15 до 120\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown5_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт время (в 1/10 минуты) бездействия пользователя, после которого программа выдаст запрос на зачернение монитора (выведение чёрного окна-заставки)\r\nОпция работает в програме, включённой в быстром режиме\r\nОпция аналогична опции операционной системы, но может использоваться как альтернативаня. Например, вы можете задать время бездействия 3 минуты, если вы обычно активно работаете, а когда читаете - перейти в медленный режим работы программы и программа не будет зачернять монитор.\r\nЗадайте большое значение (например, 1200), если не хотите использовать возможность зачернения\r\nВвод осуществляется в 1/10 минуты", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown4_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт минимальное время, после которого снова будет выведено напоминание о необходимости отдыха после нажатия кнопки \"Ни за что\" (отказа от отдыха) или после отдыха. Не влияет на другие интервалы вывода\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown6_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Программа имеет два режима учёта времени: быстрый и медленный. В быстром режиме программа может выдать запрос на зачернение экрана, если вы бездействуете; в медленном такой запрос не производится.\r\nДанная опция используется для автоматического переключения из медленного режима в быстрый.\r\nЕсли на протяжении времени, указанного в опции \"Длительность быстрого режима\" не было периода бездействия более заданного интервала, то произойдёт автоматическое переключение в быстрый режим.\r\n\r\nВвод осуществляется в секундах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown7_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Программа имеет два режима учёта времени: быстрый и медленный. В быстром режиме программа может выдать запрос на зачернение экрана, если вы бездействуете; в медленном такой запрос не производится.\r\n\r\nДанная опция используется для автоматического переключения из медленного режима в быстрый.\r\nЕсли на протяжении заданного интервала не было периода бездействия более интервала \"Интервал быстрого режима\", то произойдёт автоматическое переключение в быстрый режим.\r\n\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void textBox1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт путь к программе обновления\r\nОставьте пустым, если не хотите запускать программу обновления", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox2_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Если отмечено, программа запускается при старте операционной системы автоматически.\r\nДля нормальной работы программы поле должно быть отмечено.", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("При двойном клике на значок программы в панели задач либо при истечении времени затемнения программа выводит чёрное окно, закрывающее экран.\r\nЭто окно скрывается по нажатию клавиш Esc, alt, shift, crtl\r\nПри этом, если данная опция не отмечена, окно просто скроется.\r\nЕсли данная опция отмечена, компьютер будет заблокирован средствами операционной системы и она предложит ввести пароль на вход в операционную систему.\r\nДанная опция рекомендуется для использования на компьютерах, защищённых паролем на вход в операционную систему, напрмер, для офисных компьютеров", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox3_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Если отмечено, в лог-файле с именем формата logs\\имяПользователя-S-1-5-21-2080804779-984697084-1343764950-1000_wnd.log будет сохраняться заголовок текущего активного окна\r\nОцпия предназначена для использования с возможностями, ещё не заложенными в программу", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox4_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("В некоторых случаях фоновые процессы постоянно создают новые окна и процессы. Программа будет игнорировать появление этих окон (снова выводить чёрный экран поверх них).\r\nОпция игнорируется, если поставленна опция блокировки компьютера (компьютер просто блокируется, если вдруг поверх программы вывелось окно)\r\nЕсли чёрный экран не реагирует на нажатие Esc, ctrl или alt, кликните понему, чтобы окно получило фокус, а затем нажмите любую из этих клавиш", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox5_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Полноэкранные приложения часто являются играми или проигрывателями фильмов. Если флажок установлен, программа не будет отвлекать вас от работы с полноэкранным приложением и окнами \"top most\" (всегда наверху). Кроме этого, некоторые полноэкранные 3D-игры не позволяют правильно переключаться на другие окна.\r\n\r\nВы можете снять этот флажок, если перед каждым входом в игру, от которой вы не хотите отвлекаться, или перед просмотром фильма будете вручную (через контекстное меню программы) переходить в режим работы \"Не беспокоить\" либо \"Смотрю фильм\" (при просмотре фильма)", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox6_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("В некоторых обновлениях в программу включаются новые настройки, функции. Поставьте флажок, если хотите, чтобы программа уведомляла о доступности новой функциональности", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        
        private void checkBox7_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Если включено, в диалоговом окне \"Пора отдыхать\" нельзя будет нажать кнопку \"Ни за что!\"", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown8_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт время (в 1/10 минуты) бездействия пользователя, после которого программа выдаст запрос на зачернение монитора (выведение чёрного окна-заставки)\r\nОпция работает в програме, включённой в замедленный режим\r\nОпция аналогична опции операционной системы, но может использоваться как альтернативаня. Например, вы можете задать время бездействия 3 минуты, если вы обычно активно работаете, а когда читаете - перейти в медленный режим работы программы и программа не будет зачернять монитор.\r\nЗадайте большое значение (например, 120), если не хотите использовать возможность зачернения\r\nВвод осуществляется в 1/10 минуты", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox8_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("При запуске программа сразу же будет переведена в режим \"Отключить напоминания\" - то есть не будет напоминать о времени отдыха (но будет выдавать запросы, связанные с бездействием пользователя)", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboBox1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Задаёт режим работы, устанавливаемый программой при запуске", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown9_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Аналогично опции для быстрого режима.\r\nЕсли на протяжении времени, указанного в опции \"Длительность замедленного режима\" не было периода бездействия более заданного интервала, то произойдёт автоматическое переключение в замедленный режим.\r\n\r\nВвод осуществляется в секундах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown10_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Аналогично опции для замедленного режима.\r\n\r\nДанная опция используется для автоматического переключения из медленного режима в замедленный.\r\nЕсли на протяжении заданного интервала не было периода бездействия более интервала \"Интервал замедленного режима\", то произойдёт автоматическое переключение в замедленный режим.\r\n\r\nВвод осуществляется в минутах", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox9_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Во время отдыха мышь может совершать небольшие перемещения (от вибраций, натянутого провода).\r\nЕсли отмечено, малые перемещения мыши будут инорироваться\r\n\r\nВключение данной опции может потребовать выдачи вашим системным файерволом дополнительных прав программе на перехват системных сообщений мыши и клавиатуры\r\nДанная функция может замедлить производительность системы", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label14_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Ввод от 0 до 100%. При бездействии экран автоматически зачерняется, при этом время отдыха может начинаться при затемнении экрана (0%), либо отойти в прошлое на указанный % времени бездействия\r\n\r\nНапример, если пользователь бездействовал 3 минуты и не было реакции на вопрос о бездействии, то при значении 30% будет считаться, что к моменту зачернения экрана пользователь отдыхал уже 1 минуту\r\n\r\nПо-умолчанию 50% (среднее время бездействия)", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox10_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Для напоминания о времени отдыха по умолчанию выводится окно, однако, если окно мешает, может проигрываться звук сирены", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown12_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Ввод от 1 до 100%. Устанавливает громкость сирены, напоминающей о времени отдыха\r\n\r\nПо-умолчанию 50%", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox11_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Будет проигрывать звук сирены вместе с выдачей окна напоминания о времени отдыха", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown13_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Определяет коэффициент в формуле количества повторений звука сирены, чем больше коэффициент - тем дольше будет звучать сирена\r\n\r\nЭтот коэффициент относится к формуле, применяющейся при небольшой усталости, когда отдых был не так давно", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numericUpDown14_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Определяет коэффициент в формуле количества повторений звука сирены, чем больше коэффициент - тем дольше будет звучать сирена\r\n\r\nЭтот коэффициент относится к формуле, применяющейся при относительно большой усталости, когда отдых был довольно давно", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label18_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("Приложение может не выдавать напоминание, если видит, что вы заняты. Это необходимо, когда вы печатаете текст и не смотрите на экран (чтобы не пропал ввод).", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SirenButton_Click(object sender, EventArgs e)
        {
            int i = Form1.opts[Form1.optsName[30], 0];
            if (i > 0)
                Ask.SoundSiren(1, i);
        }

        private void SirenButton2_Click(object sender, EventArgs e)
        {
            int i = Form1.opts[Form1.optsName[31], 0];
            if (i > 0)
                Ask.SoundSiren(2, i);
        }
    }
}
