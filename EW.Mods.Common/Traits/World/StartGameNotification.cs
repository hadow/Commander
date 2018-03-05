using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    class StartGameNotificationInfo:ITraitInfo
    {
        public readonly string Notification = "StartGame";

        public object Create(ActorInitializer init)
        {
            return new StartGameNotification(this);
        }
    }


    class StartGameNotification:IWorldLoaded
    {
        StartGameNotificationInfo info;
        public StartGameNotification(StartGameNotificationInfo info)
        {
            this.info = info;
        }

        void IWorldLoaded.WorldLoaded(World world,WorldRenderer wr){

            WarGame.Sound.PlayNotification(world.Map.Rules, null, "Speech", info.Notification, world.RenderPlayer == null ? null : world.RenderPlayer.Faction.InternalName);

        }
    }
}