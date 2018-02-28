using System;
using EW.Traits;
using EW.Graphics;
using EW.Widgets;
namespace EW.Mods.Common.Traits
{
    public class LoadWidgetAtGameStartInfo:ITraitInfo
    {
        public readonly string ShellmapRoot = "MAINMENU";

        public readonly string IngameRoot = "INGAME_ROOT";

        public readonly string EditorRoot = "EDITOR_ROOT";

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


            //WarGame.LoadWidget(w,info.IngameRoot,UI.Root,new WidgetArgs());

            var widget = w.Type == WorldT.Shellmap ? info.ShellmapRoot : w.Type == WorldT.Editor ? info.EditorRoot : info.IngameRoot;

            WarGame.LoadWidget(w, widget, UI.Root, new WidgetArgs());
        }
        
    }



}