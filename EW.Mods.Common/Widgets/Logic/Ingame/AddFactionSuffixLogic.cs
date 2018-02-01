using System;
using EW.Widgets;
namespace EW.Mods.Common.Widgets.Logic
{
    public class AddFactionSuffixLogic:ChromeLogic
    {

        [ObjectCreator.UseCtor]
        public AddFactionSuffixLogic(Widget widget,World world)
        {
            string faction = "gdi";
            if (!ChromeMetrics.TryGet("FactionSuffix-" + world.LocalPlayer.Faction.InternalName, out faction))
                faction = world.LocalPlayer.Faction.InternalName;

            var suffix = "-" + faction;

            var buttonWidget = widget as ButtonWidget;
            if(buttonWidget != null)
            {
                buttonWidget.Background += suffix;
            }
            else
            {
                var imageWidget = widget as ImageWidget;
                if (imageWidget != null)
                    imageWidget.ImageCollection += suffix;
                else
                    throw new InvalidOperationException("AddFactionSuffixLogic only supports ButtonWidget and ImageWidget.");
            }
        }
    }
}