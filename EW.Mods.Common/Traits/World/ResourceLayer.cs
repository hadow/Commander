﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using EW.OpenGLES;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Attach this to the world actor. Order of the layers defines the Z sorting.
    /// </summary>
    public class ResourceLayerInfo : ITraitInfo, Requires<BuildingInfluenceInfo>,Requires<ResourceTypeInfo>
    {
        public object Create(ActorInitializer init) { return new ResourceLayer(init.Self); }
    }

    public class ResourceLayer:IRenderOverlay,IWorldLoaded,ITickRender,INotifyActorDisposing
    {

        public struct CellContents
        {
            public static readonly CellContents Empty = new CellContents();

            public ResourceType Type;

            public int Density;

            public string Variant;

            public Sprite Sprite;
        }


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

            RenderContent.CellEntryChanged += UpdateSpriteLayers;
        }


        void UpdateSpriteLayers(CPos cell)
        {
            var resource = RenderContent[cell];
            foreach(var kv in spriteLayers)
            {
                if (resource.Sprite != null && resource.Type.Palette == kv.Key)
                    kv.Value.Update(cell, resource.Sprite);
                else
                    kv.Value.Update(cell, null);
            }
        }

        public void WorldLoaded(World w,WorldRenderer wr)
        {
            var resources = w.WorldActor.TraitsImplementing<ResourceType>().ToDictionary(r => r.Info.ResourceType, r => r);

            //Build the sprite layer dictionary for rendering resources
            //All resources that have the same palette must also share a sheet and blend mode
            foreach(var r in resources)
            {
                var layer = spriteLayers.GetOrAdd(r.Value.Palette, pal =>
                {
                    var first = r.Value.Variants.First().Value.First();

                    return new TerrainSpriteLayer(w, wr, first.Sheet, first.BlendMode, pal, wr.World.Type != WorldT.Editor);
                });

                //Validate that sprites are compatible with this layer
                var sheet = layer.Sheet;
                if (r.Value.Variants.Any(kv => kv.Value.Any(s => s.Sheet != sheet)))
                    throw new InvalidDataException("Resource sprites span multiple sheets,Try loading their sequences earlier.");

                var blendMode = layer.BlendMode;
                if (r.Value.Variants.Any(kv => kv.Value.Any(s => s.BlendMode != blendMode)))
                    throw new InvalidDataException("Resource sprites specify different blend modes.Try using different palettes for resource types that use different blend modes.");
                
            }

            foreach(var cell in w.Map.AllCells)
            {
                ResourceType t;
                if (!resources.TryGetValue(w.Map.Resources[cell].Type, out t))
                    continue;

                if (!AllowResourceAt(t, cell))
                    continue;

                Content[cell] = CreateResourceCell(t, cell);
            }

            foreach(var cell in w.Map.AllCells)
            {
                var type = Content[cell].Type;
                if(type != null)
                {
                    //Set initial density based on the number of neighboring resources
                    //Adjacent includes the current cell,so is always >=1;
                    var adjacent = GetAdjacentCellsWith(type, cell);
                    var density = Int2.Lerp(0, type.Info.MaxDensity, adjacent, 9);
                    var temp = Content[cell];
                    temp.Density = Math.Max(density, 1);

                    //Initialize the RenderContent with the initial map state
                    //because the shroud may not be enabled;
                    RenderContent[cell] = Content[cell] = temp;
                    UpdateRenderedSprite(cell);
                }
            }
        }

        int GetAdjacentCellsWith(ResourceType t,CPos cell)
        {
            var sum = 0;
            for(var u = -1; u < 2; u++)
            {
                for(var v = -1; v < 2; v++)
                {
                    var c = cell + new CVec(u, v);

                    if (Content.Contains(c) && Content[c].Type == t)
                        ++sum;
                }
            }
            return sum;
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
            foreach (var kv in spriteLayers.Values)
                kv.Draw(wr.ViewPort);
        }


        public void TickRender(WorldRenderer wr,Actor self)
        {
            var remove = new List<CPos>();
            foreach(var c in dirty)
            {
                if(!self.World.FogObscures(c))
                {
                    RenderContent[c] = Content[c];
                    UpdateRenderedSprite(c);
                    remove.Add(c);
                }
            }

            foreach (var r in remove)
                dirty.Remove(r);
        }


        protected virtual void UpdateRenderedSprite(CPos cell)
        {


            var t = RenderContent[cell];
            if (t.Density > 0)
            {
                var sprites = t.Type.Variants[t.Variant];
                var frame = Int2.Lerp(0, sprites.Length - 1, t.Density, t.Type.Info.MaxDensity);
                t.Sprite = sprites[frame];
            }
            else
                t.Sprite = null;

            RenderContent[cell] = t;
        }

        bool disposed;
        public void Disposing(Actor self)
        {
            if (disposed)
                return;

            foreach (var kv in spriteLayers.Values)
                kv.Dispose();

            RenderContent.CellEntryChanged -= UpdateSpriteLayers;

            disposed = true;
        }



        
    }
}