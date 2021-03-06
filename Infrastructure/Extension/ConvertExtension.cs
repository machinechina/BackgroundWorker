﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Infrastructure.Extensions
{
    public static partial class Extension
    {
        public static string ToJson(this object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        public static T JsonToObject<T>(this string json)
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }

        /// <summary>
        /// 如果编译错误,需要Nuget引入 microsoft.csharp
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic JsonToDynamic(this string json)
        {
            return Json.Decode(json);
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            return col.Keys.Cast<string>()
                      .ToDictionary(k => k, v => col[v]);
        }

        public static T ConvertTo<T>(this object @this)
        {
            return ( T )Convert.ChangeType(@this, typeof(T));
        }

        public static object ConvertTo(this object @this, Type type)
        {
            return Convert.ChangeType(@this, type);
        }
    }
}