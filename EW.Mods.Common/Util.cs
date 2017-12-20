﻿using System;
using System.Collections.Generic;


namespace EW.Mods.Common
{
    public static class Util
    {

        public static int TickFacing(int facing,int desiredFacing,int rot)
        {
            var leftTurn = (facing - desiredFacing) & 0xFF;
            var rightTurn = (desiredFacing - facing) & 0xFF;

            if (Math.Min(leftTurn, rightTurn) < rot)
                return desiredFacing & 0xFF;
            else if (rightTurn < leftTurn)
                return (facing + rot) & 0xFF;
            else
                return (facing - rot) & 0xFF;
        }

        public static int ApplyPercentageModifiers(int number,IEnumerable<int> percentages)
        {
            var a = (decimal)number;
            foreach (var p in percentages)
                a *= p / 100m;
            return (int)a;
        }

        /// <summary>
        /// Quantizes the facing.
        /// </summary>
        /// <returns>The facing.</returns>
        /// <param name="facing">Facing.</param>
        /// <param name="numFrames">Number frames.</param>

        public static int QuantizeFacing(int facing, int numFrames){

            var step = 256 / numFrames;

            var a = (facing + step / 2) & 0xff;

            return a / step;
        }

        public static int QuantizeFacing(int facing, int numFrames, bool useClassicFacingFudge)
        {
            if (!useClassicFacingFudge || numFrames != 32)
                return Util.QuantizeFacing(facing, numFrames);

            // TD and RA divided the facing artwork into 3 frames from (north|south) to (north|south)-(east|west)
            // and then 5 frames from (north|south)-(east|west) to (east|west)
            var quadrant = ((facing + 31) & 0xFF) / 64;
            if (quadrant == 0 || quadrant == 2)
            {
                var frame = Util.QuantizeFacing(facing, 24);
                if (frame > 18)
                    return frame + 6;
                if (frame > 4)
                    return frame + 3;
                return frame;
            }
            else
            {
                var frame = Util.QuantizeFacing(facing, 40);
                return frame < 20 ? frame - 3 : frame - 8;
            }
        }
    }
}