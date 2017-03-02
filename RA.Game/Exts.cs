using System;
using System.Reflection;
using System.Collections.Generic;

namespace RA.Game
{
    public static class Exts
    {

        public static T[] GetCustomAttributes<T>(this MemberInfo mi,bool inherit) where T:class
        {
            return (T[])mi.GetCustomAttributes(typeof(T), inherit);
        }
    }
}