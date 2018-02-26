using System;
using System.Collections.Generic;
using EW.NetWork;
using EW.Framework;
using EW.Framework.Touch;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Orders
{
    public class BeaconOrderGenerator:IOrderGenerator
    {


        public IEnumerable<Order> Order(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {
            world.CancelInputMode();

            if (gs.GestureType == GestureType.Tap)
                yield return new Order("PlaceBeacon", world.LocalPlayer.PlayerActor, Target.FromCell(world, cell), false) { SuppressVisualFeedback = true };
        }


        public virtual void Tick(World world) { }

        IEnumerable<IRenderable> IOrderGenerator.Render(WorldRenderer wr, World world) { yield break; }

        IEnumerable<IRenderable> IOrderGenerator.RenderAboveShroud(WorldRenderer wr, World world) { yield break; }

        public string GetCursor(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {
            return "ability";
        }


    }
}