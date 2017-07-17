using System;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class Turn:Activity
    {
        public Turn(Actor self,int desiredFacing)
        {

        }

        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();
        }
    }
}