using System;
using EW.Primitives;
using EW.OpenGLES;
namespace EW.Support
{
    public static class PerfHistory
    {
        static readonly Color[] Colors =
        {
            Color.Red,
            Color.Green,
            Color.Orange,
            Color.Yellow,
            Color.Fuchsia,
            Color.Lime,
            Color.LightBlue,
            Color.Blue,
            Color.White,
            Color.Teal

        };

        static int nextColor;

        public static Cache<string, PerfItem> Items = new Cache<string, PerfItem>(s =>
         {
             var x = new PerfItem(s, Colors[nextColor++]);
             if (nextColor >= Colors.Length)
                 nextColor = 0;
             return x;
         });

        public static void Increment(string item,double x)
        {
            Items[item].Val += x;
        }

        public static void Tick()
        {
            foreach (var item in Items.Values)
                if (item.HasNormalTick)
                    item.Tick();
        }


    }
}