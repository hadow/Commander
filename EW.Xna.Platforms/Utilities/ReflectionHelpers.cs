using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EW.Xna.Platforms.Utilities
{
    internal static partial class ReflectionHelpers
    {

        public static bool IsValueType(Type targetT)
        {
            if (targetT == null)
                throw new NullReferenceException("Must Apply the targetType parameter");

            return targetT.IsValueType;
        }

    }
}