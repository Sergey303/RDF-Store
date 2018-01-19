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
        /// Создаёт PType по типу-параметру.
        /// Тип параметер <code>ISequence<ElementType> </code> созает последовательность <code> PTypeSequence </code>
        /// Тип параметер <code>IUnion<SubType1, ...> </code> созает альтернативные варианты <code> PTypeUnion </code>
        /// Тип параметер <code>IRecord<SubType1, ...> </code> или класс, не соответсвующий ни одному поляровскому типу созают запись <code> PTypeRecord</code>
        /// В последнем случае используются типы и имена свойств класса.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PType GetPolarType<T>()
        {
            return GetPolarType(typeof(T));
        }

        #region Cast

        /// <summary>
        /// обычное небезопасное приведение, просто чтобы избедать множества скобок в коде.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T CastFromOneRow<T>(this object o)
        {
            var type = typeof(T);
            if (type.IsInstanceOfType(o) || o.GetType().IsAssignableFrom(type))
                return (T)o;
            if (type.IsEnum)
                return (T)Enum.ToObject(type, o);

            if (o is object[] row)
            {
                Type[] genericArguments;
                if (typeof(ITuple).IsAssignableFrom(type))
                    genericArguments = type.GetGenericArguments();
                else
                    genericArguments = type.GetProperties().Select(p => p.PropertyType).ToArray();
                // if (genericArguments.Length == row.Length)
                {
                    return (T)type.GetConstructor(genericArguments).Invoke(
                        row.Select(
                            (o1, i) => typeof(PolarExtension).GetMethod("CastFromOneRow")
                                .MakeGenericMethod(genericArguments[i]).Invoke(null, new[] { row[i] })).ToArray());

                    // if (row.Length != genericArguments.Length) throw new Exception("4534534");
                }
            }

            return (T)Convert.ChangeType(o, type);
        }

        /// <summary>
        /// К каждому объектьу в потоке применяет <code>CastRow &lt; T1&gt;()</code>, 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="flow">поток массивов, приведенных к типу <code>object</code></param>
        /// <returns></returns>
        public static IEnumerable<T1> CastAllRows<T1>(this IEnumerable<object> flow) =>
            flow.Select(o => o.CastFromOneRow<T1>());

        /// <summary>
        /// К каждому объектьу в потоке применяет <code>CastRow &lt; T1, T2&gt;()</code>, 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="flow">поток массивов, приведенных к типу <code>object</code></param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2)> CastAllRows<T1, T2>(this IEnumerable<object> flow) =>
            flow.Select(o => o.CastRow<T1, T2>());

        /// <summary>
        /// К каждому объектьу в потоке применяет <code> CastRow&lt;T1, T2, T3&gt;()</code>, 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="flow">поток массивов, приведенных к типу <code>object</code></param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2, T3)> CastAllRows<T1, T2, T3>(this IEnumerable<object> flow) =>
            flow.Select(o => o.CastRow<T1, T2, T3>());

        /// <summary>
        /// К каждому объектьу в потоке применяет <code> CastRow&lt;T1, T2, T3, T4&gt;()</code>, 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="flow">поток массивов, приведенных к типу <code>object</code></param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2, T3, T4)> CastAllRows<T1, T2, T3, T4>(this IEnumerable<object> flow) =>
            flow.Select(o => o.CastRow<T1, T2, T3, T4>());

        /// <summary>
        /// К каждому объектьу в потоке применяет <code> CastRow&lt;T1, T2, T3, T4, T5&gt;()</code>, 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="flow">поток массивов, приведенных к типу <code>object</code></param>
        /// <returns></returns>
        public static IEnumerable<(T1, T2, T3, T4, T5)>
            CastAllRows<T1, T2, T3, T4, T5>(this IEnumerable<object> flow) =>
            flow.Select(o => o.CastRow<T1, T2, T3, T4, T5>());

        /// <summary>
        /// Превращает объект-массив объектов в кортеж, где элементы кортежа соответсвуют элементам массива.
        /// </summary>
        /// <typeparam name="T1">тип первого элемента кортежа</typeparam>
        /// <typeparam name="T2">тип второго элемента кортежа</typeparam>
        /// <param name="o">массив объектов</param>
        /// <returns>кортеж из элементов массива</returns>
        public static (T1, T2) CastRow<T1, T2>(this object o)
        {
            var row = (object[])o;
            return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>());
        }

        public static void Deconstruct(this object[] array, out object element1)
        {
            element1 = array[0];
        }

        public static void Deconstruct(this object[] array, out object element1, out object element2)
        {
            element1 = array[0];
            element2 = array[1];
        }

        public static void Deconstruct(
            this object[] array,
            out object element1,
            out object element2,
            out object element3)
        {
            element1 = array[0];
            element2 = array[1];
            element3 = array[2];
        }

        public static void Deconstruct(
            this object[] array,
            out object element1,
            out object element2,
            out object element3,
            out object element4)
        {
            element1 = array[0];
            element2 = array[1];
            element3 = array[2];
            element4 = array[3];
        }

        public static void Deconstruct(
            this object[] array,
            out object element1,
            out object element2,
            out object element3,
            out object element4,
            out object element5)
        {
            element1 = array[0];
            element2 = array[1];
            element3 = array[2];
            element4 = array[3];
            element5 = array[4];
        }

        /// <summary>
        /// Превращает объект-массив объектов в кортеж, где элементы кортежа соответсвуют элементам массива.
        /// </summary>
        /// <typeparam name="T1">тип первого элемента кортежа</typeparam>
        /// <typeparam name="T2">тип второго элемента кортежа</typeparam>
        /// <typeparam name="T3">тип третьего элемента кортежа</typeparam>
        /// <param name="o">массив объектов</param>
        /// <returns>кортеж из элементов массива</returns>
        public static (T1, T2, T3) CastRow<T1, T2, T3>(this object o)
        {
            var row = (object[])o;
            return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>());
        }

        /// <summary>
        /// Превращает объект-массив объектов в кортеж, где элементы кортежа соответсвуют элементам массива.
        /// </summary>
        /// <typeparam name="T1">тип первого элемента кортежа</typeparam>
        /// <typeparam name="T2">тип второго элемента кортежа</typeparam>
        /// <typeparam name="T3">тип третьего элемента кортежа</typeparam>
        /// <typeparam name="T4">тип четвертого элемента кортежа</typeparam>
        /// <param name="o">массив объектов</param>
        /// <returns>кортеж из элементов массива</returns>
        public static (T1, T2, T3, T4) CastRow<T1, T2, T3, T4>(this object o)
        {
            var row = (object[])o;
            return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>(), row[3]
                .CastFromOneRow<T4>());
        }

        /// <summary>
        /// Превращает объект-массив объектов в кортеж, где элементы кортежа соответсвуют элементам массива.
        /// </summary>
        /// <typeparam name="T1">тип первого элемента кортежа</typeparam>
        /// <typeparam name="T2">тип второго элемента кортежа</typeparam>
        /// <typeparam name="T3">тип третьего элемента кортежа</typeparam>
        /// <typeparam name="T4">тип четвертого элемента кортежа</typeparam>
        /// <typeparam name="T5">тип пятого элемента кортежа</typeparam>
        /// <param name="o">массив объектов</param>
        /// <returns>кортеж из элементов массива</returns>
        public static (T1, T2, T3, T4, T5) CastRow<T1, T2, T3, T4, T5>(this object o)
        {
            var row = (object[])o;
            return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>(),
                row[3].CastFromOneRow<T4>(), row[4].CastFromOneRow<T5>());
        }

        #endregion

        public static IEnumerable<object[]> GetAllUndeletedByLevel<Tkey, IndexImmut>(
            this IndexDynamic<Tkey, IndexImmut> index,
            Func<object[], int> levelFunc)
            where IndexImmut : IIndexImmutable<Tkey> =>
            index.GetAllByLevel(ent => levelFunc((object[])ent.Get()))
                .Select(ent => (object[])ent.Get())
                .Where(row => (bool)row[0])
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