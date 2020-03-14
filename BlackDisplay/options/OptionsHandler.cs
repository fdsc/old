using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace options
{
    public class OptionsHandler
    {
        public class OptionsData<T>: ICloneable
        {
            public string name;
            public string comment;

            public object rawData;
            public T data
            {
                get
                {
                    return (T) rawData;
                }
                set
                {
                    rawData = value;
                }
            }

            public OptionsData(T data, string name): this(data, name, null)
            {
            }

            public OptionsData(T data, string name, string comment)
            {
                rawData         = data;
                this.name       = name;
                this.comment    = comment;
            }

            public object Clone()
            {
                return new OptionsData<T>(data, name, comment);
            }

            public static implicit operator OptionsData<string>(OptionsData<T> d)
            {
                if (d.rawData is string)
                    return new OptionsData<string>((string) d.rawData, d.name, d.comment);

                throw new Exception("Type convert to OptionsData<string> failed for option " + d.name);
            }

            public static implicit operator OptionsData<bool>(OptionsData<T> d)
            {
                if (d.rawData is bool)
                    return new OptionsData<bool>((bool) d.rawData, d.name, d.comment);

                throw new Exception("Type convert to OptionsData<bool> failed for option " + d.name);
            }

            public static implicit operator OptionsData<int>(OptionsData<T> d)
            {
                if (d.rawData is int)
                    return new OptionsData<int>((int) d.rawData, d.name, d.comment);

                throw new Exception("Type convert to OptionsData<int> failed for option " + d.name);
            }

            public static implicit operator OptionsData<object>(OptionsData<T> d)
            {
                return new OptionsData<object>((object) d.rawData, d.name, d.comment);
            }

            public override string ToString()
            {
                if (rawData is string)
                    return "string:" + name + ":" + (string) rawData;

                if (rawData is bool)
                    return "bool  :" + name + ":" + (  (bool) rawData ? "Да" : "Нет"  );

                if (rawData is int)
                    return "int   :" + name + ":" + rawData.ToString();

                throw new Exception("Не удалось распознать тип аргумента опции и привести его к String");
            }
        }

        public object this [string key]
        {
            get
            {
                return options[key].rawData;
            }
        }

        public bool this [string key, bool a]
        {
            get
            {
                var t = (OptionsData<bool>) options[key];
                return t.data;
            }
        }

        public string this [string key, string a]
        {
            get
            {
                var t = (OptionsData<string>) options[key];
                return t.data;
            }
        }

        public int this [string key, int a]
        {
            get
            {
                var t = (OptionsData<int>) options[key];
                return t.data;
            }
        }

        public bool contains(string key)
        {
            return options.ContainsKey(key);
        }

        public SortedDictionary<string, OptionsData<object>> options = new SortedDictionary<string, OptionsData<object>>();

        public OptionsHandler()
        {
        }

        public virtual void addHadnler(string optionName, OptionsData<object> option)
        {
        }

        public OptionsHandler(string FileName): this()
        {
            readFromFile(FileName);
        }

        private string prepareToAdd(string optionName, string comment)
        {
            string cmt = comment;
            if (options.ContainsKey(optionName))
            {
                if (cmt == "")
                    cmt = options[optionName].comment;

                remove(optionName);
            }
            return cmt;
        }

        public void add(string optionName, string optionValue, string comment = "")
        {
            string cmt = prepareToAdd(optionName, comment);

            var option = new OptionsData<String>(optionValue, optionName, cmt);
            options[optionName] = option;

            addHadnler(optionName, option);
        }

        public void add(string optionName, int optionValue, string comment = "")
        {
            string cmt = prepareToAdd(optionName, comment);

            var option = new OptionsData<int>(optionValue, optionName, cmt);
            options[optionName] = option;

            addHadnler(optionName, option);
        }

        public void add(string optionName, bool optionValue, string comment = "")
        {
            string cmt = prepareToAdd(optionName, comment);

            var option = new OptionsData<bool>(optionValue, optionName, cmt);
            options[optionName] = option;

            addHadnler(optionName, option);
        }

        public void addBool(string optionName, string optionValue, string comment = "")
        {
            var val = optionValue.Trim().ToLower();

            if (val == "да" || val == "yes" || val == "true")
                add(optionName, true, comment);
            else
            if (val == "нет" || val == "no" || val == "false")
                add(optionName, false, comment);
            else
                throw new Exception("Тип параметра " + optionName + " не может быть определён как bool");
        }

        public void addInt(string optionName, string optionValue, string comment = "")
        {
            add(optionName, Int32.Parse(optionValue), comment);
        }

        public void remove(string optionName)
        {
            if (options.ContainsKey(optionName))
                options.Remove(optionName);
        }

        public void clear()
        {
            options.Clear();
        }

        /// <summary>
        /// Читает из файла настройки без очистки существующих настроек
        /// </summary>
        public void readFromFile(string FileName)
        {
            if (!File.Exists(FileName))
                return;

            var lines = File.ReadAllLines(FileName);
            parse(lines);
        }

        public void readFromString(string text)
        {
            var    lines = new List<String>(8);
            var    sr    = new StringReader(text);
            string line  = sr.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = sr.ReadLine();
            }

            parse(lines.ToArray());
        }

        public bool saveExecute = false;
        private void parse(string[] lines)
        {
            var se = saveExecute;
            saveExecute = false;

            string comment = null;
            foreach (var line in lines)
            {
                var l = line.Trim();
                int i = l.IndexOf(":");
                int k = l.IndexOf(":", i + 1);
                if (l.StartsWith("//"))
                {
                    comment = l.Substring(2, l.Length - 2).Trim();
                    continue;
                }
                if (i < 0 || k < 0)
                    continue;

                var type = l.Substring(0, i).Trim();
                var name = l.Substring(i + 1, k - i - 1).Trim();
                var val = l.Substring(k + 1, l.Length - k - 1).Trim();

                if (type == "int")
                    addInt(name, val, comment);
                else
                    if (type == "bool")
                        addBool(name, val, comment);
                    else
                        if (type == "string")
                            add(name, val, comment);
                        else
                            throw new Exception("Неподдерживаемый тип " + type);

                comment = null;
            }

            saveExecute = se;
        }

        public string writeToString()
        {
            var sb = new StringBuilder(128);
            foreach (var val in options)
            {
                if (val.Value.comment != null)
                    sb.AppendLine("\r\n// " + val.Value.comment);

                sb.AppendLine(val.Value.ToString() + "\r\n");
            }

            return sb.ToString();
        }

        public void writeToFile(string FileName)
        {
            File.WriteAllText( FileName, writeToString() );
        }
    }
}
