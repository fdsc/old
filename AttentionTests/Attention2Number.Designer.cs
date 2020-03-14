namespace AttentionTests
{
    partial class Attention2Number
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Attention2Number));
            this.main = new System.Windows.Forms.Label();
            this.helper = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.graphButton = new System.Windows.Forms.Button();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // main
            // 
            this.main.AutoSize = true;
            this.main.Font = new System.Drawing.Font("Microsoft Sans Serif", 38F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.main.Location = new System.Drawing.Point(129, 61);
            this.main.Name = "main";
            this.main.Size = new System.Drawing.Size(53, 59);
            this.main.TabIndex = 0;
            this.main.Tag = "";
            this.main.Text = "0";
            this.main.Click += new System.EventHandler(this.main_Click);
            // 
            // helper
            // 
            this.helper.AutoSize = true;
            this.helper.Location = new System.Drawing.Point(47, 9);
            this.helper.Name = "helper";
            this.helper.Size = new System.Drawing.Size(58, 13);
            this.helper.TabIndex = 1;
            this.helper.Text = "Введите 0";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(1, 173);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(286, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Вводить что видишь  (на владение цифровой клав.)";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            this.checkBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyDown);
            this.checkBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyUp);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(1, 155);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(115, 17);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.Text = "Удлинённый тест";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            this.checkBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyDown);
            this.checkBox2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyUp);
            // 
            // graphButton
            // 
            this.graphButton.Location = new System.Drawing.Point(232, 144);
            this.graphButton.Name = "graphButton";
            this.graphButton.Size = new System.Drawing.Size(75, 23);
            this.graphButton.TabIndex = 3;
            this.graphButton.Text = "График";
            this.graphButton.UseVisualStyleBackColor = true;
            this.graphButton.Visible = false;
            this.graphButton.Click += new System.EventHandler(this.graphButton_Click);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(1, 138);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(87, 17);
            this.checkBox3.TabIndex = 4;
            this.checkBox3.Text = "\"Три числа\"";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            this.checkBox3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyDown);
            this.checkBox3.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyUp);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 196);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 5;
            // 
            // Attention2Number
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 217);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.graphButton);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.helper);
            this.Controls.Add(this.main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Attention2Number";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Экспресс-тест на внимание \"два числа\"";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Attention2Number_Load);
            this.Shown += new System.EventHandler(this.Attention2Number_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Attention2Number_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Attention2Number_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label main;
        private System.Windows.Forms.Label helper;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button graphButton;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}