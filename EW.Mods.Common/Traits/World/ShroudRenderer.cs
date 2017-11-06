using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Xna.Platforms;
using EW.Graphics;

namespace EW.Mods.Common.Traits
{
    public class ShroudRendererInfo : ITraitInfo
    {


        public readonly string Sequence = "shroud";


        public object Create(ActorInitializer init)
        {
            return new ShroudRenderer(init.World,this);
        }
    }
    public sealed class ShroudRenderer:IWorldLoaded,INotifyActorDisposing
    {

        [Flags]
        enum Edges : byte
        {


            None = 0,
            TopLef = 0x01,
            TopRight = 0x02,
            BottomRight = 0x04,
            BottomLeft = 0x08,
            AllCorners = TopLef | TopRight | BottomRight | BottomLeft,
            TopSide = 0x10,
            RightSide = 0x20,
            BottomSide = 0x40,
            LeftSide = 0x80,
            AllSides = TopSide | RightSide | BottomSide | LeftSide,
            Top = TopSide | TopLef | TopRight,
            Right = RightSide | TopRight | BottomRight,
            Bottom = BottomSide | BottomRight | BottomLeft,
            Left = LeftSide | TopLef | BottomLeft,
            All = Top | Right | Bottom | Left,
        }

        struct TileInfo{

            public readonly Vector3 ScreenPosition;

            public readonly byte Variant;

            public TileInfo(Vector3 screenPosition,byte variant){
                ScreenPosition = screenPosition;
                Variant = variant;
            }
        }
        readonly ShroudRendererInfo info;

        readonly Map map;

        readonly Edges notVisibleEdges;

        readonly byte variantStride;

        readonly CellLayer<TileInfo> tileInfos;

        readonly Sprite[] fogSprites, shroudSprites;


        readonly HashSet<PPos> cellsDirty = new HashSet<PPos>();

        readonly HashSet<PPos> cellsAndNeighborsDirty = new HashSet<PPos>();

        Shroud currentShroud;

        TerrainSpriteLayer shroudLayer, fogLayer;

        bool disposed;

        public ShroudRenderer(World world, ShroudRendererInfo info){

            this.info = info;
            this.map = world.Map;

            tileInfos = new CellLayer<TileInfo>(map);
        }


        void IWorldLoaded.WorldLoaded(World w,WorldRenderer wr){


        }


        void INotifyActorDisposing.Disposing(Actor self){

            if (disposed)
                return;

            shroudLayer.Dispose();

            fogLayer.Dispose();

            disposed = true;
        }

    }
}