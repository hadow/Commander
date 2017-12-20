using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class StartGameNotificationInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new StartGameNotification();
        }
    }


    class StartGameNotification
    {

    }
}