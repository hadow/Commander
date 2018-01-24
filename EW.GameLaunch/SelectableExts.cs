using System;
using System.Drawing;

using System.Collections.Generic;
using System.Linq;
using EW.Framework;

namespace EW.Traits
{
    public static class SelectableExts
    {
        const int PriorityRange = 30;

        static readonly Actor[] NoActors = { };

        public static int SelectionPriority(this ActorInfo a){

            var selectableInfo = a.TraitInfoOrDefault<SelectableInfo>();
            return selectableInfo != null ? selectableInfo.Priority : int.MinValue;
        }

        public static int SelectionPriority(this Actor a){

            var basePriority = a.Info.TraitInfo<SelectableInfo>().Priority;
            var lp = a.World.LocalPlayer;

            if (a.Owner == lp || lp == null)
                return basePriority;

            switch(lp.Stances[a.Owner]){
                case Stance.Ally: return basePriority - PriorityRange;
                case Stance.Neutral:return basePriority - 2 * PriorityRange;
                case Stance.Enemy: return basePriority - 3 * PriorityRange;
                default:
                    throw new InvalidOperationException();

            }
        }

        public static Actor WithHighestSelectionPriority(this IEnumerable<ActorBoundsPair> actors,Int2 selectionPixel){

            if (!actors.Any())
                return null;

            return actors.MaxBy(a => CalculateActorSelectionPriority(a.Actor.Info, a.Bounds, selectionPixel)).Actor;

        }


        static long CalculateActorSelectionPriority(ActorInfo info,Rectangle bounds,Int2 selectionPixel){

            if (bounds.IsEmpty)
                return info.SelectionPriority();

            var centerPixel = new Int2(bounds.Left + bounds.Size.Width / 2, bounds.Top + bounds.Size.Height / 2);

            var pixelDistance = (centerPixel - selectionPixel).Length;

            return ((long)-pixelDistance << 32) + info.SelectionPriority();
        }


        public static IEnumerable<Actor> SubsetWithHighestSelectionPriority(this IEnumerable<Actor> actors){

            return actors.GroupBy(x => x.SelectionPriority())
                         .OrderByDescending(g => g.Key)
                         .Select(g => g.AsEnumerable())
                         .DefaultIfEmpty(NoActors)
                         .FirstOrDefault();
                         
        }
    }
}
