using System;
using EW.Widgets;
namespace EW.Mods.Common.Widgets
{
    public class StrategicProgressWidget:Widget
    {

        readonly World world;
        bool initialised = false;

        [ObjectCreator.UseCtor]
        public StrategicProgressWidget(World world)
        {

            IsVisible = () => true;
            this.world = world;
        }
    }
}
