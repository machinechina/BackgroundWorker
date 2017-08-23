using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Extensions
{
    public partial class Extension
    {
        public static string PathCombine(this string @this,
            string str)
        {
            return Path.Combine(@this, str);
        }

        public static string ToJointString(this IEnumerable<object> @this,
            string seperator = ",")
        {
            return string.Join(seperator, @this);
        }

        public static string ToJointString(this char[] @this,
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

        public static string Sort(this string @this)
        {
            return string.Concat(@this.OrderBy(c => c));
        }



        public static IEnumerable<string> WhereNotEmpty(this IEnumerable<string> @this)
        {
            return @this.Where(s => !string.IsNullOrEmpty(s));
        }

        public static string ConvertToChinese(this decimal number)
        {
            var s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            var d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            var r = Regex.Replace(d, ".", m => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - '-'].ToString());
            if (r.EndsWith("元"))//这两行是我加的
                r += "整";//感觉我拉低了前边代码的逼格……很惭愧
            return r;
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

        public static byte[] GetBytes(this string @this)
        {
            return Encoding.UTF8.GetBytes(@this);
        }

        public static string GetString(this byte[] @this)
        {
            return Encoding.UTF8.GetString(@this);
        }

        #endregion Regex
    }
}