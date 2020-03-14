namespace BlackDisplay
{
    partial class SimplePasswordBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.box = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // box
            // 
            this.box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.box.Location = new System.Drawing.Point(0, 0);
            this.box.Name = "box";
            this.box.Size = new System.Drawing.Size(363, 236);
            this.box.TabIndex = 0;
            this.box.Text = "";
            this.box.KeyDown += new System.Windows.Forms.KeyEventHandler(this.box_KeyDown);
            this.box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.box_KeyPress);
            this.box.KeyUp += new System.Windows.Forms.KeyEventHandler(this.box_KeyUp);
            this.box.MouseMove += new System.Windows.Forms.MouseEventHandler(this.box_MouseMove);
            this.box.MouseUp += new System.Windows.Forms.MouseEventHandler(this.box_MouseUp);
            this.box.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.box_PreviewKeyDown);
            // 
            // SimplePasswordBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.box);
            this.Name = "SimplePasswordBox";
            this.Size = new System.Drawing.Size(363, 236);
            this.Resize += new System.EventHandler(this.SimplePasswordBox_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox box;
    }
}
