namespace BlackDisplay
{
    partial class License
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.contactsInfoTextBox = new System.Windows.Forms.TextBox();
            this.yesButton = new System.Windows.Forms.Button();
            this.noButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(3, 30);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(932, 531);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "Лицензионное соглашение загружается из ресурсов программы...";
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(12, 7);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(47, 13);
            this.versionLabel.TabIndex = 1;
            this.versionLabel.Text = "Версия ";
            // 
            // contactsInfoTextBox
            // 
            this.contactsInfoTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.contactsInfoTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.contactsInfoTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.contactsInfoTextBox.Location = new System.Drawing.Point(182, 3);
            this.contactsInfoTextBox.Name = "contactsInfoTextBox";
            this.contactsInfoTextBox.Size = new System.Drawing.Size(483, 19);
            this.contactsInfoTextBox.TabIndex = 2;
            this.contactsInfoTextBox.Text = "http://relaxtime.8vs.ru/    E-Mail: prg@8vs.ru";
            this.contactsInfoTextBox.Click += new System.EventHandler(this.contactsInfoTextBox_Click);
            // 
            // yesButton
            // 
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Location = new System.Drawing.Point(762, -1);
            this.yesButton.Name = "yesButton";
            this.yesButton.Size = new System.Drawing.Size(77, 25);
            this.yesButton.TabIndex = 3;
            this.yesButton.Text = "Принимаю";
            this.yesButton.UseVisualStyleBackColor = true;
            this.yesButton.Visible = false;
            // 
            // noButton
            // 
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Location = new System.Drawing.Point(845, -1);
            this.noButton.Name = "noButton";
            this.noButton.Size = new System.Drawing.Size(90, 25);
            this.noButton.TabIndex = 3;
            this.noButton.Text = "Отказываюсь";
            this.noButton.UseVisualStyleBackColor = true;
            this.noButton.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(659, -1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 25);
            this.button1.TabIndex = 4;
            this.button1.Text = "Удалить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // License
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 561);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.noButton);
            this.Controls.Add(this.yesButton);
            this.Controls.Add(this.contactsInfoTextBox);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.richTextBox1);
            this.Name = "License";
            this.Text = "License";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.License_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.TextBox contactsInfoTextBox;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Button button1;
    }
}