using System;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits
{

    [Desc("This actor blocks bullets and missiles with 'Blockable' property.")]
    public class BlocksProjectilesInfo : ConditionalTraitInfo,IBlocksProjectilesInfo
    {
        public readonly WDist Height = WDist.FromCells(1);
        public override object Create(ActorInitializer init)
        {
            return new BlocksProjectiles(init.Self, this);
        }
    }
    public class BlocksProjectiles:ConditionalTrait<BlocksProjectilesInfo>,IBlocksProjectiles
    {
        public BlocksProjectiles(Actor self,BlocksProjectilesInfo info) : base(info) { }

        WDist IBlocksProjectiles.BlockingHeight { get { return Info.Height; } }


        public static bool AnyBlockingActorsBetween(World world,WPos start,WPos end,WDist width,WDist overscan,out WPos hit)
        {

            var actors = world.FindActorsOnLine(start, end, width, overscan);
            var length = (end - start).Length;

            foreach(var a in actors)
            {
                var blockers = a.TraitsImplementing<IBlocksProjectiles>().Where(Exts.IsTraitEnabled).ToList();

                if (!blockers.Any())
                    continue;

                var hitPos = WorldExtensions.MinimumPointLineProjection(start, end, a.CenterPosition);
                var dat = world.Map.DistanceAboveTerrain(hitPos);
                if((hitPos - start).Length < length && blockers.Any(t=>t.BlockingHeight > dat))
                {
                    hit = hitPos;
                    return true;
                }
            }
            hit = WPos.Zero;
            return false;
        }

    }
}