using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Infrastructure.Extensions
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

        /// <summary>
        /// 为Dictionary增加或修改值
        /// 如果是ConcurrentDictionary会保持线程安全
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (@this != null)
            {
                //直接调用ConcurrentDictionary的Add是线程不安全的
                if (@this is ConcurrentDictionary<TKey, TValue> @thisConcurrent)
                {
                    @thisConcurrent.AddOrUpdate(key, value, (k, v) => v);
                }
                else if (!@this.ContainsKey(key))
                {
                    @this.Add(key, value);
                }
                else
                {
                    @this[key] = value;
                }
            }
        }

        /// <summary>
        /// 为Dictionary增加或修改值
        /// 如果是ConcurrentDictionary会保持线程安全
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key"></param>
        /// <param name="updateFunc">由Func计算得出的value,注意并发时还是可能脏读</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TValue> updateFunc)
            where TValue : new()
        {
            if (@this != null)
            {
                //直接调用ConcurrentDictionary的Add是线程不安全的
                if (@this is ConcurrentDictionary<TKey, TValue> @thisConcurrent)
                {
                    @thisConcurrent.AddOrUpdate(key, k =>
                    {
                        var v = new TValue();
                        updateFunc(v);
                        return v;
                    }, (k, v) =>
                    {
                        updateFunc(v);
                        return v;
                    });
                }
                else
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
}