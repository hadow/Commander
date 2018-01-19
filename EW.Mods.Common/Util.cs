using System;
using System.Collections.Generic;
using EW.Support;
using EW.Mods.Common.Traits;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common
{
    public static class Util
    {

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> ts,MersenneTwister random)
        {
            var items = ts.ToArray();

            for(var i = 0; i < items.Length-1; i++)
            {
                var j = random.Next(items.Length - i);
                var item = items[i + j];
                items[i + j] = items[i];
                items[i] = item;
                yield return item;
            }

            if (items.Length > 0)
                yield return items[items.Length - 1];
        }


        public static IEnumerable<CPos> RandomWalk(CPos p,MersenneTwister r)
        {
            for(; ; )
            {
                var dx = r.Next(-1, 2);
                var dy = r.Next(-1, 2);

                if (dx == 0 && dy == 0)
                    continue;

                p += new CVec(dx, dy);
                yield return p;
            }
        }

        public static int RandomDelay(World world,int[] range)
        {
            if (range.Length == 0)
                return 0;

            if (range.Length == 1)
                return range[0];

            return world.SharedRandom.Next(range[0], range[1]);
        }


        public static int GetNearestFacing(int facing,int desiredFacing)
        {
            var turn = desiredFacing - facing;
            if (turn > 128)
                turn -= 256;

            if (turn < -128)
                turn += 256;

            return facing + turn;
        }

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


        public static bool FacingWithinTolerance(int facing,int desiredFacing,int facingTolerance)
        {
            if (facingTolerance == 0 && facing == desiredFacing)
                return true;

            var delta = Util.NormalizeFacing(desiredFacing - facing);
            return delta <= facingTolerance || delta >= 256 - facingTolerance;
        }

        public static int ApplyPercentageModifiers(int number,IEnumerable<int> percentages)
        {
            var a = (decimal)number;
            foreach (var p in percentages)
                a *= p / 100m;
            return (int)a;
        }

        /// <summary>
        /// Wraps an arbitrary integer facing value into the range 0-255
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int NormalizeFacing(int f)
        {
            if (f >= 0)
                return f & 0xFF;

            var negative = -f & 0xFF;
            return negative == 0 ? 0 : 256 - negative;
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


        public static WPos BetweenCells(World w,CPos from,CPos to)
        {
            var fromPos = from.Layer == 0 ? w.Map.CenterOfCell(from) : w.GetCustomMovementLayers()[from.Layer].CenterOfCell(from);

            var toPos = to.Layer == 0 ? w.Map.CenterOfCell(to) : w.GetCustomMovementLayers()[to.Layer].CenterOfCell(to);

            return WPos.Lerp(fromPos, toPos, 1, 2);
        }


        public static WDist MinimumRequiredBlockerScanRadius(Ruleset rules)
        {
            return rules.Actors.Where(a => a.Value.HasTraitInfo<IBlocksProjectilesInfo>())
                .SelectMany(a => a.Value.TraitInfos<HitShapeInfo>()).Max(h => h.Type.OuterRadius);
        }


        public static IEnumerable<CPos> AdjacentCells(World w,Target target)
        {
            var cells = target.Positions.Select(p => w.Map.CellContaining(p)).Distinct();
            return ExpandFootprint(cells, true);
        }

        public static IEnumerable<CPos> ExpandFootprint(IEnumerable<CPos> cells,bool allowDiagonal)
        {
            return cells.SelectMany(c => Neighbours(c, allowDiagonal)).Distinct();
        }

        static IEnumerable<CPos> Neighbours(CPos c,bool allowDiagonal)
        {
            yield return c;
            yield return new CPos(c.X - 1, c.Y);
            yield return new CPos(c.X + 1, c.Y);
            yield return new CPos(c.X, c.Y - 1);
            yield return new CPos(c.X, c.Y + 1);

            if (allowDiagonal)
            {
                yield return new CPos(c.X - 1, c.Y - 1);
                yield return new CPos(c.X + 1, c.Y - 1);
                yield return new CPos(c.X - 1, c.Y + 1);
                yield return new CPos(c.X + 1, c.Y + 1);
            }
        }
    }
}