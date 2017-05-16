using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class CaptureNotificationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new CaptureNotification();
        }
    }

    public class CaptureNotification
    {


    }


}