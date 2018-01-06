using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RDFCommon
{
    public static class Config
    {
        /// <summary>
        /// Путь к директории с ttl файлом
        /// </summary>
        private static string Source_data_folder_path
        {
            get => parameters["source_data_folder_path"];
            set => parameters["source_data_folder_path"] = value;
        }
        /// <summary>
        /// путь к ttl файлу
        /// </summary>
        public static string TurtleFileFullPath
        {
            get => parameters["ttl file full path"];
            set => parameters["ttl file full path"] = value;
        }
        /// <summary>
        /// путь к файлам базы данных
        /// </summary>
        public static string DatabaseFolder
        {
            get => parameters["data base"];
            set => parameters["data base"] = value;
        }
        /// <summary>
        /// словарь пар: имя параметра -> значение.
        /// Указаны значения по умолчанию
        /// затем они переназначаются из ini файла, затем параметрами при запуске
        /// </summary>
        private static Dictionary<string, string> parameters = new Dictionary<string, string>() {{"source_data_folder_path", "examples\\" }, {"ttl file name", "1M.ttl"}, {"data base", "../Databases/" } };

        /// <summary>
        /// Попытка загрузить файл config.ini, если успешно, то из него берутся параметры и записываются в <code>this.parameters</code>
        /// </summary>
        /// <returns><code>true</code> - файл найден
        ///             <code>false</code> - не найден 
        /// </returns>
        public static bool TryLoadIni()
        {
            try
            {
                using (StreamReader file = new StreamReader("config.ini"))
                {
                    while (!file.EndOfStream)
                    {
                        var readLine = file.ReadLine();
                        if (string.IsNullOrWhiteSpace(readLine)) continue;

                        var pnameValue = readLine.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        var pname = pnameValue[0];
                        var pValue = pnameValue[1];
                        //if (parameters.TryGetValue(pname, out var existsValue) && //    !string.IsNullOrWhiteSpace(existsValue)) continue;
                        if (parameters.ContainsKey(pname))
                        {
                            parameters[pname] = pValue;
                        }
                        else
                        {
                            parameters.Add(pname, pValue);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("trying load ini file failed");
                Console.WriteLine(e);
                return false;
            }
            return false;
        }
        /// <summary>
        /// символы для выделения в строке ini  файла имени параметра и значения
        /// </summary>
        private static readonly char[] splitChars = new[] {'#', '=', ' ', '\n', '\r'};

        /// <summary>
        /// Сначала параметры имеют значения по умолчанию, затем назначаются из ini файла, если он есть, затем назначаются указанные при запуске из <code>args</code>
        /// </summary>
        /// <param name="args">параметры указываемые при запуске программы</param>
        public static void Load(string[] args)
        {
            TryLoadIni();
            if (args != null)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    args[i] = args[i].ToLower();
                    if (args[i] == "-db")
                    {
                        DatabaseFolder= args[i + 1];
                    }
                    if (args[i] == "-dtll")
                    {
                        Source_data_folder_path = args[i + 1];
                    }
                    if (args[i] == "-tll")
                    {
                        TurtleFileFullPath = args[i+1];
                    }
                    if (args[i] == "-h")
                    {
                        Console.WriteLine("-dtll source turtle data folder full path were .ttl file. Default project path");
                        Console.WriteLine("-tll turtle file path full or relative dtll");
                        Console.WriteLine("-db database folder path full or relative project");
                    }
                }
            }
            
            if (!Source_data_folder_path.EndsWith("\\") &&
                !Source_data_folder_path.EndsWith("/"))
                Source_data_folder_path += "/";
            //проверка наличия файла ttl по уже указнанному пути
            if (File.Exists(TurtleFileFullPath)) return;
            // если его нет, то поиск по пути TurtleFileFullPath относительно Source_data_folder_path
            if (File.Exists(Source_data_folder_path + TurtleFileFullPath))
                TurtleFileFullPath = Source_data_folder_path + TurtleFileFullPath;
            else
            {
                throw new FileNotFoundException(TurtleFileFullPath);
            }
        }
    }
}