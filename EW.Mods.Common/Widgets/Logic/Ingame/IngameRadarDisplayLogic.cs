using System;
using System.Linq;
using EW.Widgets;
using System.Drawing;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Widgets.Logic
{
    public class IngameRadarDisplayLogic:ChromeLogic
    {


        [ObjectCreator.UseCtor]
        public IngameRadarDisplayLogic(Widget widget,World world)
        {
            var radarEnabled = false;
            var cachedRadarEnabled = false;
            var blockColor = Color.Transparent;
            var radar = widget.Get<RadarWidget>("RADAR_MINIMAP");
            radar.IsEnabled = () => radarEnabled;

            var ticker = widget.Get<LogicTickerWidget>("RADAR_TICKER");
            ticker.OnTick = () =>
            {
                radarEnabled = world.ActorsHavingTrait<ProvidesRadar>(r => !r.IsTraitDisabled).Any(a => a.Owner == world.LocalPlayer);

                if (radarEnabled != cachedRadarEnabled)
                {
                    //WarGame.Sound.playno
                }
                cachedRadarEnabled = radarEnabled;

            };

            var block = widget.GetOrNull<ColorBlockWidget>("RADAR_FADETOBLACK");
            if(block != null)
            {
                radar.Animating = x => blockColor = Color.FromArgb((int)(255 * x), Color.Black);
                block.IsVisible = () => blockColor.A != 0;
                block.GetColor = () => blockColor;
            }
        }
    }
}