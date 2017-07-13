using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW.Support
{
    public class PerfItem
    {

        public readonly Color C;
        public readonly string Name;

        public double Val = 0.0;
        double[] samples = new double[100];

        public bool HasNormalTick = true;
        public PerfItem(string name,Color c)
        {
            Name = name;
            C = c;
        }

        public void Tick()
        {

        }

    }
}