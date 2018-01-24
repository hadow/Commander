using System;
using EW.Widgets;
using EW.Graphics;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Widgets
{
    public class ViewportControllerWidget:Widget
    {
        readonly ModData modData;
        readonly ResourceLayer resourceLayer;


        World world;
        WorldRenderer worldRenderer;

        [ObjectCreator.UseCtor]
        public ViewportControllerWidget(ModData modData,World world,WorldRenderer worldRenderer)
        {
            this.modData = modData;
            this.world = world;
            this.worldRenderer = worldRenderer;
        }



        public override void Draw()
        {
            base.Draw();
        }
    }
}
