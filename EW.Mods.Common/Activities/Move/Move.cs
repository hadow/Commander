using System;
using System.Linq;
using System.Collections.Generic;
using EW.Mods.Common.Pathfinder;
using EW.Mods.Common.Traits;
using EW.Activities;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Activities
{
    /// <summary>
    /// 
    /// </summary>
    public class Move:Activity
    {
        readonly Actor ignoredActor;
        readonly Func<List<CPos>> getPath;

        readonly WDist nearEnough;


        readonly Mobile mobile;
        List<CPos> path;
        CPos? destination;          //目的地
        public Move(Actor self,CPos destination)
        {
            getPath = () =>
            {
                List<CPos> path;
                using(var search = PathSearch.FromPoint(self.World, mobile.Info, self, mobile.ToCell, destination, false).WithoutLaneBias())
                {
                    path = self.World.WorldActor.Trait<IPathFinder>().FindPath(search);
                }
                return path;

            };

            this.destination = destination;
            nearEnough = WDist.Zero;
            
        }
        public Move(Actor self,CPos destination,WDist nearEnough)
        {

        }

        public Move(Actor self,CPos destination,Actor ignoredActor)
        {

        }

        public Move(Actor self,Func<List<CPos>> getPath)
        {

        }

        public override void Cancel(Actor self)
        {
            base.Cancel(self);
        }

        public override void Queue(Activity activity)
        {
            base.Queue(activity);
        }


        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;
            if (mobile.IsTraitDisabled)
                return this;
            if (destination == mobile.ToCell)
                return NextActivity;

            if(path == null)
            {
                if(mobile.TicksBeforePathing>0)
                {
                    --mobile.TicksBeforePathing;
                    return this;
                }
                path = EvalPath();
                SanityCheckPath(mobile);
            }

            if(path.Count == 0)
            {
                destination = mobile.ToCell;
                return this;
            }

            destination = path[0];

            var nextCell = PopPath(self);
            if (nextCell == null)
                return this;

            var firstFacing = self.World.Map.FacingBetween(mobile.FromCell, nextCell.Value.First, mobile.Facing);

            if(firstFacing != mobile.Facing)
            {
                path.Add(nextCell.Value.First);
                return ActivityUtils.SequenceActivities(new Turn(self, firstFacing), this);
            }
            else
            {
                mobile.SetLocation(mobile.FromCell, mobile.FromSubCell, nextCell.Value.First, nextCell.Value.Second);
                var from = self.World.Map.CenterOfSubCell(mobile.FromCell, mobile.FromSubCell);
                return this;
            }
        }

        Pair<CPos,SubCell>? PopPath(Actor self)
        {
            if (path.Count == 0)
                return null;

            var nextCell = path[path.Count - 1];

            var subCell = mobile.GetAvailableSubCell(nextCell, SubCell.Any, ignoredActor);

            return Pair.New(nextCell, subCell);
        }

        List<CPos> EvalPath()
        {
            var path = getPath().TakeWhile(a => a != mobile.ToCell).ToList();
            mobile.PathHash = HashList(path);
            return path;
        }

        void SanityCheckPath(Mobile mobile)
        {
            if (path.Count == 0)
                return;
            var d = path[path.Count - 1] - mobile.ToCell;
            if (d.LengthSquard > 2)
                throw new InvalidOperationException("(Move) Sanity check failed");
        }


        static int HashList<T>(List<T> xs)
        {
            var hash = 0;
            var n = 0;
            foreach(var x in xs)
                hash += n++ * x.GetHashCode();
            return hash;
        }
    }
}