using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public class ResourceLayerInfo : ITraitInfo, Requires<BuildingInfluenceInfo>,Requires<ResourceTypeInfo>
    {
        public object Create(ActorInitializer init) { return new ResourceLayer(init.Self); }
    }

    public class ResourceLayer:IRenderOverlay,IWorldLoaded,ITickRender,INotifyActorDisposing
    {
        readonly World world;
        
        readonly HashSet<CPos> dirty = new HashSet<CPos>();

        readonly BuildingInfluence buildingInfluence;

        readonly Dictionary<PaletteReference, TerrainSpriteLayer> spriteLayers = new Dictionary<PaletteReference, TerrainSpriteLayer>();

        protected readonly CellLayer<CellContents> Content;
        protected readonly CellLayer<CellContents> RenderContent;


        public ResourceLayer(Actor self)
        {
            world = self.World;

            buildingInfluence = self.Trait<BuildingInfluence>();

            Content = new CellLayer<CellContents>(world.Map);
            RenderContent = new CellLayer<CellContents>(world.Map);

            RenderContent.CellEntryChanged += UpdateSpriteLayer;
        }


        void UpdateSpriteLayer(CPos cell)
        {

        }
        public void WorldLoaded(World w,WorldRenderer wr)
        {
            var resources = w.WorldActor.TraitsImplementing<ResourceType>().ToDictionary(r => r.Info.ResourceType, r => r);

            foreach(var r in resources)
            {
                var layer = spriteLayers.GetOrAdd(r.Value.Palette, pal =>
                {
                    var first = r.Value.Variants.First().Value.First();

                    return new TerrainSpriteLayer(w, wr, first.Sheet, first.BlendMode, pal, wr.World.Type != WorldT.Editor);
                });
            }
        }

        CellContents CreateResourceCell(ResourceType t,CPos cell)
        {
            world.Map.CustomTerrain[cell] = world.Map.Rules.TileSet.GetTerrainIndex(t.Info.TerrainType);

            return new CellContents
            {
                Type = t,
                Variant = ChooseRandomVariant(t),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool AllowResourceAt(ResourceType rt,CPos cell)
        {
            if (!world.Map.Contains(cell))
                return false;

            if (!rt.Info.AllowedTerrainTypes.Contains(world.Map.GetTerrainInfo(cell).Type))
                return false;

            return true;

        }

        protected virtual string ChooseRandomVariant(ResourceType t)
        {
            return t.Variants.Keys.Random(WarGame.CosmeticRandom);
        }
        public void Render(WorldRenderer wr)
        {

        }


        public void TickRender(WorldRenderer wr,Actor self)
        {

        }

        public void Disposing(Actor self)
        {

        }



        public struct CellContents
        {
            public static readonly CellContents Empty = new CellContents();

            public ResourceType Type;

            public int Density;

            public string Variant;

            public Sprite Sprite;
        }
    }
}