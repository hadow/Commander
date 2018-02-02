using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Framework;
using EW.NetWork;
using EW.Framework.Touch;
namespace EW
{
    public interface IOrderGenerator
    {

        IEnumerable<Order> Order(World world, CPos cell, Int2 worldPixel, GestureSample gs);

        void Tick(World world);

        IEnumerable<IRenderable> Render(WorldRenderer wr, World world);

        IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world);

        string GetCursor(World world, CPos cell, Int2 worldPixel, GestureSample gs);


    }
}