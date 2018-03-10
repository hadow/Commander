using System;
namespace EW.Mods.Common.Activities
{
    public class CaptureActor:Enter
    {


        public CaptureActor(Actor self,Actor target):base(self,target,EnterBehaviour.Dispose)
        {
            
        }
    }
}
