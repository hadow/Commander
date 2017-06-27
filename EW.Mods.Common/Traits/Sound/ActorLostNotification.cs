using System;
using System.Collections.Generic;
using EW.Traits;
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