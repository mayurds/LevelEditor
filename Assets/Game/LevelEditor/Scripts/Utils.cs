using System;
using System.Collections.Generic;
using System.Linq;
public static class EnumUtil
{
    public static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}

public static class LinqUtils
{
    public static T TryGetElement<T>(this IEnumerable<T> seq, int index, T ifnull = default(T))
    {
        if (index < seq.Count())
        {
            var element = seq.ToArray()[index];
            return element;
        }
        return ifnull;
    }
}
