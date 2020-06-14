namespace BlackDisplay
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.testsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.тестыВниманияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.красночёрнаяТаблицаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.экспресстест2ЧислаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.справкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.планироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.шифрованиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.генерироватьПарольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.вычислениеХэшейToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.контрольНеизменностиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.проконтролироватьНеизменностьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сравнитьХешиДляКонтроляToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.шифроватьФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.расшифроватьФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалениеФайлаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьФайлМногопроходноToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьФайлТремяПроходамиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьФайлОднимПроходомToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьДиректориюToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьДиректориюОднимПроходомToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.создатьНаДискеБольшойФайлToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьБольшойФайлТриПроходаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьБольшойФайлОдинПроходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.режимToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.быстрыйToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.замедленныйToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.медленныйToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.медленныйРучнойToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.неВыдаватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.игровойToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.неБеспокоитьВообщеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.отключитьНапоминанияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.толькоСиренаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сокращённаяСиренаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.убратьМенюToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testsMenu.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.testsMenu;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Экран учёта отдыха";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // testsMenu
            // 
            this.testsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.тестыВниманияToolStripMenuItem,
            this.оПрограммеToolStripMenuItem,
            this.справкаToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.tasksToolStripMenuItem,
            this.режимToolStripMenuItem,
            this.выходToolStripMenuItem});
            this.testsMenu.Name = "contextMenuStrip1";
            this.testsMenu.Size = new System.Drawing.Size(181, 180);
            // 
            // тестыВниманияToolStripMenuItem
            // 
            this.тестыВниманияToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.красночёрнаяТаблицаToolStripMenuItem,
            this.экспресстест2ЧислаToolStripMenuItem});
            this.тестыВниманияToolStripMenuItem.Name = "тестыВниманияToolStripMenuItem";
            this.тестыВниманияToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.тестыВниманияToolStripMenuItem.Text = "Тесты внимания";
            // 
            // красночёрнаяТаблицаToolStripMenuItem
            // 
            this.красночёрнаяТаблицаToolStripMenuItem.Name = "красночёрнаяТаблицаToolStripMenuItem";
            this.красночёрнаяТаблицаToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.красночёрнаяТаблицаToolStripMenuItem.Text = "Красно-чёрная таблица";
            this.красночёрнаяТаблицаToolStripMenuItem.Click += new System.EventHandler(this.красночёрнаяТаблицаToolStripMenuItem_Click);
            // 
            // экспресстест2ЧислаToolStripMenuItem
            // 
            this.экспресстест2ЧислаToolStripMenuItem.Name = "экспресстест2ЧислаToolStripMenuItem";
            this.экспресстест2ЧислаToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.экспресстест2ЧислаToolStripMenuItem.Text = "Экспресс-тест \"2 числа\"";
            this.экспресстест2ЧислаToolStripMenuItem.Click += new System.EventHandler(this.экспресстест2ЧислаToolStripMenuItem_Click);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // справкаToolStripMenuItem
            // 
            this.справкаToolStripMenuItem.Name = "справкаToolStripMenuItem";
            this.справкаToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.справкаToolStripMenuItem.Text = "Справка (веб)";
            this.справкаToolStripMenuItem.Click += new System.EventHandler(this.справкаToolStripMenuItem_Click);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            this.настройкиToolStripMenuItem.Click += new System.EventHandler(this.настройкиToolStripMenuItem_Click);
            // 
            // tasksToolStripMenuItem
            // 
            this.tasksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.планироватьToolStripMenuItem,
            this.шифрованиеToolStripMenuItem,
            this.удалениеФайлаToolStripMenuItem});
            this.tasksToolStripMenuItem.Name = "tasksToolStripMenuItem";
            this.tasksToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.tasksToolStripMenuItem.Text = "Дополнительно";
            // 
            // планироватьToolStripMenuItem
            // 
            this.планироватьToolStripMenuItem.Name = "планироватьToolStripMenuItem";
            this.планироватьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.планироватьToolStripMenuItem.Text = "Планировать";
            this.планироватьToolStripMenuItem.Visible = false;
            this.планироватьToolStripMenuItem.Click += new System.EventHandler(this.планироватьToolStripMenuItem_Click);
            // 
            // шифрованиеToolStripMenuItem
            // 
            this.шифрованиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.генерироватьПарольToolStripMenuItem,
            this.вычислениеХэшейToolStripMenuItem,
            this.контрольНеизменностиToolStripMenuItem,
            this.шифроватьФайлToolStripMenuItem,
            this.расшифроватьФайлToolStripMenuItem});
            this.шифрованиеToolStripMenuItem.Name = "шифрованиеToolStripMenuItem";
            this.шифрованиеToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.шифрованиеToolStripMenuItem.Text = "Шифрование";
            // 
            // генерироватьПарольToolStripMenuItem
            // 
            this.генерироватьПарольToolStripMenuItem.Name = "генерироватьПарольToolStripMenuItem";
            this.генерироватьПарольToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.генерироватьПарольToolStripMenuItem.Text = "Генерировать пароль";
            this.генерироватьПарольToolStripMenuItem.Click += new System.EventHandler(this.генерироватьПарольToolStripMenuItem_Click);
            // 
            // вычислениеХэшейToolStripMenuItem
            // 
            this.вычислениеХэшейToolStripMenuItem.Name = "вычислениеХэшейToolStripMenuItem";
            this.вычислениеХэшейToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.вычислениеХэшейToolStripMenuItem.Text = "Вычисление хэшей";
            this.вычислениеХэшейToolStripMenuItem.Click += new System.EventHandler(this.вычислениеХэшейToolStripMenuItem_Click);
            // 
            // контрольНеизменностиToolStripMenuItem
            // 
            this.контрольНеизменностиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.проконтролироватьНеизменностьToolStripMenuItem,
            this.сравнитьХешиДляКонтроляToolStripMenuItem});
            this.контрольНеизменностиToolStripMenuItem.Name = "контрольНеизменностиToolStripMenuItem";
            this.контрольНеизменностиToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.контрольНеизменностиToolStripMenuItem.Text = "Контроль неизменности";
            this.контрольНеизменностиToolStripMenuItem.Visible = false;
            // 
            // проконтролироватьНеизменностьToolStripMenuItem
            // 
            this.проконтролироватьНеизменностьToolStripMenuItem.Name = "проконтролироватьНеизменностьToolStripMenuItem";
            this.проконтролироватьНеизменностьToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.проконтролироватьНеизменностьToolStripMenuItem.Text = "Вычислить хеши для контроля";
            this.проконтролироватьНеизменностьToolStripMenuItem.Click += new System.EventHandler(this.проконтролироватьНеизменностьToolStripMenuItem_Click);
            // 
            // сравнитьХешиДляКонтроляToolStripMenuItem
            // 
            this.сравнитьХешиДляКонтроляToolStripMenuItem.Name = "сравнитьХешиДляКонтроляToolStripMenuItem";
            this.сравнитьХешиДляКонтроляToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.сравнитьХешиДляКонтроляToolStripMenuItem.Text = "Сравнить хеши для контроля";
            this.сравнитьХешиДляКонтроляToolStripMenuItem.Click += new System.EventHandler(this.сравнитьХешиДляКонтроляToolStripMenuItem_Click);
            // 
            // шифроватьФайлToolStripMenuItem
            // 
            this.шифроватьФайлToolStripMenuItem.Name = "шифроватьФайлToolStripMenuItem";
            this.шифроватьФайлToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.шифроватьФайлToolStripMenuItem.Text = "Зашифровать файл";
            this.шифроватьФайлToolStripMenuItem.Click += new System.EventHandler(this.шифроватьФайлToolStripMenuItem_Click);
            // 
            // расшифроватьФайлToolStripMenuItem
            // 
            this.расшифроватьФайлToolStripMenuItem.Name = "расшифроватьФайлToolStripMenuItem";
            this.расшифроватьФайлToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.расшифроватьФайлToolStripMenuItem.Text = "Расшифровать файл";
            this.расшифроватьФайлToolStripMenuItem.Click += new System.EventHandler(this.расшифроватьФайлToolStripMenuItem_Click);
            // 
            // удалениеФайлаToolStripMenuItem
            // 
            this.удалениеФайлаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.удалитьФайлToolStripMenuItem,
            this.toolStripMenuItem1,
            this.удалитьФайлМногопроходноToolStripMenuItem,
            this.удалитьФайлТремяПроходамиToolStripMenuItem,
            this.удалитьФайлОднимПроходомToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolStripMenuItem4,
            this.toolStripMenuItem3,
            this.удалитьДиректориюToolStripMenuItem,
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem,
            this.удалитьДиректориюОднимПроходомToolStripMenuItem,
            this.toolStripSeparator3,
            this.создатьНаДискеБольшойФайлToolStripMenuItem1,
            this.создатьБольшойФайлТриПроходаToolStripMenuItem,
            this.создатьБольшойФайлОдинПроходToolStripMenuItem,
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem});
            this.удалениеФайлаToolStripMenuItem.Name = "удалениеФайлаToolStripMenuItem";
            this.удалениеФайлаToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.удалениеФайлаToolStripMenuItem.Text = "Удаление файла";
            // 
            // удалитьФайлToolStripMenuItem
            // 
            this.удалитьФайлToolStripMenuItem.Name = "удалитьФайлToolStripMenuItem";
            this.удалитьФайлToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьФайлToolStripMenuItem.Text = "Удалить файл";
            this.удалитьФайлToolStripMenuItem.Click += new System.EventHandler(this.удалитьФайлToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(423, 22);
            this.toolStripMenuItem1.Text = "Удалить файл простыми проходами x512";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // удалитьФайлМногопроходноToolStripMenuItem
            // 
            this.удалитьФайлМногопроходноToolStripMenuItem.Name = "удалитьФайлМногопроходноToolStripMenuItem";
            this.удалитьФайлМногопроходноToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьФайлМногопроходноToolStripMenuItem.Text = "Удалить файл по Гутману";
            this.удалитьФайлМногопроходноToolStripMenuItem.Click += new System.EventHandler(this.удалитьФайлМногопроходноToolStripMenuItem_Click);
            // 
            // удалитьФайлТремяПроходамиToolStripMenuItem
            // 
            this.удалитьФайлТремяПроходамиToolStripMenuItem.Name = "удалитьФайлТремяПроходамиToolStripMenuItem";
            this.удалитьФайлТремяПроходамиToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьФайлТремяПроходамиToolStripMenuItem.Text = "Удалить файл пятью проходами";
            this.удалитьФайлТремяПроходамиToolStripMenuItem.Click += new System.EventHandler(this.удалитьФайлТремяПроходамиToolStripMenuItem_Click);
            // 
            // удалитьФайлОднимПроходомToolStripMenuItem
            // 
            this.удалитьФайлОднимПроходомToolStripMenuItem.Name = "удалитьФайлОднимПроходомToolStripMenuItem";
            this.удалитьФайлОднимПроходомToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьФайлОднимПроходомToolStripMenuItem.Text = "Удалить файл одним проходом";
            this.удалитьФайлОднимПроходомToolStripMenuItem.Click += new System.EventHandler(this.удалитьФайлОднимПроходомToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(420, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(423, 22);
            this.toolStripMenuItem4.Text = "Удалить директорию";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(423, 22);
            this.toolStripMenuItem3.Text = "Удалить директорию простыми проходами x512";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // удалитьДиректориюToolStripMenuItem
            // 
            this.удалитьДиректориюToolStripMenuItem.Name = "удалитьДиректориюToolStripMenuItem";
            this.удалитьДиректориюToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьДиректориюToolStripMenuItem.Text = "Удалить директорию по Гутману";
            this.удалитьДиректориюToolStripMenuItem.Click += new System.EventHandler(this.удалитьДиректориюToolStripMenuItem_Click);
            // 
            // удалитьДиректориюТремяПроходамиToolStripMenuItem
            // 
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem.Name = "удалитьДиректориюТремяПроходамиToolStripMenuItem";
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem.Text = "Удалить директорию пятью проходами";
            this.удалитьДиректориюТремяПроходамиToolStripMenuItem.Click += new System.EventHandler(this.удалитьДиректориюТремяПроходамиToolStripMenuItem_Click);
            // 
            // удалитьДиректориюОднимПроходомToolStripMenuItem
            // 
            this.удалитьДиректориюОднимПроходомToolStripMenuItem.Name = "удалитьДиректориюОднимПроходомToolStripMenuItem";
            this.удалитьДиректориюОднимПроходомToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.удалитьДиректориюОднимПроходомToolStripMenuItem.Text = "Удалить директорию одним проходом";
            this.удалитьДиректориюОднимПроходомToolStripMenuItem.Click += new System.EventHandler(this.удалитьДиректориюОднимПроходомToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(420, 6);
            // 
            // создатьНаДискеБольшойФайлToolStripMenuItem1
            // 
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Enabled = false;
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Name = "создатьНаДискеБольшойФайлToolStripMenuItem1";
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Size = new System.Drawing.Size(423, 22);
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Text = "Создать большой файл (Гутман)";
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Visible = false;
            this.создатьНаДискеБольшойФайлToolStripMenuItem1.Click += new System.EventHandler(this.создатьНаДискеБольшойФайлToolStripMenuItem1_Click);
            // 
            // создатьБольшойФайлТриПроходаToolStripMenuItem
            // 
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Enabled = false;
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Name = "создатьБольшойФайлТриПроходаToolStripMenuItem";
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Text = "Создать большой файл (пять проходов)";
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Visible = false;
            this.создатьБольшойФайлТриПроходаToolStripMenuItem.Click += new System.EventHandler(this.создатьБольшойФайлТриПроходаToolStripMenuItem_Click);
            // 
            // создатьБольшойФайлОдинПроходToolStripMenuItem
            // 
            this.создатьБольшойФайлОдинПроходToolStripMenuItem.Name = "создатьБольшойФайлОдинПроходToolStripMenuItem";
            this.создатьБольшойФайлОдинПроходToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.создатьБольшойФайлОдинПроходToolStripMenuItem.Text = "Создать много файлов (не гарантирует полное перезатирание)";
            this.создатьБольшойФайлОдинПроходToolStripMenuItem.Click += new System.EventHandler(this.создатьБольшойФайлОдинПроходToolStripMenuItem_Click);
            // 
            // режимToolStripMenuItem
            // 
            this.режимToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.быстрыйToolStripMenuItem,
            this.замедленныйToolStripMenuItem,
            this.медленныйToolStripMenuItem,
            this.медленныйРучнойToolStripMenuItem,
            this.неВыдаватьToolStripMenuItem,
            this.игровойToolStripMenuItem,
            this.неБеспокоитьВообщеToolStripMenuItem,
            this.toolStripSeparator1,
            this.отключитьНапоминанияToolStripMenuItem,
            this.толькоСиренаToolStripMenuItem,
            this.сокращённаяСиренаToolStripMenuItem});
            this.режимToolStripMenuItem.Name = "режимToolStripMenuItem";
            this.режимToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.режимToolStripMenuItem.Text = "Режим";
            // 
            // быстрыйToolStripMenuItem
            // 
            this.быстрыйToolStripMenuItem.Name = "быстрыйToolStripMenuItem";
            this.быстрыйToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.быстрыйToolStripMenuItem.Text = "Быстрый";
            this.быстрыйToolStripMenuItem.Click += new System.EventHandler(this.быстрыйToolStripMenuItem_Click);
            // 
            // замедленныйToolStripMenuItem
            // 
            this.замедленныйToolStripMenuItem.Name = "замедленныйToolStripMenuItem";
            this.замедленныйToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.замедленныйToolStripMenuItem.Text = "Замедленный";
            this.замедленныйToolStripMenuItem.Click += new System.EventHandler(this.замедленныйToolStripMenuItem_Click);
            // 
            // медленныйToolStripMenuItem
            // 
            this.медленныйToolStripMenuItem.Name = "медленныйToolStripMenuItem";
            this.медленныйToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.медленныйToolStripMenuItem.Text = "Медленный";
            this.медленныйToolStripMenuItem.Click += new System.EventHandler(this.медленныйToolStripMenuItem_Click);
            // 
            // медленныйРучнойToolStripMenuItem
            // 
            this.медленныйРучнойToolStripMenuItem.Name = "медленныйРучнойToolStripMenuItem";
            this.медленныйРучнойToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.медленныйРучнойToolStripMenuItem.Text = "Медленный ручной";
            this.медленныйРучнойToolStripMenuItem.Click += new System.EventHandler(this.медленныйРучнойToolStripMenuItem_Click);
            // 
            // неВыдаватьToolStripMenuItem
            // 
            this.неВыдаватьToolStripMenuItem.Name = "неВыдаватьToolStripMenuItem";
            this.неВыдаватьToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.неВыдаватьToolStripMenuItem.Text = "Смотрю фильм";
            this.неВыдаватьToolStripMenuItem.Click += new System.EventHandler(this.неВыдаватьToolStripMenuItem_Click);
            // 
            // игровойToolStripMenuItem
            // 
            this.игровойToolStripMenuItem.Name = "игровойToolStripMenuItem";
            this.игровойToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.игровойToolStripMenuItem.Text = "Игровой";
            this.игровойToolStripMenuItem.Click += new System.EventHandler(this.игровойToolStripMenuItem_Click);
            // 
            // неБеспокоитьВообщеToolStripMenuItem
            // 
            this.неБеспокоитьВообщеToolStripMenuItem.Name = "неБеспокоитьВообщеToolStripMenuItem";
            this.неБеспокоитьВообщеToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.неБеспокоитьВообщеToolStripMenuItem.Text = "Не беспокоить";
            this.неБеспокоитьВообщеToolStripMenuItem.Click += new System.EventHandler(this.неБеспокоитьВообщеToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(212, 6);
            // 
            // отключитьНапоминанияToolStripMenuItem
            // 
            this.отключитьНапоминанияToolStripMenuItem.Name = "отключитьНапоминанияToolStripMenuItem";
            this.отключитьНапоминанияToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.отключитьНапоминанияToolStripMenuItem.Text = "Отключить напоминания";
            this.отключитьНапоминанияToolStripMenuItem.Click += new System.EventHandler(this.отключитьНапоминанияToolStripMenuItem_Click);
            // 
            // толькоСиренаToolStripMenuItem
            // 
            this.толькоСиренаToolStripMenuItem.Name = "толькоСиренаToolStripMenuItem";
            this.толькоСиренаToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.толькоСиренаToolStripMenuItem.Text = "Только сирена";
            this.толькоСиренаToolStripMenuItem.Click += new System.EventHandler(this.толькоСиренаToolStripMenuItem_Click);
            // 
            // сокращённаяСиренаToolStripMenuItem
            // 
            this.сокращённаяСиренаToolStripMenuItem.Name = "сокращённаяСиренаToolStripMenuItem";
            this.сокращённаяСиренаToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.сокращённаяСиренаToolStripMenuItem.Text = "Сокращённая сирена";
            this.сокращённаяСиренаToolStripMenuItem.Click += new System.EventHandler(this.сокращённаяСиренаToolStripMenuItem_Click);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 18);
            this.label1.TabIndex = 1;
            this.label1.UseWaitCursor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.убратьМенюToolStripMenuItem,
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(294, 48);
            // 
            // убратьМенюToolStripMenuItem
            // 
            this.убратьМенюToolStripMenuItem.Name = "убратьМенюToolStripMenuItem";
            this.убратьМенюToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.убратьМенюToolStripMenuItem.Text = "Убрать меню";
            // 
            // закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem
            // 
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem.Name = "закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem";
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem.Text = "Закрыть (клавиши Esc, ctrl, alt или shift)";
            this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem.Click += new System.EventHandler(this.закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem_Click);
            // 
            // создатьМногоФайловзадержкаБезГарантийToolStripMenuItem
            // 
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem.Name = "создатьМногоФайловзадержкаБезГарантийToolStripMenuItem";
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem.Size = new System.Drawing.Size(423, 22);
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem.Text = "Создать много файлов (без гарантий; задержка)";
            this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem.Click += new System.EventHandler(this.создатьМногоФайловзадержкаБезГарантийToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(209, 143);
            this.Controls.Add(this.label1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Leave += new System.EventHandler(this.Form1_Leave);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.testsMenu.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip testsMenu;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        public System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem режимToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem быстрыйToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem медленныйToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem тестыВниманияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem экспресстест2ЧислаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem справкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem неВыдаватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem неБеспокоитьВообщеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem медленныйРучнойToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem отключитьНапоминанияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tasksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem планироватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem замедленныйToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem красночёрнаяТаблицаToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem убратьМенюToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem закрытьклавишиEscCtrlAltИлиShiftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem шифрованиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem генерироватьПарольToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem шифроватьФайлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem толькоСиренаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem игровойToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сокращённаяСиренаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem вычислениеХэшейToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem расшифроватьФайлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалениеФайлаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьФайлМногопроходноToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьФайлТремяПроходамиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьФайлОднимПроходомToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьДиректориюToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьДиректориюТремяПроходамиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьДиректориюОднимПроходомToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem создатьНаДискеБольшойФайлToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem создатьБольшойФайлТриПроходаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem создатьБольшойФайлОдинПроходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem удалитьФайлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem контрольНеизменностиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem проконтролироватьНеизменностьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сравнитьХешиДляКонтроляToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem создатьМногоФайловзадержкаБезГарантийToolStripMenuItem;
    }
}

