using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extension
{
    public static partial class Extension
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

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (@this != null)
            {
                if (!@this.ContainsKey(key))
                {
                    @this.Add(key, value);
                }
                else
                {
                    @this[key] = value;
                }
            }
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TValue> updateFunc)
            where TValue : new()
        {
            if (@this != null)
            {
                if (!@this.ContainsKey(key))
                {
                    @this.Add(key, new TValue());
                }
                updateFunc(@this[key]);
            }
        }
    }
}
