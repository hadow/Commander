using System;
using EW.Widgets;
namespace EW.Mods.Common.Widgets
{
    public class LogicTickerWidget:Widget
    {
        public Action OnTick = () => { };


        public override void Tick()
        {
            OnTick();
        }
    }
}
