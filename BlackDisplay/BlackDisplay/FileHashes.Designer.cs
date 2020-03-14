namespace BlackDisplay
{
    partial class FileHashes
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
            this.fileNameBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.hashBox = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.prefixBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.saveDirBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // fileNameBox
            // 
            this.fileNameBox.Location = new System.Drawing.Point(2, 15);
            this.fileNameBox.Name = "fileNameBox";
            this.fileNameBox.Size = new System.Drawing.Size(898, 20);
            this.fileNameBox.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(599, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(300, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Вычислить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(1, 88);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(898, 255);
            this.logBox.TabIndex = 2;
            this.logBox.Text = "";
            // 
            // hashBox
            // 
            this.hashBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hashBox.FormattingEnabled = true;
            this.hashBox.Items.AddRange(new object[] {
            "224",
            "256",
            "384",
            "512"});
            this.hashBox.Location = new System.Drawing.Point(472, 64);
            this.hashBox.Name = "hashBox";
            this.hashBox.Size = new System.Drawing.Size(121, 21);
            this.hashBox.TabIndex = 3;
            this.hashBox.SelectedIndexChanged += new System.EventHandler(this.hashBox_SelectedIndexChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1, 64);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Остановить";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, -1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Имя папки";
            // 
            // prefixBox
            // 
            this.prefixBox.Location = new System.Drawing.Point(319, 64);
            this.prefixBox.Name = "prefixBox";
            this.prefixBox.Size = new System.Drawing.Size(147, 20);
            this.prefixBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(165, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Префикс файла результата";
            // 
            // saveDirBox
            // 
            this.saveDirBox.Location = new System.Drawing.Point(168, 38);
            this.saveDirBox.Name = "saveDirBox";
            this.saveDirBox.Size = new System.Drawing.Size(731, 20);
            this.saveDirBox.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-1, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Папка для сохранения";
            // 
            // FileHashes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 344);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.prefixBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.hashBox);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.saveDirBox);
            this.Controls.Add(this.fileNameBox);
            this.Name = "FileHashes";
            this.Text = "FileHashes";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FileHashes_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox fileNameBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.ComboBox hashBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox prefixBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox saveDirBox;
        private System.Windows.Forms.Label label3;
    }
}