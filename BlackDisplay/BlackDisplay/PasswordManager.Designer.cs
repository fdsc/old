namespace BlackDisplay
{
    partial class PasswordManager
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
            this.tagList = new System.Windows.Forms.CheckedListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.RichTextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.RecordNameBox = new System.Windows.Forms.TextBox();
            this.newTagBox = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.foundedBox = new System.Windows.Forms.ComboBox();
            this.searchTagList = new System.Windows.Forms.CheckedListBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tagList
            // 
            this.tagList.FormattingEnabled = true;
            this.tagList.Location = new System.Drawing.Point(-1, 30);
            this.tagList.Name = "tagList";
            this.tagList.Size = new System.Drawing.Size(480, 229);
            this.tagList.TabIndex = 1;
            this.tagList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.tagList_ItemCheck);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(404, 292);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Сохранить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(785, 32);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(119, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Открыть файл";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(-1, 321);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(482, 347);
            this.textBox.TabIndex = 4;
            this.textBox.Text = "";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(785, 59);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(119, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Закрыть файл";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(660, 59);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(119, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Очистить буфер";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // RecordNameBox
            // 
            this.RecordNameBox.Location = new System.Drawing.Point(-1, 295);
            this.RecordNameBox.Name = "RecordNameBox";
            this.RecordNameBox.Size = new System.Drawing.Size(399, 20);
            this.RecordNameBox.TabIndex = 9;
            // 
            // newTagBox
            // 
            this.newTagBox.Location = new System.Drawing.Point(-1, 269);
            this.newTagBox.Name = "newTagBox";
            this.newTagBox.Size = new System.Drawing.Size(399, 20);
            this.newTagBox.TabIndex = 11;
            this.newTagBox.TextChanged += new System.EventHandler(this.newTagBox_TextChanged);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(404, 267);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 12;
            this.button6.Text = "Метка";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // foundedBox
            // 
            this.foundedBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.foundedBox.FormattingEnabled = true;
            this.foundedBox.Location = new System.Drawing.Point(-1, 5);
            this.foundedBox.Name = "foundedBox";
            this.foundedBox.Size = new System.Drawing.Size(445, 21);
            this.foundedBox.TabIndex = 13;
            this.foundedBox.SelectedIndexChanged += new System.EventHandler(this.foundedBox_SelectedIndexChanged);
            // 
            // searchTagList
            // 
            this.searchTagList.CheckOnClick = true;
            this.searchTagList.FormattingEnabled = true;
            this.searchTagList.Location = new System.Drawing.Point(485, 88);
            this.searchTagList.Name = "searchTagList";
            this.searchTagList.Size = new System.Drawing.Size(419, 574);
            this.searchTagList.TabIndex = 14;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(660, 32);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(119, 23);
            this.button5.TabIndex = 15;
            this.button5.Text = "Пароль в буфер";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(536, 59);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(118, 23);
            this.button7.TabIndex = 16;
            this.button7.Tag = "";
            this.button7.Text = "Взять пароль";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(536, 32);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(118, 23);
            this.button8.TabIndex = 17;
            this.button8.Tag = "";
            this.button8.Text = "Перешифровать";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(485, 4);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(419, 20);
            this.searchTextBox.TabIndex = 18;
            // 
            // PasswordManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 669);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.searchTagList);
            this.Controls.Add(this.foundedBox);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.newTagBox);
            this.Controls.Add(this.RecordNameBox);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tagList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PasswordManager";
            this.Text = "Управление паролями";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PasswordManager_FormClosing);
            this.Load += new System.EventHandler(this.PasswordManager_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PasswordManager_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox tagList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox RecordNameBox;
        private System.Windows.Forms.TextBox newTagBox;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ComboBox foundedBox;
        private System.Windows.Forms.CheckedListBox searchTagList;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.TextBox searchTextBox;


    }
}