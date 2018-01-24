using System;
using EW.Widgets;
using EW.Mods.Common.Scripting;
namespace EW.Mods.Common.Widgets.Logic
{
    public class LoadIngamePlayerOrObserverUILogic:ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public LoadIngamePlayerOrObserverUILogic(Widget widget,World world)
        {
            var ingameRoot = widget.Get("INGAME_ROOT");
            var worldRoot = widget.Get("WORLD_ROOT");


            if(world.LocalPlayer == null){


            }
            else{



            }

            world.GameOver += () =>
            {
                UI.CloseWindow();

                if(world.LocalPlayer != null)
                {
                    var scriptContext = world.WorldActor.TraitOrDefault<LuaScript>();
                    //var missionData = world.WorldActor.Info.TraitInfoOrDefault<>()
                }

            };
        }
    }
}
