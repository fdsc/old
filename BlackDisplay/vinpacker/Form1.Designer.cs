namespace vinpacker
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.keyNameBox = new System.Windows.Forms.TextBox();
            this.createChechBox = new System.Windows.Forms.CheckBox();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.packetNameBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // keyNameBox
            // 
            this.keyNameBox.Location = new System.Drawing.Point(12, 41);
            this.keyNameBox.Name = "keyNameBox";
            this.keyNameBox.Size = new System.Drawing.Size(199, 20);
            this.keyNameBox.TabIndex = 1;
            // 
            // createChechBox
            // 
            this.createChechBox.AutoSize = true;
            this.createChechBox.Location = new System.Drawing.Point(20, 66);
            this.createChechBox.Name = "createChechBox";
            this.createChechBox.Size = new System.Drawing.Size(68, 17);
            this.createChechBox.TabIndex = 2;
            this.createChechBox.Text = "Создать";
            this.createChechBox.UseVisualStyleBackColor = true;
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.CheckFileExists = false;
            this.OpenFileDialog1.FileName = "openFileDialog1";
            this.OpenFileDialog1.RestoreDirectory = true;
            this.OpenFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // packetNameBox
            // 
            this.packetNameBox.Items.AddRange(new object[] {
            "update",
            "relaxtime"});
            this.packetNameBox.Location = new System.Drawing.Point(90, 12);
            this.packetNameBox.Name = "packetNameBox";
            this.packetNameBox.Size = new System.Drawing.Size(121, 21);
            this.packetNameBox.TabIndex = 3;
            this.packetNameBox.Text = "update";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(216, 90);
            this.Controls.Add(this.packetNameBox);
            this.Controls.Add(this.createChechBox);
            this.Controls.Add(this.keyNameBox);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox keyNameBox;
        private System.Windows.Forms.CheckBox createChechBox;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        private System.Windows.Forms.ComboBox packetNameBox;
    }
}

