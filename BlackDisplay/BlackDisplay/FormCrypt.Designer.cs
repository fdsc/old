namespace BlackDisplay
{
    partial class FormCrypt
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
            this.hashCountBox = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CryptButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.FileNameBox = new System.Windows.Forms.RichTextBox();
            this.PostfixBox = new System.Windows.Forms.RichTextBox();
            this.OpenButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.checkButton = new System.Windows.Forms.Button();
            this.nonLZMA = new System.Windows.Forms.CheckBox();
            this.CloseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.hashCountBox)).BeginInit();
            this.SuspendLayout();
            // 
            // hashCountBox
            // 
            this.hashCountBox.Location = new System.Drawing.Point(3, 86);
            this.hashCountBox.Maximum = new decimal(new int[] {
            1233977344,
            465661,
            0,
            0});
            this.hashCountBox.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.hashCountBox.Name = "hashCountBox";
            this.hashCountBox.Size = new System.Drawing.Size(133, 20);
            this.hashCountBox.TabIndex = 0;
            this.hashCountBox.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 57);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(133, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Сложность перебора";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(53, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Подготовка к шифрованию";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "keccak; CFB",
            "keccak; 28147-89; mod 28147-89"});
            this.comboBox1.Location = new System.Drawing.Point(142, 85);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(176, 21);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(142, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Режим шифрования";
            // 
            // CryptButton
            // 
            this.CryptButton.Enabled = false;
            this.CryptButton.Location = new System.Drawing.Point(93, 253);
            this.CryptButton.Name = "CryptButton";
            this.CryptButton.Size = new System.Drawing.Size(133, 23);
            this.CryptButton.TabIndex = 6;
            this.CryptButton.Text = "Зашифровать";
            this.CryptButton.UseVisualStyleBackColor = true;
            this.CryptButton.Click += new System.EventHandler(this.CryptButton_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(3, 175);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(315, 72);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "* Перед шифрованием запомните пароль/ключ (остальные опции запомнятся в зашифрова" +
    "нном файле)\n\n* После шифрования удалите файл, который шифровали, если в этом ест" +
    "ь необходимость.";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // FileNameBox
            // 
            this.FileNameBox.BackColor = System.Drawing.SystemColors.Control;
            this.FileNameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FileNameBox.Location = new System.Drawing.Point(3, 19);
            this.FileNameBox.Name = "FileNameBox";
            this.FileNameBox.ReadOnly = true;
            this.FileNameBox.Size = new System.Drawing.Size(315, 32);
            this.FileNameBox.TabIndex = 8;
            this.FileNameBox.Text = "";
            // 
            // PostfixBox
            // 
            this.PostfixBox.BackColor = System.Drawing.SystemColors.Control;
            this.PostfixBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PostfixBox.Location = new System.Drawing.Point(3, 127);
            this.PostfixBox.Name = "PostfixBox";
            this.PostfixBox.ReadOnly = true;
            this.PostfixBox.Size = new System.Drawing.Size(315, 42);
            this.PostfixBox.TabIndex = 9;
            this.PostfixBox.Text = "1\n2\n3";
            // 
            // OpenButton
            // 
            this.OpenButton.Location = new System.Drawing.Point(3, 253);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(75, 23);
            this.OpenButton.TabIndex = 10;
            this.OpenButton.Text = "Открыть";
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Visible = false;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(243, 19);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 11;
            this.DeleteButton.Text = "Удалить";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Visible = false;
            this.DeleteButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkButton
            // 
            this.checkButton.Location = new System.Drawing.Point(123, 253);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(75, 23);
            this.checkButton.TabIndex = 12;
            this.checkButton.Text = "Проверить";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Visible = false;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // nonLZMA
            // 
            this.nonLZMA.AutoSize = true;
            this.nonLZMA.BackColor = System.Drawing.SystemColors.Control;
            this.nonLZMA.Location = new System.Drawing.Point(142, 108);
            this.nonLZMA.Name = "nonLZMA";
            this.nonLZMA.Size = new System.Drawing.Size(85, 17);
            this.nonLZMA.TabIndex = 13;
            this.nonLZMA.Text = "Без сжатия";
            this.nonLZMA.UseVisualStyleBackColor = false;
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(243, 253);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 11;
            this.CloseButton.Text = "Закрыть";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Visible = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // FormCrypt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 280);
            this.Controls.Add(this.nonLZMA);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.OpenButton);
            this.Controls.Add(this.PostfixBox);
            this.Controls.Add(this.FileNameBox);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.CryptButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.hashCountBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCrypt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Шифрование файла";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormCrypt_FormClosed);
            this.Shown += new System.EventHandler(this.FormCrypt_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.hashCountBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown hashCountBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CryptButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox FileNameBox;
        private System.Windows.Forms.RichTextBox PostfixBox;
        private System.Windows.Forms.Button OpenButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.CheckBox nonLZMA;
        private System.Windows.Forms.Button CloseButton;
    }
}