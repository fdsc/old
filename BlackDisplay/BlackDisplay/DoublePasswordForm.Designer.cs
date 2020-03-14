namespace BlackDisplay
{
    partial class DoublePasswordForm
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
            this.Cancel = new System.Windows.Forms.Button();
            this.EnterPwd = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.box1 = new BlackDisplay.SimplePasswordBox();
            this.ByFileButton = new System.Windows.Forms.Button();
            this.oldPassword = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(122, 166);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(115, 33);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Отмена";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // EnterPwd
            // 
            this.EnterPwd.Location = new System.Drawing.Point(243, 166);
            this.EnterPwd.Name = "EnterPwd";
            this.EnterPwd.Size = new System.Drawing.Size(327, 33);
            this.EnterPwd.TabIndex = 8;
            this.EnterPwd.Text = "Ввести";
            this.EnterPwd.UseVisualStyleBackColor = true;
            this.EnterPwd.Click += new System.EventHandler(this.EnterPwd_Click);
            // 
            // box1
            // 
            this.box1.isGlobalBackground = false;
            this.box1.Location = new System.Drawing.Point(1, 2);
            this.box1.Masked = false;
            this.box1.Name = "box1";
            this.box1.Size = new System.Drawing.Size(569, 162);
            this.box1.TabIndex = 6;
            this.toolTip1.SetToolTip(this.box1, "Введите пароль. Паролем может быть стих или проза");
            this.box1.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.box1_HelpRequested);
            // 
            // ByFileButton
            // 
            this.ByFileButton.Location = new System.Drawing.Point(1, 166);
            this.ByFileButton.Name = "ByFileButton";
            this.ByFileButton.Size = new System.Drawing.Size(115, 33);
            this.ByFileButton.TabIndex = 11;
            this.ByFileButton.Text = "Из файла";
            this.ByFileButton.UseVisualStyleBackColor = true;
            this.ByFileButton.Click += new System.EventHandler(this.ByFileButton_Click);
            // 
            // oldPassword
            // 
            this.oldPassword.AutoSize = true;
            this.oldPassword.Location = new System.Drawing.Point(245, 200);
            this.oldPassword.Name = "oldPassword";
            this.oldPassword.Size = new System.Drawing.Size(199, 17);
            this.oldPassword.TabIndex = 12;
            this.oldPassword.Text = "Старый пароль из буфера обмена";
            this.oldPassword.UseVisualStyleBackColor = true;
            // 
            // DoublePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 217);
            this.Controls.Add(this.oldPassword);
            this.Controls.Add(this.ByFileButton);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.EnterPwd);
            this.Controls.Add(this.box1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DoublePasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Введите пароль к файлу";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DoublePasswordForm_FormClosed);
            this.Shown += new System.EventHandler(this.DoublePasswordForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button EnterPwd;
        private SimplePasswordBox box1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button ByFileButton;
        private System.Windows.Forms.CheckBox oldPassword;
    }
}