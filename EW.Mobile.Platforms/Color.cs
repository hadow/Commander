using System;
using System.Collections.Generic;


namespace EW.Mobile.Platforms
{
    /// <summary>
    /// 32-bit color
    /// </summary>
    public struct Color:IEquatable<Color>
    {

        public static Color Black
        {
            get;private set;
        }


        public bool Equals(Color other)
        {
            return false;
        }

    }
}