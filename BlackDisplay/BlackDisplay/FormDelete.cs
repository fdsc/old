using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BlackDisplay
{
    public partial class FormDelete : Form
    {
        public FormDelete()
        {
            InitializeComponent();
        }

        Form1.DoDataSanitizationObject ddso = null;
        public FormDelete(Form1.DoDataSanitizationObject ddso): this()
        {
            this.ddso = ddso;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ddso.countToWrite = (int) numericUpDown1.Value;
            ddso.complex = slowErase.Checked;
        }
    }
}
