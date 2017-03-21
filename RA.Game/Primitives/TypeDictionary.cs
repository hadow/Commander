using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace RA.Game.Primitives
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeDictionary:IEnumerable
    {
        static readonly Func<Type, List<object>> CreateList = type => new List<object>();

        readonly Dictionary<Type, List<object>> data = new Dictionary<Type, List<object>>();

        public IEnumerator GetEnumerator()
        {
            return WithInterface<object>().GetEnumerator();
        }


        public IEnumerable<T> WithInterface<T>()
        {
            List<object> objs;
            if(data.TryGetValue(typeof(T),out objs))
            {
                return objs.Cast<T>();
            }
            return new T[0];
        }


        public bool Contains<T>()
        {
            return data.ContainsKey(typeof(T));
        }
    }
}