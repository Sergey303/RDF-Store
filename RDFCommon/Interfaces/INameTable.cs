using System.Collections.Generic;

namespace RDFCommon.Interfaces
{
    public interface INametable
    {
        void Clear();
       // int Capacity { get; set; }
        int GetCode(string s);
        string GetString(int c);
        int GetSetCode(string s);
    //    void Expand(int length_estimation, IEnumerable<string> keyflow);
       // void Save();
     //  void FreeMemory();
      //  IGetDictionaryLong<string, int> InsertPortion(IEnumerable<string> unsorted);
        Dictionary<string, int> InsertPortion(IEnumerable<string> unsortedNames);
    }
}
