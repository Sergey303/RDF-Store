using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConsoleEndpoint;

static internal class CastFromArrayObjectsExtension
{
    /// <summary>
    /// ������� ������������ ����������, ������ ����� �������� ��������� ������ � ����.
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
                        (o1, i) => typeof(CastFromArrayObjectsExtension).GetMethod("CastFromOneRow")
                            .MakeGenericMethod(genericArguments[i]).Invoke(null, new[] { row[i] })).ToArray());

                // if (row.Length != genericArguments.Length) throw new Exception("4534534");
            }
        }

        return (T)Convert.ChangeType(o, type);
    }

    /// <summary>
    /// � ������� �������� � ������ ��������� <code>CastRow &lt; T1&gt;()</code>, 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="flow">����� ��������, ����������� � ���� <code>object</code></param>
    /// <returns></returns>
    public static IEnumerable<T1> CastAllRows<T1>(this IEnumerable<object> flow) =>
        flow.Select(o => o.CastFromOneRow<T1>());

    /// <summary>
    /// � ������� �������� � ������ ��������� <code>CastRow &lt; T1, T2&gt;()</code>, 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="flow">����� ��������, ����������� � ���� <code>object</code></param>
    /// <returns></returns>
    public static IEnumerable<(T1, T2)> CastAllRows<T1, T2>(this IEnumerable<object> flow) =>
        flow.Select<object, (T1, T2)>(o => o.CastRow<T1, T2>());

    /// <summary>
    /// � ������� �������� � ������ ��������� <code> CastRow&lt;T1, T2, T3&gt;()</code>, 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="flow">����� ��������, ����������� � ���� <code>object</code></param>
    /// <returns></returns>
    public static IEnumerable<(T1, T2, T3)> CastAllRows<T1, T2, T3>(this IEnumerable<object> flow) =>
        flow.Select<object, (T1, T2, T3)>(o => o.CastRow<T1, T2, T3>());

    /// <summary>
    /// � ������� �������� � ������ ��������� <code> CastRow&lt;T1, T2, T3, T4&gt;()</code>, 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="flow">����� ��������, ����������� � ���� <code>object</code></param>
    /// <returns></returns>
    public static IEnumerable<(T1, T2, T3, T4)> CastAllRows<T1, T2, T3, T4>(this IEnumerable<object> flow) =>
        flow.Select<object, (T1, T2, T3, T4)>(o => o.CastRow<T1, T2, T3, T4>());

    /// <summary>
    /// � ������� �������� � ������ ��������� <code> CastRow&lt;T1, T2, T3, T4, T5&gt;()</code>, 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="flow">����� ��������, ����������� � ���� <code>object</code></param>
    /// <returns></returns>
    public static IEnumerable<(T1, T2, T3, T4, T5)>
        CastAllRows<T1, T2, T3, T4, T5>(this IEnumerable<object> flow) =>
        flow.Select<object, (T1, T2, T3, T4, T5)>(o => o.CastRow<T1, T2, T3, T4, T5>());

    /// <summary>
    /// ���������� ������-������ �������� � ������, ��� �������� ������� ������������ ��������� �������.
    /// </summary>
    /// <typeparam name="T1">��� ������� �������� �������</typeparam>
    /// <typeparam name="T2">��� ������� �������� �������</typeparam>
    /// <param name="o">������ ��������</param>
    /// <returns>������ �� ��������� �������</returns>
    public static (T1, T2) CastRow<T1, T2>(this object o)
    {
        var row = (object[])o;
        return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>());
    }

    /// <summary>
    /// ���������� ������-������ �������� � ������, ��� �������� ������� ������������ ��������� �������.
    /// </summary>
    /// <typeparam name="T1">��� ������� �������� �������</typeparam>
    /// <typeparam name="T2">��� ������� �������� �������</typeparam>
    /// <typeparam name="T3">��� �������� �������� �������</typeparam>
    /// <param name="o">������ ��������</param>
    /// <returns>������ �� ��������� �������</returns>
    public static (T1, T2, T3) CastRow<T1, T2, T3>(this object o)
    {
        var row = (object[])o;
        return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>());
    }

    /// <summary>
    /// ���������� ������-������ �������� � ������, ��� �������� ������� ������������ ��������� �������.
    /// </summary>
    /// <typeparam name="T1">��� ������� �������� �������</typeparam>
    /// <typeparam name="T2">��� ������� �������� �������</typeparam>
    /// <typeparam name="T3">��� �������� �������� �������</typeparam>
    /// <typeparam name="T4">��� ���������� �������� �������</typeparam>
    /// <param name="o">������ ��������</param>
    /// <returns>������ �� ��������� �������</returns>
    public static (T1, T2, T3, T4) CastRow<T1, T2, T3, T4>(this object o)
    {
        var row = (object[])o;
        return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>(), row[3]
            .CastFromOneRow<T4>());
    }

    /// <summary>
    /// ���������� ������-������ �������� � ������, ��� �������� ������� ������������ ��������� �������.
    /// </summary>
    /// <typeparam name="T1">��� ������� �������� �������</typeparam>
    /// <typeparam name="T2">��� ������� �������� �������</typeparam>
    /// <typeparam name="T3">��� �������� �������� �������</typeparam>
    /// <typeparam name="T4">��� ���������� �������� �������</typeparam>
    /// <typeparam name="T5">��� ������ �������� �������</typeparam>
    /// <param name="o">������ ��������</param>
    /// <returns>������ �� ��������� �������</returns>
    public static (T1, T2, T3, T4, T5) CastRow<T1, T2, T3, T4, T5>(this object o)
    {
        var row = (object[])o;
        return (row[0].CastFromOneRow<T1>(), row[1].CastFromOneRow<T2>(), row[2].CastFromOneRow<T3>(),
            row[3].CastFromOneRow<T4>(), row[4].CastFromOneRow<T5>());
    }
}