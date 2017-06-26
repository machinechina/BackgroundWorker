using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extension
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
    }
}
