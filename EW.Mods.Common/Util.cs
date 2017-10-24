using System;
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
    }
}