using System;
using System.Collections.Generic;
using EW.Mods.Common.Pathfinder;
using EW.Mods.Common.Traits;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    /// <summary>
    /// 
    /// </summary>
    public class Move:Activity
    {
        readonly Func<List<CPos>> getPath;

        readonly WDist nearEnough;


        readonly Mobile mobile;
        List<CPos> path;
        CPos? destination;
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
            throw new NotImplementedException();

        }

    }
}