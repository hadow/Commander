using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using EW.Support;
namespace EW
{
    public static class Enum<T>
    {
        public static T Parse(string s) { return (T)Enum.Parse(typeof(T), s); }
    }

    
    public static class Exts
    {
        public enum ISqrtRoundMode { Floor, Nearest, Ceiling }

        public static int NextPowerOf2(int v)
        {
            --v;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            ++v;
            return v;
        }

        public static int ISqrt(int number, ISqrtRoundMode round = ISqrtRoundMode.Floor)
        {
            return 0;
        }


        public static uint ISqrt(uint number, ISqrtRoundMode round = ISqrtRoundMode.Floor)
        {
            return 0;
        }

        public static long ISqrt(long number,ISqrtRoundMode round = ISqrtRoundMode.Floor)
        {
            return 0L;
        }

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

        public static V GetOrAdd<K,V>(this Dictionary<K,V> d,K k) where V:new()
        {
            return d.GetOrAdd(k, _ => new V());
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


        public static IEnumerable<T> Append<T>(this IEnumerable<T> ts,params T[] moreTs)
        {
            return ts.Concat(moreTs);
        }

        public static string JoinWith<T>(this IEnumerable<T> ts,string j)
        {
            return string.Join(j, ts);
        }


        public static Dictionary<TKey,TSource> ToDictionaryWithConflictLog<TSource,TKey>(this IEnumerable<TSource> source,Func<TSource,TKey> keySelector,
            string debugName,Func<TKey,string> logKey,Func<TSource,string> logValue)
        {
            return ToDictionaryWithConflictLog(source, keySelector, x => x, debugName, logKey, logValue);
        }

        public static Dictionary<TKey,TElement> ToDictionaryWithConflictLog<TSource,TKey,TElement>(this IEnumerable<TSource> source,Func<TSource,TKey> keySelector,
            Func<TSource,TElement> elementSelector,string debugName,Func<TKey,string> logKey = null,Func<TElement,string> logValue = null)
        {
            logKey = logKey ?? (s => s.ToString());
            logValue = logValue ?? (s => s.ToString());

            var dupKeys = new Dictionary<TKey,List<string>>();
            var d = new Dictionary<TKey, TElement>();
            foreach(var item in source)
            {
                var key = keySelector(item);
                var element = elementSelector(item);

                if (d.ContainsKey(key))
                {
                    List<string> dupKeyMessages;
                    if(!dupKeys.TryGetValue(key,out dupKeyMessages))
                    {
                        dupKeyMessages = new List<string>();
                        dupKeyMessages.Add(logValue(d[key]));
                        dupKeys.Add(key, dupKeyMessages);
                    }

                    dupKeyMessages.Add(logValue(element));
                    continue;
                }
                d.Add(key, element);
            }

            if (dupKeys.Count > 0)
            {
                var badKeysFormatted = string.Join(", ", dupKeys.Select(p => "{0} :[{1}]".F(logKey(p.Key), string.Join(",", p.Value))));
                var msg = "{0},duplicate values found for the following keys:{1}".F(debugName, badKeysFormatted);
                throw new ArgumentException(msg);
            }
            return d;
        }

        public static bool TryParseIntegerInvariant(string s,out int i)
        {
            return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out i);
        }

        public static int ParseIntegerInvariant(string s)
        {
            return int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }


        public static T Random<T>(this IEnumerable<T> ts,MersenneTwister r)
        {
            return Random(ts, r, false);
        }

        static T Random<T>(IEnumerable<T> ts,MersenneTwister r,bool throws)
        {
            var xs = ts as ICollection<T>;
            xs = xs ?? ts.ToList();
            if (xs.Count == 0)
            {
                if (throws)
                    throw new ArgumentException("Collection must not be empty!");
                else
                    return default(T);
            }
            else
                return xs.ElementAt(r.Next(xs.Count));
        }

        public static T[] MakeArray<T>(int count,Func<int,T> f)
        {
            var result = new T[count];
            for(var i = 0; i < count; i++)
            {
                result[i] = f(i);
            }
            return result;
        }

        public static int IntegerDivisionRoundingAwayFromZero(int dividend,int divisor)
        {
            int remainder;
            var quotient = Math.DivRem(dividend, divisor, out remainder);
            if (remainder == 0)
                return quotient;
            return quotient + (Math.Sign(dividend) == Math.Sign(divisor) ? 1 : -1);
        }

        public static int ToBits(this IEnumerable<bool> bits)
        {
            var i = 0;
            var result = 0;
            foreach (var b in bits)
                if (b)
                    result |= 1 << i++;
                else
                    i++;
            if (i > 33)
                throw new InvalidOperationException("ToBits only accepts up to 32 values.");
            return result;
        }
    }
}