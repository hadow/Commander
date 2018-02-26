using System;
using System.Collections.Generic;
using EW.Widgets;
using EW.Mods.Common.Orders;
namespace EW.Mods.Common.Widgets.Logic
{
    public class BeaconOrderButtonLogic : ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public BeaconOrderButtonLogic(Widget widget,World world)
        {
            var beacon = widget as ButtonWidget;
            if (beacon != null)
                OrderButtonsChromeUtils.BindOrderButton<BeaconOrderGenerator>(world, beacon, "beacon");
        }
    }

    public class OrderButtonsChromeUtils
    {
        public static void BindOrderButton<T>(World world,ButtonWidget w,string icon) where T : IOrderGenerator, new()
        {
            w.OnClick = () => world.ToggleInputMode<T>();
            w.IsHighlighted = () => world.OrderGenerator is T;

            w.Get<ImageWidget>("ICON").GetImageName = () => world.OrderGenerator is T ? icon + "-active" : icon;
        }
    }
}