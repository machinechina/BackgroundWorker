using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public partial class Extension
    {

        public static string PathCombine(this string @this, string str)
        {
            return Path.Combine(@this, str);
        }

        public static string ToJointString(this object[] @this, string seperator = ",")
        {
            return string.Join(seperator, @this);
        }

        public static string[] Split(this string @this, string separator)
        {
            return @this.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}
