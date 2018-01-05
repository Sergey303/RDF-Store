using RDFCommon.long_dictionary;

namespace RDFCommon.Interfaces
{
    public interface IGetDictionaryLong<TKey, TValue>
    {
        TValue this[TKey key] { get;}

        IGetLongStream<TKey> Keys { get; }
        IGetLongStream<TValue> Values { get; }

        bool ContainsKey(TKey key);
        KeyValueHash<TKey, TValue, ulong> TryGetValue(KeyValueHash<TKey, TValue, ulong> key);
        bool TryGetValue(TKey key, out TValue value);
        void FreeMemory();
        ulong Count { get; }
    }
}