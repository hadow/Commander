using System;
using System.Collections.Generic;

namespace RA.Primitives
{
    public  struct Pair<T,U>:IEquatable<Pair<T,U>>
    {
        public T First;
        public U Second;

        internal static IEqualityComparer<T> Tcomparer = EqualityComparer<T>.Default;
        internal static IEqualityComparer<U> Ucomparer = EqualityComparer<U>.Default;

        public static bool operator ==(Pair<T,U> a,Pair<T,U> b)
        {
            return Tcomparer.Equals(a.First, b.First) && Ucomparer.Equals(b.Second, a.Second);
        }

        public static bool operator !=(Pair<T,U> a,Pair<T,U> b)
        {
            return !(a == b);
        }
        public Pair(T first,U second)
        {
            First = first;
            Second = second;
        }

        public bool Equals(Pair<T,U> other)
        {
            return this == other;
        }



    }

    public static class Pair
    {


        public static Pair<T,U> New<T,U>(T t,U u) { return new Pair<T, U>(t, u); }
        static Pair()
        {
            
        }
    }
}