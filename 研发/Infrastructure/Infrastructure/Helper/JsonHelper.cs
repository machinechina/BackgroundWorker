using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static dynamic FromJsonToDynamic(this String json)
        {
            return Json.Decode(json);
        }

        public static T FromJsonToObject<T>(this String json)
        {
            return Json.Decode<T>(json);
        }
    }

}