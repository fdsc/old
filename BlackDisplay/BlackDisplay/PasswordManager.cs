using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using keccak;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlackDisplay
{
    public partial class PasswordManager : Form
    {
        public PasswordManager()
        {
            InitializeComponent();
        }

        private void PasswordManager_Load(object sender, EventArgs e)
        {

        }

        private void PasswordManager_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            decryptFile();
        }

        private void decryptFile()
        {
            var cnt = new BlackDisplay.Form1.OpenFileDialogContext(this);
            // cnt.dialog.InitialDirectory = ".";
            cnt.dialog.Multiselect = false;
            cnt.dialog.RestoreDirectory = true;
            cnt.dialog.SupportMultiDottedExtensions = true;
            cnt.dialog.CheckFileExists = false;
            cnt.dialog.Filter = "Black display pass. files|*.bdcp|All|*.*";
            cnt.dialog.FilterIndex = 1;
            cnt.dialog.Title = "Выберите файл для расшифровки";

            cnt.closedEvent += new BlackDisplay.Form1.OpenFileDialogContext.closed(cnt_closedEventDecrypt);
            cnt.show();
        }

        PasswordSecure key;
        string fileName;
        void cnt_closedEventDecrypt(Form1.OpenFileDialogContext context, bool isOK)
        {
            if (!isOK)
                return;

            toStart:

            DoublePasswordForm pwdForm1, pwdForm2 = null;
            bool isSuccess = false;
            do
            {
                pwdForm1 = new DoublePasswordForm(1, Path.GetFileName(context.dialog.FileName));
                pwdForm1.ShowDialog();

                if (pwdForm1.resultText == null)
                    return;

                if (pwdForm1.resultText.Length < 6)
                {
                    MessageBox.Show("Извините, но введённый текст настолько мал, что не может являться паролем", "Шифрование", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    continue;
                }

                if (pwdForm1.fromFile)
                    break;

                pwdForm2 = new DoublePasswordForm(2, Path.GetFileName(context.dialog.FileName));
                pwdForm2.ShowDialog();

                if (pwdForm2.cancel)
                {
                    pwdForm1.clearResultText();
                    return;
                }

                if (!String.IsNullOrEmpty(pwdForm2.resultText) && pwdForm1.resultText != pwdForm2.resultText)
                {
                    pwdForm1.clearResultText();
                    pwdForm2.clearResultText();
                    if (MessageBox.Show("Введённые пароли не равны друг другу\r\nХотите попробовать ещё раз?", "Шифрование", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                        return;
                }
                else
                    isSuccess = true;
            }
            while (!isSuccess);

            ClearKey();
            ClearWindow();

            byte[] key1;
            Form1.GetKeyByPassword(pwdForm1, out key1, 22);
            pwdForm1.clearResultText();
            pwdForm2.clearResultText();

            var sha = new SHA3(0);

            var fi = new FileInfo(context.dialog.FileName);
            fileName = fi.FullName;
            if (!fi.Exists)
            {
                var newBytes = sha.multiCryptLZMA(new byte[0], key1, null, 12, false, 0, SHA3.getHashCountForMultiHash() - 8);
                File.WriteAllBytes(fileName, newBytes);
            }

            try
            {
                var openFile = File.ReadAllBytes(fileName);
                var decryptedFile = sha.multiDecryptLZMA(openFile, key1);

                if (decryptedFile == null)
                {
                    MessageBox.Show("Файл расшифровать не удалось");
                    BytesBuilder.BytesToNull(key1);
                    goto toStart;
                }

                this.key = new PasswordSecure(key1);
                var str = Encoding.UTF32.GetString(decryptedFile);

                BytesBuilder.ToNull(openFile);
                BytesBuilder.ToNull(decryptedFile);

                GetRecordsFromFile(str);
                BytesBuilder.ClearString(str);
            }
            catch (Exception e)
            {
                MessageBox.Show("Расшифрование не удалось! " + e.Message, "Расшифрование не удалось", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetTagsAndMainTags();
        }

        private void SetTagsAndMainTags()
        {
            foreach (var record in records)
            {
                foundedBox.Items.Add(record.Key);
                foreach (var tag in record.Value.tags)
                {
                    if (!tags.Contains(tag))
                    {
                        tags.Add(tag);
                        searchTagList.Items.Add(tag);
                        // tagList      .Items.Add(tag);
                    }
                }
            }
        }

        public void pwdToClipboard(string pwd)
        {
            var t = new object();

            // Для работы с буфером обмена приходится извращаться, т.к. он поддерживает только потоки STAThread
            var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        Clipboard.SetText(pwd);
                    }
                    finally
                    {
                        lock (t)
                            Monitor.Pulse(t);
                    }
                }/*,
                64 * 1024*/
                );

            lock (t)
            {
                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                Monitor.Wait(t);
            }
        }

         public string pwdFromClipboard()
        {
            string result = null;
            var t = new object();

            // Для работы с буфером обмена приходится извращаться, т.к. он поддерживает только потоки STAThread
            var thr = new Thread
                ((ThreadStart)
                delegate
                {
                    try
                    {
                        result = Clipboard.GetText();
                    }
                    finally
                    {
                        lock (t)
                            Monitor.Pulse(t);
                    }
                }/*,
                64 * 1024*/
                );

            lock (t)
            {
                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
                Monitor.Wait(t);
            }

            return result;
        }

        private void ClearWindow(bool onlyLists = false)
        {
            searchTextBox.Clear();
            foundedBox.Items.Clear();
            tagList.Items.Clear();
            searchTagList.Items.Clear();

            if (!onlyLists)
            {
                textBox.Clear();
                newTagBox.Clear();
                RecordNameBox.Clear();
                currentRecord = null;

                clearRecords();
                tags.Clear();
                clearClipboard();
            }
        }

        private void clearClipboard()
        {
            pwdToClipboard("                                                                                                                              ");
            pwdToClipboard(" ");
        }

        private void GetRecordsFromFile(string str)
        {
            using (var sr = new StringReader(str))
            {
                int counter = 0;
                var line = sr.ReadLine();
                string pwdLine = null;
                string MainTag = null;
                string cryptLine = null;
                PasswordRecord current = null;

                while (line != null)
                {
                    line = line.Trim();

                    if (line == "" && counter > 2)
                    {
                        counter = 0;
                        line = sr.ReadLine();
                        continue;
                    }

                    switch (counter)
                    {
                        case 0:
                            MainTag = line;
                            break;
                        case 1:
                            cryptLine = line;
                            break;
                        case 2:
                            pwdLine = line;
                            current = new PasswordRecord(pwdLine, cryptLine, MainTag);
                            records.Add(MainTag, current);
                            break;
                        default:
                            current.tags.Add(line);
                            break;
                    }

                    counter++;
                    line = sr.ReadLine();
                }
            }
        }

        public void recordsToFile()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var record in records)
            {
                sb.AppendLine(record.Value.mainTag);
                sb.AppendLine(record.Value.crypt);
                sb.AppendLine(record.Value.pwd);
                foreach (var tag in record.Value.tags)
                    sb.AppendLine(tag);

                sb.AppendLine();
            }

            var content = sb.ToString();
            sb.Clear();

            var curKey = key.getObjectValue();
            var sha    = new SHA3(content.Length);
            var bytes  = sha.multiCryptLZMA(  new UTF32Encoding().GetBytes(content), curKey, null, 22, false, 0, SHA3.getHashCountForMultiHash() - 8  );
            File.WriteAllBytes(fileName, bytes);

            BytesBuilder.ClearString(content);
            BytesBuilder.ToNull(curKey);
            BytesBuilder.ToNull(bytes);
        }

        PasswordRecord currentRecord = null;
        SortedList<string, PasswordRecord> records = new SortedList<string, PasswordRecord>();
        SortedSet<string> tags = new SortedSet<string>();

        public class PasswordRecord
        {
            public List<string> tags = new List<string>();
            public          string pwd;
            public          string crypt;
            public readonly string mainTag;

            public PasswordRecord(string base64Pwd, string base64Crypt, string MainTag)
            {
                pwd     = base64Pwd;
                crypt   = base64Crypt;
                mainTag = MainTag;
            }

            public void clear(bool notTagsClear = false)
            {
                BytesBuilder.ClearString(mainTag);
                BytesBuilder.ClearString(pwd);
                BytesBuilder.ClearString(crypt);

                if (!notTagsClear)
                {
                    foreach (var tag in tags)
                        BytesBuilder.ClearString(tag);
                    tags.Clear();
                }
            }

            public override string ToString()
            {
                return mainTag;
            }
        }

        private void PasswordManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearKey();
            ClearWindow();

            e.Cancel = false;
        }

        private void ClearKey()
        {
            if (key != null)
            {
                key.Dispose();
                key = null;
            }

            clearRecords();
        }

        private void clearRecords()
        {
            foreach (var record in records)
            {
                record.Value.clear();
            }

            records.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            save();
        }

        SHA3.SHA3Random random = null;
        public string crypt(string str, PasswordSecure pwd = null)
        {
            if (pwd == null)
                pwd = key;

            var sha     = new SHA3(str.Length);
            if (random == null)
            {
                var inits = str + DateTime.Now.ToString("r");
                var t = new UTF32Encoding().GetBytes(inits);
                var bbi = new BytesBuilder();
                bbi.add(t);
                bbi.add(sha.CreateInitVector(0, 64, 40));
                var init = bbi.getBytes();

                random = new SHA3.SHA3Random(init);

                bbi.clear();
                BytesBuilder.ClearString(inits);
                BytesBuilder.ToNull(init);
            }

            var bytes = new UTF32Encoding().GetBytes(str);

            var openKey = pwd.getObjectValue();
            var crypted = sha.multiCryptLZMA(bytes, openKey, null, 22, false);

            BytesBuilder.BytesToNull(openKey);
            BytesBuilder.BytesToNull(bytes);
            BytesBuilder.ClearString(str);

            return Convert.ToBase64String(crypted);
        }

        public void trunc8NullBytes(ref byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; bytes[i] == 0; i++)
                {
                    j++;
                    if (j == 8)
                    {
                        var newBytes = new byte[i - 8];
                        BytesBuilder.CopyTo(bytes, newBytes, 0, i - 8);
                        BytesBuilder.ToNull(bytes);

                        bytes = newBytes;
                        return;
                    }
                }
            }
        }

        public string decrypt(string base64str, PasswordSecure pwd = null)
        {
            if (pwd == null)
                pwd = key;

            var sha     = new SHA3(base64str.Length);
            var openKey = pwd.getObjectValue();
            var crypted = sha.multiDecryptLZMA(Convert.FromBase64String(base64str), openKey);
            BytesBuilder.BytesToNull(openKey);

            var result = new UTF32Encoding().GetString(crypted); //Convert.ToBase64String(crypted);
            BytesBuilder.ToNull(crypted);
            return result;
        }

        private void save()
        {
            if (currentRecord == null)
            {
                if (RecordNameBox.Text.Length > 0)
                {
                    currentRecord = new PasswordRecord(crypt(""), crypt(textBox.Text), RecordNameBox.Text);
                    records.Add(currentRecord.mainTag, currentRecord);
                }
                else
                    MessageBox.Show("Извините, необходимо ввести имя записи, по которой вы будете искать её в дальнейшем");
            }
            else
            {
                var newMainTag = RecordNameBox.Text.Trim();

                if (newMainTag.Length == 0)
                {
                    if (MessageBox.Show("Вы ввели пустое имя записи.\r\nВы уверены, что хотите удалить запись?", "Удаление записи", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                        return;
                }

                if (newMainTag != currentRecord.mainTag)
                {
                    var oldCR = currentRecord;
                    records.Remove(oldCR.mainTag);

                    if (newMainTag.Length > 0)
                    {
                        currentRecord = new PasswordRecord(oldCR.pwd, crypt(textBox.Text), newMainTag);
                        currentRecord.tags = oldCR.tags;
                        records.Add(currentRecord.mainTag, currentRecord);
                        setNewCurrentRecord(currentRecord.mainTag);
                    }
                }
                else
                {
                    currentRecord.crypt = crypt(textBox.Text);
                }
            }

            recordsToFile();
        }

        private void foundedBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (foundedBox.SelectedIndex < 0)
            {
                ClearWindow(true);
                return;
            }

            setNewCurrentRecord(foundedBox.SelectedItem as string);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clearClipboard();
            button4.BackColor = Color.FromName("control");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearKey();
            ClearWindow();
        }
        
        private void setNewCurrentRecord(string mainTag)
        {
            currentRecord = records[mainTag];
            RecordNameBox.Text = currentRecord.mainTag;

            foreach (var tag in tags)
            {
                tagList.Items.Add(tag);
            }

            for (int j = 0; j < currentRecord.tags.Count; j++)
            {
                var tag = currentRecord.tags[j];
                for (int i = 0; i < tagList.Items.Count; i++)
                {
                    if ((tagList.Items[i] as string).CompareTo(tag) == 0)
                    {
                        tagList.SetItemChecked(i, true);
                        break;
                    }
                }
            }

            textBox.Text = decrypt(currentRecord.crypt);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currentRecord == null)
                return;

            pwdToClipboard(decrypt(currentRecord.pwd));
            button4.BackColor = Color.Red;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            recryptFile();
        }

        private void recryptFile()
        {
            DoublePasswordForm pwdForm1, pwdForm2 = null;
            bool isSuccess = false;
            do
            {
                pwdForm1 = new DoublePasswordForm(1);
                pwdForm1.ShowDialog();

                if (pwdForm1.resultText == null)
                    return;

                if (pwdForm1.resultText.Length < 6)
                {
                    MessageBox.Show("Извините, но введённый текст настолько мал, что не может являться паролем", "Шифрование", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    continue;
                }

                if (pwdForm1.fromFile)
                    break;

                pwdForm2 = new DoublePasswordForm(2);
                pwdForm2.ShowDialog();

                if (pwdForm2.cancel)
                {
                    pwdForm1.clearResultText();
                    return;
                }

                if (!String.IsNullOrEmpty(pwdForm2.resultText) && pwdForm1.resultText != pwdForm2.resultText)
                {
                    pwdForm1.clearResultText();
                    pwdForm2.clearResultText();
                    if (MessageBox.Show("Введённые пароли не равны друг другу\r\nХотите попробовать ещё раз?", "Шифрование", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                        return;
                }
                else
                    isSuccess = true;
            }
            while (!isSuccess);

            byte[] newKey1;
            Form1.GetKeyByPassword(pwdForm1, out newKey1, 10);
            pwdForm1.clearResultText();
            pwdForm2.clearResultText();

            var sha = new SHA3(0);
            var newKey = new PasswordSecure(newKey1);
            foreach (var record in records)
            {
                record.Value.pwd   = crypt(decrypt(record.Value.pwd),   newKey);
                record.Value.crypt = crypt(decrypt(record.Value.crypt), newKey);
            }

            ClearKey();
            key = newKey;

            recordsToFile();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var tag = newTagBox.Text;
            newTagBox.Text = "";
            if (tags.Contains(tag))
            {
                var i = tagList.Items.IndexOf(tag);
                tagList.SetItemChecked(i, true);

                if (!currentRecord.tags.Contains(tag))
                    currentRecord.tags.Add(tag);
            }
            else
            {
                tags.Add(tag);
                currentRecord.tags.Add(tag);

                tagList      .Items.Add(tag);
                searchTagList.Items.Add(tag);

                var i = tagList.Items.IndexOf(tag);
                tagList.SetItemChecked(i, true);
            }

            recordsToFile();
        }

        private void newTagBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void tagList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            currentRecord.tags.Clear();
            foreach (var item in tagList.CheckedItems)
            {
                currentRecord.tags.Add(item as string);
            }

            recordsToFile();
        }
    }
}
