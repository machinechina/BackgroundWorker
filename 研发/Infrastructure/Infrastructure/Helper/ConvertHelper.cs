using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using System.Web.Script.Serialization;
namespace Infrastructure.Helper
{
    public static class DynamicJsonConverter
    {
        public static String ToJson(this Object obj)
        {
            return Json.Encode(obj);
        }

        public static dynamic JsonToDynamic(this String json)
        {
            return Json.Decode(json);
        }

        public static T JsonToObject<T>(this String json)
        {
            return Json.Decode<T>(json);
        }
    }

    public static class CollectionConverter
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            return col.Keys.Cast<string>()
                      .ToDictionary(k => k, v => col[v]);
        }
    }

}