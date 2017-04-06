using System;
using System.Reflection;

namespace EW.Mobile.Platforms.Content
{
    internal static class ContentExtensions
    {

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            var attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return type.GetConstructor(attrs, null, new Type[0], null);
        }


    }
}