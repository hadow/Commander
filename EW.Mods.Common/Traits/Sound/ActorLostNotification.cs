using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    class ActorLostNotificationInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ActorLostNotification();
        }
    }
    class ActorLostNotification
    {
    }
}