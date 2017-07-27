using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public partial class Extension
    {

        public static string PathCombine(this string @this,
            string str)
        {
            return Path.Combine(@this, str);
        }

        public static string ToJointString(this object[] @this,
            string seperator = ",")
        {
            return string.Join(seperator, @this);
        }

        public static string[] Split(this string @this,
            string separator)
        {
            return @this.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static bool Contains(this string @this,
            string value, bool caseInsensitive = false)
        {
            return @this.IndexOf(value, caseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture) >= 0;
        }

        public static int Lines(this string @this)
        {
            return @this?.Split('\n').Length ?? 0;
        }
        #region Regex   
        public static bool MatchRegex(this string @this,
            string regexPattern)
        {
            return Regex.IsMatch(@this, regexPattern);
        }
        public static IEnumerable<string> FindRegex(this string @this,
          string regexPattern)
        {
            return Regex.Matches(@this, regexPattern)
                .Cast<Match>()
                .Select(m => m.Value);
        }
        public static string ReplaceRegex(this string @this,
               string regexPattern, string value)
        {
            return Regex.Replace(@this, regexPattern, value);
        }
        #endregion
    }
}
