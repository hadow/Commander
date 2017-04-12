using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace EW
{
    public static class Exts
    {


        public static string F(this string fmt,params object[] args)
        {
            return string.Format(fmt, args);
        }

        public static T[] GetCustomAttributes<T>(this MemberInfo mi,bool inherit) where T:class
        {
            return (T[])mi.GetCustomAttributes(typeof(T), inherit);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="d"></param>
        /// <param name="k"></param>
        /// <param name="createFn"></param>
        /// <returns></returns>
        public static V GetOrAdd<K,V>(this Dictionary<K,V> d,K k,Func<K,V> createFn)
        {
            V ret;
            if(!d.TryGetValue(k,out ret))
            {
                d.Add(k, ret = createFn(k));
            }
            return ret;
        }


        public static IEnumerable<string> GetNamespaces(this Assembly a)
        {
            return a.GetTypes().Select(t => t.Namespace).Distinct().Where(n => n != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Lazy<T> Lazy<T>(Func<T> p)
        {
            return new Lazy<T>(p);
        }

        public static bool HasAttribute<T>(this MemberInfo mi)
        {
            return mi.GetCustomAttributes(typeof(T), true).Length != 0;
        }

        public static T Clamp<T>( this T val,T min,T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            else if (val.CompareTo(max) > 0)
                return max;
            else
                return val;
        }
    }
}