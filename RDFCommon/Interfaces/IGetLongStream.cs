using System.Collections.Generic;
using System.IO;

namespace RDFCommon.Interfaces
{
    public interface IGetLongStream<T> : IEnumerable<T>
    {
        ulong Read(T[] buffer, int offset, int  count);
        T Read();
        ulong Seek(ulong offset, SeekOrigin loc);
        ulong Length { get; }
        ulong Position { get; set; }
        ulong Count { get; }
        T this[ulong index] { get; set; }
        bool Contains(T item);
        ulong IndexOf(T item);
        void FreeMemory();
        IGetLongStream<T> Copy();
    }
}