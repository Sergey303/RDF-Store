using System;

namespace RDFCommon.Interfaces
{
    /// <summary>
    /// Альтернатива <see cref="System.Collections.IEnumerable{T}"/> 
    /// Применимо, где нельзя порождать поток.
    /// Содержит метод  Start.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenerator<T>
    {
        void Start(Action<T> onGenerate);
    }
}