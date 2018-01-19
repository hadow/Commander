using System;
using System.Linq;
using EW.Traits;
using EW.Framework;

namespace EW.Mods.Common.Traits.Render
{
    class WithGateSpriteBodyInfo:WithSpriteBodyInfo,Requires<GateInfo>
    {

        public readonly CVec[] WallConnections = { };

        public readonly string Type = "wall";

        public readonly string OpenSequence = null;

        public override object Create(ActorInitializer init)
        {
            return new WithGateSpriteBody(init, this);
        }

    }

    class WithGateSpriteBody:WithSpriteBody,INotifyRemovedFromWorld,ITick,IWallConnector
    {
        readonly WithGateSpriteBodyInfo gateBodyInfo;
        readonly Gate gate;

        bool renderOpen;
        public WithGateSpriteBody(ActorInitializer init,WithGateSpriteBodyInfo info) : base(init, info, () => 0)
        {
            gateBodyInfo = info;
            gate = init.Self.Trait<Gate>();
        }


        void ITick.Tick(Actor self)
        {
            if (gateBodyInfo.OpenSequence == null)
                return;

            if(gate.Position == gate.OpenPosition ^ renderOpen)
            {
                renderOpen = gate.Position == gate.OpenPosition;
                UpdateState(self);
            }
        }


        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            UpdateNeighbours(self);
        }

        void UpdateState(Actor self)
        {
            if (renderOpen)
                DefaultAnimation.PlayRepeating(NormalizeSequence(self, gateBodyInfo.OpenSequence));
            else
                DefaultAnimation.PlayFetchIndex(NormalizeSequence(self, Info.Sequence), GetGateFrame);
        }


        void UpdateNeighbours(Actor self)
        {
            var footprint = gate.Footprint.ToArray();
            var adjacent = Util.ExpandFootprint(footprint, true).Except(footprint).Where(self.World.Map.Contains).ToList();

            var adjacentActorTraits = adjacent.SelectMany(self.World.ActorMap.GetActorsAt).SelectMany(a => a.TraitsImplementing<IWallConnector>());

            foreach (var rb in adjacentActorTraits)
                rb.SetDirty();
        }


        int GetGateFrame()
        {
            return Int2.Lerp(0, DefaultAnimation.CurrentSequence.Length - 1, gate.Position, gate.OpenPosition);
        }

        protected override void OnBuildComplete(Actor self)
        {
            UpdateState(self);
            UpdateNeighbours(self);
        }


        bool IWallConnector.AdjacentWallCanConnect(Actor self, CPos wallLocation, string wallType, out CVec facing)
        {
            facing = wallLocation - self.Location;
            return wallType == gateBodyInfo.Type && gateBodyInfo.WallConnections.Contains(facing);
        }

        void IWallConnector.SetDirty() { }
    }
}