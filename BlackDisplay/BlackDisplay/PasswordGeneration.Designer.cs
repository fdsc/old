namespace BlackDisplay
{
    partial class PasswordGeneration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Clear = new System.Windows.Forms.Button();
            this.generate = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.MouseAllowBox = new System.Windows.Forms.CheckBox();
            this.GlobalHookPwdCheckBox = new System.Windows.Forms.CheckBox();
            this.base64LengthCombo = new System.Windows.Forms.ComboBox();
            this.PwdLengthCombo = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button4 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.inBox = new BlackDisplay.SimplePasswordBox();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(720, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Нажимайте в поле ввода на клавиатуру и мышь (учитываются, в том числе, клавиши F1" +
    "-F12, crtl, alt, shift; для мыши - и позиция, и клавиша).";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(703, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Учитывается момент нажатия клавиши. Не учитывается раскладка клавиатуры. Движения" +
    " мышью учитываются, если отмечен флажок";
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(726, 2);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(96, 23);
            this.Clear.TabIndex = 5;
            this.Clear.Text = "Очистить";
            this.toolTip1.SetToolTip(this.Clear, "Сбросить все наработанные случайные биты");
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // generate
            // 
            this.generate.Location = new System.Drawing.Point(3, 645);
            this.generate.Name = "generate";
            this.generate.Size = new System.Drawing.Size(158, 23);
            this.generate.TabIndex = 6;
            this.generate.Text = "Base64 в буфер обмена";
            this.toolTip1.SetToolTip(this.generate, "Сгенерировать случайный массив и скопировать его в буфер обмена как строку Base64" +
        "");
            this.generate.UseVisualStyleBackColor = true;
            this.generate.Click += new System.EventHandler(this.generate_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Латиница и цифры",
            "Набор yandex.ru: латиница, цифры, символы !@#$%^&*()_-+:;,.",
            "Только цифры",
            "Латиница, цифры, символы ~`!@#$%^&*()_-+=|\\{[}]:;\"\'<,>.?/",
            "Цифры 16-ричной системы счисления (0123456789ABCDEF)",
            "GUID",
            "Латиница строчные и цифры"});
            this.comboBox1.Location = new System.Drawing.Point(213, 619);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(779, 21);
            this.comboBox1.TabIndex = 9;
            this.toolTip1.SetToolTip(this.comboBox1, "Выберите, какие символы могут присутствовать в пароле");
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(0, 622);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(207, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Набор символов, допустимых в пароле";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(322, 645);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(158, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Пароль в буфер обмена";
            this.toolTip1.SetToolTip(this.button2, "Сгенерировать пароль и скопировать его в буфер обмена");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MouseAllowBox
            // 
            this.MouseAllowBox.AutoSize = true;
            this.MouseAllowBox.Checked = true;
            this.MouseAllowBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MouseAllowBox.Location = new System.Drawing.Point(813, 650);
            this.MouseAllowBox.Name = "MouseAllowBox";
            this.MouseAllowBox.Size = new System.Drawing.Size(179, 17);
            this.MouseAllowBox.TabIndex = 13;
            this.MouseAllowBox.Text = "Учитывать движения мышкой";
            this.toolTip1.SetToolTip(this.MouseAllowBox, "Окно будет обрабатывать движения мышью даже без нажатия");
            this.MouseAllowBox.UseVisualStyleBackColor = true;
            this.MouseAllowBox.CheckedChanged += new System.EventHandler(this.MouseAllowBox_CheckedChanged);
            // 
            // GlobalHookPwdCheckBox
            // 
            this.GlobalHookPwdCheckBox.AutoSize = true;
            this.GlobalHookPwdCheckBox.Checked = true;
            this.GlobalHookPwdCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GlobalHookPwdCheckBox.Location = new System.Drawing.Point(755, 650);
            this.GlobalHookPwdCheckBox.Name = "GlobalHookPwdCheckBox";
            this.GlobalHookPwdCheckBox.Size = new System.Drawing.Size(49, 17);
            this.GlobalHookPwdCheckBox.TabIndex = 17;
            this.GlobalHookPwdCheckBox.Text = "Фон";
            this.toolTip1.SetToolTip(this.GlobalHookPwdCheckBox, "Окно будет обрабатывать любые нажатия и движения мышью, в том числе в свёрнутом с" +
        "остоянии");
            this.GlobalHookPwdCheckBox.UseVisualStyleBackColor = true;
            this.GlobalHookPwdCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // base64LengthCombo
            // 
            this.base64LengthCombo.FormattingEnabled = true;
            this.base64LengthCombo.Items.AddRange(new object[] {
            "Имеющиеся",
            "64 бита",
            "128 бит",
            "256 бит",
            "512 бит",
            "1024 бит",
            "2048 бит",
            "4096 бит",
            "8192 бит",
            "16384 бит",
            "32768 бит",
            "65536 бит"});
            this.base64LengthCombo.Location = new System.Drawing.Point(167, 646);
            this.base64LengthCombo.Name = "base64LengthCombo";
            this.base64LengthCombo.Size = new System.Drawing.Size(140, 21);
            this.base64LengthCombo.TabIndex = 18;
            this.toolTip1.SetToolTip(this.base64LengthCombo, "Длина в битах запрашиваемого массива");
            // 
            // PwdLengthCombo
            // 
            this.PwdLengthCombo.FormattingEnabled = true;
            this.PwdLengthCombo.Items.AddRange(new object[] {
            "Имеющиеся",
            "18 символов",
            "21 символов",
            "24 символов",
            "27 символ",
            "30 символа",
            "33 символов"});
            this.PwdLengthCombo.Location = new System.Drawing.Point(486, 646);
            this.PwdLengthCombo.Name = "PwdLengthCombo";
            this.PwdLengthCombo.Size = new System.Drawing.Size(140, 21);
            this.PwdLengthCombo.TabIndex = 19;
            this.toolTip1.SetToolTip(this.PwdLengthCombo, "Длина запрашиваемого пароля в символах");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(828, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(164, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "Срезать генерированное";
            this.toolTip1.SetToolTip(this.button1, "Сбросить биты, которые непосредственно участвовали в генерации");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(322, 674);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(158, 23);
            this.button4.TabIndex = 22;
            this.button4.Text = "Цифробуквенная таблица";
            this.toolTip1.SetToolTip(this.button4, "Сгенерировать таблицу (см. в файлах tmp.gif и tmp2.gif )");
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(3, 674);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(158, 23);
            this.button6.TabIndex = 23;
            this.button6.Text = "Двоичные данные в файл";
            this.toolTip1.SetToolTip(this.button6, "Записать случайную последовательность байт в файл tmp.bin");
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(632, 646);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(117, 23);
            this.button3.TabIndex = 21;
            this.button3.Text = "Проверить буфер";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // inBox
            // 
            this.inBox.isGlobalBackground = false;
            this.inBox.Location = new System.Drawing.Point(1, 31);
            this.inBox.Masked = false;
            this.inBox.Name = "inBox";
            this.inBox.Size = new System.Drawing.Size(991, 582);
            this.inBox.TabIndex = 0;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(486, 674);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(158, 23);
            this.button5.TabIndex = 22;
            this.button5.Text = "Цифробуквенная латинская";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // PasswordGeneration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 699);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.PwdLengthCombo);
            this.Controls.Add(this.base64LengthCombo);
            this.Controls.Add(this.GlobalHookPwdCheckBox);
            this.Controls.Add(this.MouseAllowBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.generate);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PasswordGeneration";
            this.Text = "Генерация случайных паролей";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PasswordGeneration_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PasswordGeneration_FormClosed);
            this.Load += new System.EventHandler(this.PasswordGeneration_Load);
            this.Resize += new System.EventHandler(this.PasswordGeneration_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SimplePasswordBox inBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Button generate;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox MouseAllowBox;
        private System.Windows.Forms.CheckBox GlobalHookPwdCheckBox;
        private System.Windows.Forms.ComboBox base64LengthCombo;
        private System.Windows.Forms.ComboBox PwdLengthCombo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
    }
}