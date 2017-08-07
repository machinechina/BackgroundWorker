using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure.Extensions
{
    public partial class Extension
    {
        /// <summary>
        /// 通过反射查找包含指定关键字的成员,可以包含子成员(非原生类型)
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public static IEnumerable<string> FindMembers(this Type @this, string name, bool recursive = false, bool caseInsensitive = false)
        {
            //All members
            foreach (var m in
             @this.FindMembers(
                MemberTypes.All,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic,
                new MemberFilter((m, o) =>
                        m.Name.Contains(o.ToString(), caseInsensitive)
                        //inner field for prop
                        && !m.Name.Contains("k__BackingField")
                        //inner get/set methods for prop
                        && !m.Name.StartsWith("get_")
                        && !m.Name.StartsWith("set_")), name)
                    .Select(m =>
                     $"{m.DeclaringType.Name}/{m.Name}({m.MemberType})"))
                yield return m;

            if (recursive)
                //Sub fields/properties
                foreach (var m in
                    @this.FindMembers(
                        MemberTypes.Field |
                        MemberTypes.Property,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic, null, null)
                        .Select(m =>
                            (m as FieldInfo)?.FieldType ??
                            (m as PropertyInfo).PropertyType)
                        .Where(m =>
                            !m.IsSimpleType())
                            .SelectMany(m =>
                                m.FindMembers(name, true, caseInsensitive)))
                    yield return m;
        }

        public static bool IsSimpleType(this TypeInfo type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimpleType((type.GetGenericArguments()[0]).GetTypeInfo());
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        public static bool IsSimpleType(this Type type)
        {
            return type.GetTypeInfo().IsSimpleType();
        }
    }
}