using options;
namespace BlackDisplay
{
    public partial class Form1
    {
        public class RtdbOptions: options.OptionsHandler
        {
            public RtdbOptions(): base()
            {
                addOptions();
            }

            public RtdbOptions(string FileName): this()
            {
                readFromFile(FileName);
                this.remove(optsName[6]);

                if (this.contains(optsName[11]))
                {
                    if (this[optsName[11], 0] == -1)
                    {
                        this.add(optsName[19], -1);
                        this.add(optsName[20], -1);
                    }
                    else
                    {
                        if (this[optsName[11], 0] <= 2)
                        {
                            int time = this[optsName[11], 0] * 10;
                            if (time == 10)
                                time = 8;

                            this.add(optsName[19], time); // Переставляем старый режим в быстрый
                            this.add(optsName[21], 1);  // Устанавливаем как режим по-умолчанию быстрый режим
                        }
                        else
                            this.add(optsName[20], this[optsName[11], 0] * 10); // Переставляем старый быстрый режим в замедленный
                    }

                    this.remove(optsName[11]);                    
                }

                saveExecute = true;
            }

            public static readonly int LicenseVersion     = 20120115;
            public static readonly int lastNewFuncversion = 20120814;
            private void addOptions()
            {
                add(optsName[0],  60*15,             "(минут) Время перерыва со сбросом истории (после этого времени происходит начало нового отсчёта часов работы и отдыха, более давние события в расчёте не участвуют)");
                //add(optsName[6],  20,                "максимальное эффективное время отдыха (оператору необходимо отдыхать не раз в сутки, а не реже, чем каждые два часа; если отдых длительный (более данного времени), программа будет засчитывать его как менее эффективный; более удвоенного времени - не засчитывается)");
                add(optsName[1],  10,                "время отдыха, приходящееся на один час (минут)");
                add(optsName[2],  80,                "рабочий интервал: интервал времени, содержащий цикл работа-отдых (минут)");
                add(optsName[3],  true,              "нужно ли требовать ввод пароля после отдыха (требуется ли блокировка компьютера)?");
                add(optsName[4],  true,              "запускать при запуске системы?");
                add(optsName[5],  "./updator/updatorvs8.exe",  "путь к программе обновлений, оставьте поле пустым (string:6updatorpath:), если не хотите получать обновлений");
                add(optsName[7],  false,             "лицензия принята");
                add(optsName[9],  20111129,          "версия лицензии"); /* новая 20120115 */
                add(optsName[10], 15,                "минимальное время работы без напоминаний, мин. (при слишком большом пропуске отдыха напоминания выходят чаще, но не чаще, чем указано в этом параметре)");
                // add(optsName[11], 3,                 "(минут) время бездействия, после которого в быстром режиме идёт затемнение экрана (-1 - возможность отключена)");

                add(optsName[8],  false,             "сохранять заголовок активного окна в логе?");

                add(optsName[12], 12,                "(минут) переключение в быстрый режим реакции на бездействие - если активность более указанного числа минут, происходит автоматическое переключение с медленного режима на быстрый");
                add(optsName[13], 23,                "(секунд) переключение в быстрый режим реакции на бездействие - указанный перерыв в секундах считается неактивностью для отмены перехода в быстрый режим [действует совместно с настройкой 12TimeToFast]; при этом данный перерыв проверяется лишь раз в 5 секунд");
                add(optsName[14], true,              "В некоторых случаях фоновые процессы постоянно создают новые окна и процессы. Программа будет игнорировать появление этих окон");
                add(optsName[15], true,              "Не выдавать диалоги приложения в случае, если пользователь работает с topMost либо с полноэкранными приложениями");
                add(optsName[16], true,              "Выдавать сообщения о новой функциональности");
                add(optsName[17], lastNewFuncversion,"Дата последней функциональности, о которой было объявлено");
                add(optsName[18], false,             "Без кнопки \"Ни за что!\"");
                add(optsName[19], 8,                 "(десятых минуты) время бездействия, после которого в быстром режиме идёт затемнение экрана (-1 - возможность отключена)");
                add(optsName[20], 25,                "(десятых минуты) время бездействия, после которого в замедленном режиме идёт затемнение экрана (-1 - возможность отключена)");
                add(optsName[21], 2,                 "Номер режима по-умолчанию (1 - быстрый, 2 - замедленный, 3 - медленный, 4 - медленный ручной, 5 - смотрю фильм, 6 - не беспокоить)");
                add(optsName[22], false,             "Отключить напоминания по-умолчанию");
                add(optsName[23], 17,                "(минут) Аналогично опции "  + optsName[12] + ", но для замедленного режима");
                add(optsName[24], 75,                "(секунд) Аналогично опции " + optsName[13] + ", но для замедленного режима");
                add(optsName[25], true,              "Игнорировать небольшие передвижения мыши (не считать это свидетельством работы пользователя)");
                add(optsName[26], 50,                "(%; 0-100%) При бездействии экран зачерняется. При этом время отдыха может начинатся при затемнении экрана (0%), либо отойти в прошлое на указанный % времени бездействия");
                add(optsName[27], false,             "Режим 'Только сирена' по умолчанию");
                add(optsName[28], 50,                "Громкость сирены (1-100)");
                add(optsName[29], 0,                 "Проигрывать звук сирены одновременно с выдачей окна напоминания об отдыхе");
                add(optsName[30], 8,                 "Продолжительность сирены, если время отдыха не долгое");
                add(optsName[31], 16,                "Продолжительность сирены, если усталость большая");
                add(optsName[32], 1,                 "Ждать бездействия перед выдачей напоминания");
                add(optsName[33], 2,                 "Максимальное количество повторений сирены в режиме 'Сокращённая сирена'");
                add(optsName[34], false,             "При старте программы сразу же начинать собирать данные для генерации паролей");
            }

            public override void addHadnler(string optionName, OptionsData<object> option)
            {
                DbgLog.dbg.varToLog("RtdbOptions.addHadnler", "saveExecute", saveExecute);

                if (saveExecute)
                {
                    this.remove(optsName[6]);
                    saveOptionsToFile();
                }
            }
        }


        private void getTimes(out long maxRelaxTime, out long minFullRelaxInterval, out long relaxByHour, out long relaxEventInterval, out long RelaxTime, out long minShortWorkInterval)
        {
            // maxRelaxTime         = opts[optsName[ 6], 0] * minute;
            maxRelaxTime         = 0;
            minFullRelaxInterval = (long) opts[optsName[ 0], 0] * minute;
            relaxByHour          = (long) opts[optsName[ 1], 0] * minute;
            relaxEventInterval   = (long) opts[optsName[ 2], 0] * minute;
            minShortWorkInterval = (long) opts[optsName[10], 0] * minute;

            if (relaxByHour > hour)
                relaxByHour = hour;
            if (relaxByHour < minute)
                relaxByHour = minute;

            RelaxTime = (long) ((double) relaxByHour * (double) relaxEventInterval / (double) hour);
        }
    }
}
