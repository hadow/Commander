using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    public class LoadWidgetAtGameStartInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new LoadWidgetAtGameStart(this);
        }
    }


    public class LoadWidgetAtGameStart:IWorldLoaded
    {

        readonly LoadWidgetAtGameStartInfo info;
        public LoadWidgetAtGameStart(LoadWidgetAtGameStartInfo info)
        {
            this.info = info;
        }


        void IWorldLoaded.WorldLoaded(World w, WorldRenderer render)
        {

        }
        
    }



}