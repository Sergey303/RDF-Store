using System.IO;

namespace RDFCommon
{
  public static class Config
    {
      public static readonly string Source_data_folder_path;

      static Config()
      {
          using (StreamReader file=new StreamReader("../config.ini"))
          {
              while (!file.EndOfStream)
              {
                   var readLine = file.ReadLine();
                  if (readLine.StartsWith("#source_data_folder_path"))
                      Source_data_folder_path = readLine.Replace("#source_data_folder_path","").Trim();
              }
              if (!Source_data_folder_path.EndsWith("\\") &&
                  !Source_data_folder_path.EndsWith("/")) 
                  Source_data_folder_path += "/";
          }
      }
    }
}
