using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{

    public enum TrailTye { Cell, CenterPosition };
    /// <summary>
    /// Renders a sprite effect when leaving a cell.
    /// </summary>

    public class LeavesTrailsInfo : ConditionalTraitInfo
    {
        public readonly string Image = null;

        [SequenceReference("Image")]
        public readonly string[] Sequences = { "idle" };

        [PaletteReference]
        public readonly string Palette = "effect";

        public readonly HashSet<string> TerrainTypes = new HashSet<string>();


        public readonly bool VisibleThroughFog = false;


        public readonly TrailTye Type = TrailTye.Cell;

        public readonly bool TrailWhileStationary = false;

        public readonly int StationaryInterval = 0;

        public readonly bool TrailWhileMoving = false;

        public readonly int MovingInterval = 0;

        public readonly int StartDelay = 0;

        public readonly WVec[] Offsets = { WVec.Zero };

        public readonly bool SpawnAtLastPosition = true;

        public override object Create(ActorInitializer init)
        {
            return new LeavesTrails(init.Self, this);
        }
    }
    public class LeavesTrails:ConditionalTrait<LeavesTrailsInfo>,ITick
    {
        BodyOrientation body;
        IFacing facing;
        int cachedFacing;
        int cachedInterval;
        WPos cachedPosition;

        int ticks;
        int offset;
        bool wasStationary;
        bool isMoving;

        public LeavesTrails(Actor self,LeavesTrailsInfo info) : base(info)
        {
            cachedInterval = Info.StartDelay;
        }

        protected override void Created(Actor self){
            
            body = self.Trait<BodyOrientation>();
            facing = self.TraitOrDefault<IFacing>();
            cachedFacing = facing != null ? facing.Facing : 0;
            cachedPosition = self.CenterPosition;

        }

        void ITick.Tick(Actor self){

            if (IsTraitDisabled)
                return;

            wasStationary = !isMoving;
            isMoving = self.CenterPosition != cachedPosition;

            if ((isMoving && !Info.TrailWhileMoving) || (!isMoving && !Info.TrailWhileStationary))
                return;

            if(isMoving == wasStationary && (Info.StartDelay > -1)){
                cachedInterval = Info.StartDelay;
                ticks = 0;
            }

            if(++ticks>=cachedInterval){

                var spawnCell = Info.SpawnAtLastPosition ? self.World.Map.CellContaining(cachedPosition) : self.World.Map.CellContaining(self.CenterPosition);
                var type = self.World.Map.GetTerrainInfo(spawnCell).Type;

                if (++offset >= Info.Offsets.Length)
                    offset = 0;

                var offsetRotation = Info.Offsets[offset].Rotate(body.QuantizeOrientation(self, self.Orientation));
                var spawnPosition = Info.SpawnAtLastPosition ? cachedPosition : self.CenterPosition;

                var pos = Info.Type == TrailTye.CenterPosition ? spawnPosition + body.LocalToWorld(offsetRotation) : self.World.Map.CenterOfCell(spawnCell);

                var spawnFacing = Info.SpawnAtLastPosition ? cachedFacing : (facing != null ? facing.Facing : 0);

                if(Info.TerrainTypes.Contains(type) && !string.IsNullOrEmpty(Info.Image))
                {
                    self.World.AddFrameEndTask(w=>w.Add(new SpriteEffect(pos,self.World,Info.Image,
                                                                         Info.Sequences.Random(WarGame.CosmeticRandom),Info.Palette,Info.VisibleThroughFog,false,spawnFacing)));
                }

                cachedPosition = self.CenterPosition;
                cachedFacing = facing != null ? facing.Facing : 0;

                ticks = 0;
                cachedInterval = isMoving ? Info.MovingInterval : Info.StationaryInterval;
            }

        }


        protected override void TraitEnabled(Actor self)
        {
            cachedPosition = self.CenterPosition;

        }
    }
}