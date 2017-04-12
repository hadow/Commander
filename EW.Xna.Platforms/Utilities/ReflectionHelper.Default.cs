using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EW.Xna.Platforms.Utilities
{
    internal partial class ReflectionHelpers
    {

        internal static class SizeOf<T>
        {
            static int _sizeOf;
            static SizeOf()
            {
                _sizeOf = Marshal.SizeOf<T>();
            }

            static public int Get()
            {
                return _sizeOf;
            }
        }


        internal static int ManagedSizeOf(Type type)
        {
            return Marshal.SizeOf(type);
        }
    }
}