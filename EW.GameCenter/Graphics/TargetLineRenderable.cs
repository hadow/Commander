using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public struct TargetLineRenderable:IRenderable,IFinalizedRenderable
    {

        readonly Color color;
        readonly IEnumerable<WPos> waypoints;

        public TargetLineRenderable(IEnumerable<WPos> waypoints,Color color)
        {
            this.waypoints = waypoints;
            this.color = color;
        }
            



    }
}