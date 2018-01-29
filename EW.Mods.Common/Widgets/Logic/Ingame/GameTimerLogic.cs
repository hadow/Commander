using System;
using System.Collections.Generic;
using EW.NetWork;
using EW.Widgets;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Widgets.Logic
{
    public class GameTimerLogic:ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public GameTimerLogic(Widget widget,OrderManager orderManager,World world)
        {
            var timer = widget.GetOrNull<LabelWidget>("GAME_TIMER");
            var status = widget.GetOrNull<LabelWidget>("GAME_TIMER_STATUS");
            var startTick = UI.LastTickTime;

            if(timer != null)
            {
                //Timers in replays should be synced to the effective game time,not the playback time.
                var timestep = world.Timestep;
                if (world.IsReplay)
                    timestep = world.WorldActor.Trait<MapOptions>().GameSpeed.Timestep;

                timer.GetText = () =>
                {
                    //if(status == null &&)
                    return WidgetUtils.FormatTime(world.WorldTick, timestep);
                };
            }
        }
            

    }
}