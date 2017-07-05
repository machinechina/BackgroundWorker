using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extension
{
    public static partial class CollectionExtension
    {
        public static void ForEach<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> @this, Action<KeyValuePair<TKey, TValue>> action)
        {
            if (@this != null)
            {
                foreach (var kv in @this)
                {
                    action(kv);
                }
            }
        }

        public static void ForEach<T>(this ICollection<T> @this, Action<T> action)
        {
            if (@this != null)
            {
                foreach (var kv in @this)
                {
                    action(kv);
                }
            }
        }
    }
}
