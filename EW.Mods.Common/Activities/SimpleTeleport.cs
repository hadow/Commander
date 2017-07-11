using System;
using EW.Activities;

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
            return NextActivity;
        }
    }
}