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
    public partial class License : Form
    {
        public License()
        {
            InitializeComponent();
            versionLabel.Text = "Версия программы " + Program.version;
        }

        public DialogResult dialogView()
        {
            yesButton.Visible = true;
            noButton .Visible = true;
            contactsInfoTextBox.Visible = false;

            return this.ShowDialog();
        }

        private void contactsInfoTextBox_Click(object sender, EventArgs e)
        {/*
            var dlgResult = MessageBox.Show("Хотите скопировать в буфер обмена адрес e-mail (кнопка да) или адрес сайта (кнопка нет)?", "Копирование", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dlgResult == System.Windows.Forms.DialogResult.Yes)
                Clipboard.SetText("prg@8vs.ru");
            else
            if (dlgResult == System.Windows.Forms.DialogResult.No)
                Clipboard.SetText("http://relaxtime.8vs.ru/");*/
            // Clipboard не работает в MTAThread, а WaitAll невозможно использовать в STAThread
        }

        private void License_Shown(object sender, EventArgs e)
        {
            this.Text = "О программе (" + AppDomain.CurrentDomain.BaseDirectory + ")";
            richTextBox1.Text = BlackDisplay.Properties.Resources.License;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить программу с компьютера?", "Анинсталляция", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                return;

            Close();
            Program.uninstall(true);
        }
    }
}
