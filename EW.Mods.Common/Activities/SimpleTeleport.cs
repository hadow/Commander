using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class SimpleTeleport:Activity
    {
        CPos destination;

        public SimpleTeleport(CPos destination)
        {
            this.destination = destination;
        }

        public override Activity Tick(Actor self)
        {
            self.Trait<IPositionable>().SetPosition(self, destination);
            self.Generation++;
            return NextActivity;
        }
    }
}