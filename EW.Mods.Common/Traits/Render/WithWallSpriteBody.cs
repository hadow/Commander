using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    interface IWallConnectorInfo : ITraitInfoInterface
    {
        string GetWallConnectionType();
    }

    class WithWallSpriteBodyInfo : WithSpriteBodyInfo,IWallConnectorInfo,Requires<BuildingInfo>
    {
        public readonly string Type = "wall";
        public override object Create(ActorInitializer init)
        {
            return new WithWallSpriteBody(init, this);
        }

        string IWallConnectorInfo.GetWallConnectionType()
        {
            return Type;
        }
        
    }
    class WithWallSpriteBody:WithSpriteBody,INotifyRemovedFromWorld,IWallConnector,ITick
    {
        readonly WithWallSpriteBodyInfo wallInfo;

        int adjacent = 0;
        bool dirty = true;

        public WithWallSpriteBody(ActorInitializer init,WithWallSpriteBodyInfo info) : base(init, info, () => 0)
        {
            wallInfo = info;
        }

        bool IWallConnector.AdjacentWallCanConnect(Actor self, CPos wallLocation, string wallType, out CVec facing)
        {
            facing = wallLocation - self.Location;
            return wallInfo.Type == wallType && Math.Abs(facing.X) + Math.Abs(facing.Y) == 1;
        }

        void IWallConnector.SetDirty()
        {
            dirty = true;
        }

        void ITick.Tick(Actor self)
        {
            if (!dirty)
                return;

            //Update connection to neighbours
            var adjacentActors = CVec.Directions.SelectMany(dir => self.World.ActorMap.GetActorsAt(self.Location + dir));

            adjacent = 0;

            foreach(var a in adjacentActors)
            {
                CVec facing;
                var wc = a.TraitsImplementing<IWallConnector>().FirstEnabledTraitOrDefault();
                if (wc == null || !wc.AdjacentWallCanConnect(a, self.Location, wallInfo.Type, out facing))
                    continue;

                if (facing.Y > 0)
                    adjacent |= 1;
                else if (facing.X < 0)
                    adjacent |= 2;
                else if (facing.Y < 0)
                    adjacent |= 4;
                else if (facing.X > 0)
                    adjacent |= 8;

            }

            dirty = false;
        }


        protected override void OnBuildComplete(Actor self)
        {
            DefaultAnimation.PlayFetchIndex(NormalizeSequence(self, Info.Sequence), () => adjacent);
            UpdateNeighbours(self);

            self.World.AddFrameEndTask(_ => DefaultAnimation.Tick());
        }


        static void UpdateNeighbours(Actor self)
        {
            var adjacentActorTraits = CVec.Directions.SelectMany(dir => self.World.ActorMap.GetActorsAt(self.Location + dir))
                .SelectMany(a => a.TraitsImplementing<IWallConnector>());

            foreach (var aat in adjacentActorTraits)
                aat.SetDirty();
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            UpdateNeighbours(self);
        }

        protected override void TraitEnabled(Actor self)
        {
            dirty = true;
        }
    }
}