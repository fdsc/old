namespace BlackDisplay
{
    partial class PwdCheckResultForm
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
            this.pwdStrengthBar = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.textResultBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // pwdStrengthBar
            // 
            this.pwdStrengthBar.Location = new System.Drawing.Point(3, 2);
            this.pwdStrengthBar.Maximum = 1000;
            this.pwdStrengthBar.Name = "pwdStrengthBar";
            this.pwdStrengthBar.Size = new System.Drawing.Size(538, 23);
            this.pwdStrengthBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pwdStrengthBar.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(466, 202);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Закрыть";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textResultBox
            // 
            this.textResultBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textResultBox.Location = new System.Drawing.Point(3, 31);
            this.textResultBox.Name = "textResultBox";
            this.textResultBox.ReadOnly = true;
            this.textResultBox.Size = new System.Drawing.Size(538, 165);
            this.textResultBox.TabIndex = 2;
            this.textResultBox.Text = "";
            // 
            // PwdCheckResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 227);
            this.Controls.Add(this.textResultBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pwdStrengthBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PwdCheckResultForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PwdCheckResult";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PwdCheckResultForm_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar pwdStrengthBar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox textResultBox;
    }
}