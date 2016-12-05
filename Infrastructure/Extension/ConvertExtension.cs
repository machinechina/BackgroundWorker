using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Script.Serialization;

namespace Infrastructure.Extensions
{
    public static partial class Extension
    {
        public static String ToJson(this Object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        public static T JsonToObject<T>(this String json)
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            return col.Keys.Cast<string>()
                      .ToDictionary(k => k, v => col[v]);
        }
    }
}