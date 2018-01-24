using System;
using EW.Traits;
using EW.Graphics;
using EW.Widgets;
namespace EW.Mods.Common.Traits
{
    public class LoadWidgetAtGameStartInfo:ITraitInfo
    {
        public readonly string IngameRoot = "INGAME_ROOT";

        public readonly bool ClearRoot = true;


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
            if (info.ClearRoot)
                UI.ResetAll();

            WarGame.LoadWidget(w,info.IngameRoot,UI.Root,new WidgetArgs());
        }
        
    }



}