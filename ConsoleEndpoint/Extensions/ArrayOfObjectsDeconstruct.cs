using ConsoleEndpoint;

static internal class ArrayOfObjectsDeconstruct
{
    /// <summary>
    /// ћектод  со спецаильной сигнатурой дл€ встроенного в C# 7 "синтаксического сахара"
    /// object[] someArray= new object[]{ 5, 7, "sddd"};
    /// ¬ следующей строчке вызываетс€ метод Deconstruct(someArray,  out object element1, out object element2, out object element3) 
    /// (object x, object y, object z) = someArray;  
    /// </summary>
    /// <param name="array"></param>
    /// <param name="element1"></param>
    /// <param name="element2"></param>
    public static void Deconstruct(this object[] array, out OVT element1, out object element2)
    {
        element1 = (OVT)array[0];
        element2 = array[1];
    }

    public static (T1, T2) Cast<T1, T2>(this (object, object) tuple)
    {
        return ((T1)tuple.Item1, (T2)tuple.Item2);
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
}