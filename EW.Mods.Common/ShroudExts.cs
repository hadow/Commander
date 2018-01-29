using System;
using System.Collections.Generic;
using EW.Primitives;
using EW.Traits;
namespace EW.Mods.Common
{
    public static class ShroudExts
    {

        public static bool AnyExplored(this Shroud shroud,Pair<CPos,SubCell>[] cells)
        {
            //PERF:Avoid LINQ
            foreach (var cell in cells)
                if (shroud.IsExplored(cell.First))
                    return true;

            return false;
        }


        public static bool AnyVisible(this Shroud shroud,Pair<CPos,SubCell>[] cells)
        {
            //PERF:Avoid LINQ
            foreach (var cell in cells)
                if (shroud.IsVisible(cell.First))
                    return true;

            return false;
        }
    }
}