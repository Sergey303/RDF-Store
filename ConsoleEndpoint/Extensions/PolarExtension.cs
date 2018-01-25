using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Polar.DB;

namespace ConsoleEndpoint
{
    using Polar.CellIndexes;

    public static class PolarExtension
    {
        /// <summary>
        /// Рекурсивно создаёт PType в соответсвии с указанынм типом-аргументом:
        /// int -> PTypeEnumeration.integer
        /// int -> PTypeEnumeration.integer
        /// bool -> PTypeEnumeration.boolean
        /// double || float -> PTypeEnumeration.real
        /// string -> PTypeEnumeration.sstring
        /// char[] -> PTypeEnumeration.fstring
        /// byte -> PTypeEnumeration.@byte
        /// ISequence&lt;тип элемента &gt; -> PTypeSequence(GetPolarType( тип элемента ))
        /// IUnion&lt;тип элемента &gt; -> PTypeUnion(GetPolarType( тип элемента ), ...) элементы получают названия - названия типа. 
        /// Пустой вариант всегда добавляется.
        /// IRecord&lt;типы элементов &gt; -> PTypeRecord(GetPolarType( тип элемента ), ...) элементы получают названия - названия типа.
        /// Произвольный тип -> PTypeRecord. Свойства типа используются для элементов записи с такими же именами и типами.
        /// 
        /// Пример использования
        /// PType t= typeof(IRecord&lt;bool, IUnion&lt;string, int&gt;, string, ISequence&lt;IUnion&lt;string&gt;&gt;).GetPolarType();
        /// PolarExtension.GetPolarType&lt;IRecord&lt;bool, IUnion&lt;string, int&gt;, string, ISequence&lt;IUnion&lt;string&gt;&gt;&gt;();
        /// Не может создать рекурсивные типы, вложенные в себя, например, t= IUnion &lt; t &gt;
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PType GetPolarType(this Type type)
        {
            
            if (type == typeof(int))
                return new PType(PTypeEnumeration.integer);
            if (type == typeof(long))
                return new PType(PTypeEnumeration.longinteger);
            if (type == typeof(bool))
                return new PType(PTypeEnumeration.boolean);
            if (type == typeof(double) || type == typeof(float))
                return new PType(PTypeEnumeration.real);
            if (type == typeof(string))
                return new PType(PTypeEnumeration.sstring);
            if (type == typeof(char[]))
                return new PType(PTypeEnumeration.fstring);
            if (type == typeof(byte))
                return new PType(PTypeEnumeration.@byte);

            if (typeof(ISequence<>).IsAssignableFrom(type))
            {
                var genericArguments = type.GetGenericArguments();
                var elementType = genericArguments[0];
                return new PTypeSequence(GetPolarType(elementType));
            }

            if (typeof(IRecord).IsAssignableFrom(type))
            {
                // Record<аргументы>
                var genericArguments = type.GetGenericArguments();
                NamedType[] namedTypes = new NamedType[genericArguments.Length];
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    namedTypes[i] = new NamedType(genericArguments[i].Name, GetPolarType(genericArguments[i]));
                }

                return new PTypeRecord(namedTypes);
            }

            if (typeof(IUnion).IsAssignableFrom(type))
            {
                // Union<аргументы>
                var genericArguments = type.GetGenericArguments();
                NamedType[] namedTypes = new NamedType[genericArguments.Length + 1];
                namedTypes[0] = new NamedType("empty", new PType(PTypeEnumeration.none));
                for (var i = 1; i < genericArguments.Length + 1; i++)
                {
                    namedTypes[i] = new NamedType(genericArguments[i - 1].Name, GetPolarType(genericArguments[i - 1]));
                }

                return new PTypeUnion(namedTypes);
            }

            // произвольный тип - Record. Используем его свойства.
            return new PTypeRecord(
                type.GetProperties()
                    .Select(property => new NamedType(property.Name, GetPolarType(property.PropertyType))).ToArray());
        }

        /// <summary>
        /// Рекурсивно создаёт PType в соответсвии с указанынм типом-аргументом:
        /// int -> PTypeEnumeration.integer
        /// int -> PTypeEnumeration.integer
        /// bool -> PTypeEnumeration.boolean
        /// double || float -> PTypeEnumeration.real
        /// string -> PTypeEnumeration.sstring
        /// char[] -> PTypeEnumeration.fstring
        /// byte -> PTypeEnumeration.@byte
        /// ISequence&lt;тип элемента &gt; -> PTypeSequence(GetPolarType( тип элемента ))
        /// IUnion&lt;тип элемента &gt; -> PTypeUnion(GetPolarType( тип элемента ), ...) элементы получают названия - названия типа. 
        /// Пустой вариант всегда добавляется.
        /// IRecord&lt;типы элементов &gt; -> PTypeRecord(GetPolarType( тип элемента ), ...) элементы получают названия - названия типа.
        /// Произвольный тип -> PTypeRecord. Свойства типа используются для элементов записи с такими же именами и типами.
        /// 
        /// Пример использования
        /// PType t= typeof(IRecord&lt;bool, IUnion&lt;string, int&gt;, string, ISequence&lt;IUnion&lt;string&gt;&gt;).GetPolarType();
        /// PolarExtension.GetPolarType&lt;IRecord&lt;bool, IUnion&lt;string, int&gt;, string, ISequence&lt;IUnion&lt;string&gt;&gt;&gt;();
        /// Не может создать рекурсивные типы, вложенные в себя, например, t= IUnion &lt; t &gt;
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PType GetPolarType<T>()
        {
            return GetPolarType(typeof(T));
        }

        /// <summary>
        /// Вызывает GetAllByLevel, отсеивает удалённые строки где row[0] = false и возвращает содержимое row[1].
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="IndexImmut"></typeparam>
        /// <param name="index"></param>
        /// <param name="levelFunc">Функция применяется к содержимому таблицы row[1] </param>
        /// <returns></returns>
        public static IEnumerable<object[]> GetAllUndeletedByLevel<Tkey, IndexImmut>(
            this IndexDynamic<Tkey, IndexImmut> index,
            Func<object[], int> levelFunc)
            where IndexImmut : IIndexImmutable<Tkey> =>
            index.GetAllByLevel(ent => levelFunc(ent.Get().CastRow<object, object[]>().Item2))
                .Select(ent => (object[])ent.Get())
                .Where(row => !(bool)row[0])
                .Select(row => (object[])row[1]);
    
    // public static Type Get(PType pt)
        // {
        // switch (pt.Vid)
        // {
        // case PTypeEnumeration.none:
        // return null;
        // case PTypeEnumeration.boolean:
        // return typeof(bool);
        // case PTypeEnumeration.character:
        // return typeof(char);
        // case PTypeEnumeration.integer:
        // return typeof(int);
        // case PTypeEnumeration.longinteger:
        // return typeof(long);
        // case PTypeEnumeration.real:
        // return typeof(float);
        // case PTypeEnumeration.fstring:
        // return typeof(string);
        // case PTypeEnumeration.sstring:
        // return typeof(string);
        // case PTypeEnumeration.record:
        // return typeof(IEnumerable<>).MakeGenericType(Get(((PTypeSequence)pt).ElementType));

        // case PTypeEnumeration.sequence:
        // return typeof(IEnumerable<>).MakeGenericType(Get(((PTypeSequence)pt).ElementType));
        // case PTypeEnumeration.union:
        // return typeof(IEnumerable<>).MakeGenericType(Get(((PTypeSequence)pt).ElementType));
        // case PTypeEnumeration.@byte:
        // return typeof(byte);
        // case PTypeEnumeration.objPair:
        // return typeof(int);
        // default:
        // throw new ArgumentOutOfRangeException();
        // }
        // }

    }

    /// <summary>
    /// Интерфейс для указания записи в <code>GetPolarType</code>
    /// </summary>
    public interface IRecord
    {
    }

    /// <summary>
    /// Интерфейс для указания записи в <code>GetPolarType</code>
    /// </summary>
    public interface IRecord<T> : IRecord
    {
        // public T Item1;
    }

    /// <summary>
    /// Класс для указания записи в <code>GetPolarType</code>
    /// </summary>
    public interface IRecord<T1, T2> : IRecord<T1>
    {
        // public T2 Item2;
    }

    /// <summary>
    /// Класс для указания записи в <code>GetPolarType</code>
    /// </summary>
    public interface IRecord<T1, T2, T3> : IRecord<T1, T2>
    {
        // public T3 Item3;
    }

    /// <summary>
    /// Класс для указания записи в <code>GetPolarType</code>
    /// </summary>
    public interface IRecord<T1, T2, T3, T4> : IRecord<T1, T2, T3>
    {
        // public T4 Item4;
    }

    // todo t4...

    /// <summary>
    /// Интерфейс для указания альтернативных вариантов в <code>GetPolarType</code>
    /// </summary>
    public interface IUnion
    {
    }

    /// <summary>
    /// Интерфейс для указания альтернативных вариантов в <code>GetPolarType</code>
    /// </summary>
    public interface IUnion<T1> : IUnion
    {
    }

    /// <summary>
    /// Интерфейс для указания альтернативных вариантов в <code>GetPolarType</code>
    /// </summary>
    public interface IUnion<T1, T2> : IUnion
    {
    }

    /// <summary>
    /// Интерфейс для указания альтернативных вариантов в <code>GetPolarType</code>
    /// </summary>
    public interface IUnion<T1, T2, T3> : IUnion
    {
    }

    // todo t4...

    /// <summary>
    /// Интерфейс для указания последовательности в <code>GetPolarType</code>
    /// </summary>
    public interface ISequence<T>
    {
    }
}