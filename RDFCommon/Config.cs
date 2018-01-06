using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RDFCommon
{
    public static class Config
    {
        /// <summary>
        /// ���� � ���������� � ttl ������
        /// </summary>
        private static string Source_data_folder_path
        {
            get => parameters["source_data_folder_path"];
            set => parameters["source_data_folder_path"] = value;
        }
        /// <summary>
        /// ���� � ttl �����
        /// </summary>
        public static string TurtleFileFullPath
        {
            get => parameters["ttl file full path"];
            set => parameters["ttl file full path"] = value;
        }
        /// <summary>
        /// ���� � ������ ���� ������
        /// </summary>
        public static string DatabaseFolder
        {
            get => parameters["data base"];
            set => parameters["data base"] = value;
        }
        /// <summary>
        /// ������� ���: ��� ��������� -> ��������.
        /// ������� �������� �� ���������
        /// ����� ��� ��������������� �� ini �����, ����� ����������� ��� �������
        /// </summary>
        private static Dictionary<string, string> parameters = new Dictionary<string, string>() {{"source_data_folder_path", "examples\\" }, {"ttl file name", "1M.ttl"}, {"data base", "../Databases/" } };

        /// <summary>
        /// ������� ��������� ���� config.ini, ���� �������, �� �� ���� ������� ��������� � ������������ � <code>this.parameters</code>
        /// </summary>
        /// <returns><code>true</code> - ���� ������
        ///             <code>false</code> - �� ������ 
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
        /// ������� ��� ��������� � ������ ini  ����� ����� ��������� � ��������
        /// </summary>
        private static readonly char[] splitChars = new[] {'#', '=', ' ', '\n', '\r'};

        /// <summary>
        /// ������� ��������� ����� �������� �� ���������, ����� ����������� �� ini �����, ���� �� ����, ����� ����������� ��������� ��� ������� �� <code>args</code>
        /// </summary>
        /// <param name="args">��������� ����������� ��� ������� ���������</param>
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
            //�������� ������� ����� ttl �� ��� ����������� ����
            if (File.Exists(TurtleFileFullPath)) return;
            // ���� ��� ���, �� ����� �� ���� TurtleFileFullPath ������������ Source_data_folder_path
            if (File.Exists(Source_data_folder_path + TurtleFileFullPath))
                TurtleFileFullPath = Source_data_folder_path + TurtleFileFullPath;
            else
            {
                throw new FileNotFoundException(TurtleFileFullPath);
            }
        }
    }
}