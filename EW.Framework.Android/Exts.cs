using System;


namespace EW.Framework
{
    public static class Exts
    {

        public static bool IsPowerOf2(int v)
        {
            return (v & (v - 1)) == 0;
        }

    }
}