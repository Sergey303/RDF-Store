using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RDFCommon
{

    /// <summary>
    /// �������� ���������� ������ �� ���������� ������������ .sln, ����� ������� �� ini ����� � �� ���������� ��� ������� ����� ������� <code>Load(string[] args=null)</code>
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// ���� � ���������� � ttl ������
        /// </summary>
        private static string Source_data_folder_path
        {
            get
            {
                if (!parameters["source_data_folder_path"].EndsWith("\\") &&
                    !parameters["source_data_folder_path"].EndsWith("/"))
                    parameters["source_data_folder_path"] += "/";
                return parameters["source_data_folder_path"];
            }
            set => parameters["source_data_folder_path"] = value;
        }

        /// <summary>
        /// ���� � ttl �����
        /// </summary>
        public static string TurtleFileFullPath
        {
            get
            {
                var v = parameters["ttl file path"];
                //�������� ������� ����� ttl �� ��� ����������� ����
                if (File.Exists(v)) return v;
                // ���� ��� ���, �� ����� �� ���� TurtleFileFullPath ������������ Source_data_folder_path
                //if (File.Exists(Source_data_folder_path + TurtleFileFullPath)) 
                TurtleFileFullPath = v = Source_data_folder_path + v;
                return v;
            }
            set => parameters["ttl file path"] = value;
        }

        /// <summary>
        /// ���� � ������ ���� ������
        /// </summary>
        public static string DatabaseFolder
        {
            get
            {
                if (!parameters["data base"].EndsWith("\\") &&
                    !parameters["data base"].EndsWith("/"))
                    parameters["data base"] += "/";
                return parameters["data base"] + Random.Next() + "\\";
            }
            set => parameters["data base"] = value;
        }

        /// <summary>
        /// ������� ���: ��� ��������� -> ��������.
        /// ������� �������� �� ���������
        /// ����� ��� ��������������� �� ini �����, ����� ����������� ��� �������
        /// </summary>
        private static Dictionary<string, string> parameters = new Dictionary<string, string>() {{"source_data_folder_path", "ConsoleSparqlCore\\examples\\" }, {"ttl file path", "simplest.ttl" }, {"data base", "/Databases/" } };

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

        private static readonly Random Random = new Random();

        /// <summary>
        /// ������� ��������� ����� �������� �� ���������, ����� ����������� �� ini �����, ���� �� ����, ����� ����������� ��������� ��� ������� �� <code>args</code>
        /// </summary>
        /// <param name="args">��������� ����������� ��� ������� ���������</param>
        public static void Load(string[] args=null)
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
                        Console.WriteLine("-dtll source turtle data folder full path were .ttl file, by default ConsoleSparqlCore\\examples\\ relative .sln");
                        Console.WriteLine("-tll turtle file path full or relative dtll");
                        Console.WriteLine("-db database folder path full or relative .sln");
                    }
                }
            }
            
        }
    }
}