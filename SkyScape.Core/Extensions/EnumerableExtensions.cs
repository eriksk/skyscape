using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AddRepeat<T>(this IEnumerable<T> list, T item, int count)
        {
            var l = list.ToList();
            for (int i = 0; i < count; i++)
                l.Add(item);
            return l;
        }
    }
}
