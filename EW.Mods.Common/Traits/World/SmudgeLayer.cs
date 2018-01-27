using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EW.Traits;

using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public struct MapSmudge
    {
        public string Type;
        public int Depth;
    }

    /// <summary>
    /// Attach this to the world actor. Order of the layers defines the Z sorting.
    /// </summary>
    public class SmudgeLayerInfo : ITraitInfo
    {
        public readonly string Type = "Scorch";

        public readonly string Sequence = "scorch";//烧灼

        public readonly int SmokePercentage = 25;

        public readonly string SmokeType = "smoke_m";

        [SequenceReference("SmokeTyp")]
        public readonly string SmokeSequence = "idle";

        [PaletteReference]
        public readonly string SmokePalette = "effect";

        [PaletteReference]
        public readonly string Palette = TileSet.TerrainPaletteInternalName;

        [FieldLoader.LoadUsing("LoadInitialSmudges")]
        public readonly Dictionary<CPos, MapSmudge> InitialSmudges;


        public static object LoadInitialSmudges(MiniYaml yaml)
        {
            MiniYaml smudgeYaml;
            var nd = yaml.ToDictionary();
            var smudges = new Dictionary<CPos, MapSmudge>();
            if(nd.TryGetValue("InitialSmudges",out smudgeYaml))
            {
                foreach(var node in smudgeYaml.Nodes)
                {
                    try
                    {
                        var cell = FieldLoader.GetValue<CPos>("key", node.Key);
                        var parts = node.Value.Value.Split(',');
                        var type = parts[0];
                        var depth = FieldLoader.GetValue<int>("depth", parts[1]);
                        smudges.Add(cell, new MapSmudge { Type = type, Depth = depth });
                    }
                    catch{}
                }
            }
            return smudges;
        }

        public object Create(ActorInitializer init) { return new SmudgeLayer(init.Self,this); }
    }



    public class SmudgeLayer:IRenderOverlay,IWorldLoaded,ITickRender,INotifyActorDisposing
    {

        struct Smudge
        {
            public string Type;
            public int Depth;
            public Sprite Sprite;
        }
        public readonly SmudgeLayerInfo Info;

        readonly World world;
        TerrainSpriteLayer render;

        readonly Dictionary<string, Sprite[]> smudges = new Dictionary<string, Sprite[]>();

        readonly Dictionary<CPos, Smudge> tiles = new Dictionary<CPos, Smudge>();
        readonly Dictionary<CPos, Smudge> dirty = new Dictionary<CPos, Smudge>();

        bool disposed;

        public SmudgeLayer(Actor self,SmudgeLayerInfo info)
        {
            Info = info;
            world = self.World;

            var sequenceProvider = world.Map.Rules.Sequences;
            var types = sequenceProvider.Sequences(Info.Sequence);

            foreach(var t in types)
            {
                var seq = sequenceProvider.GetSequence(Info.Sequence, t);
                var sprites = Exts.MakeArray(seq.Length, x => seq.GetSprite(x));
                smudges.Add(t, sprites);
            }
        }


        public void WorldLoaded(World w ,WorldRenderer wr)
        {
            var first = smudges.First().Value.First();
            var sheet = first.Sheet;
            if (smudges.Values.Any(sprites => sprites.Any(s => s.Sheet != sheet)))
                throw new InvalidDataException("Resource sprites span multiple sheets,Try loading their sequences earlier.");

            var blendMode = first.BlendMode;
            if (smudges.Values.Any(sprites => sprites.Any(s => s.BlendMode != blendMode)))
                throw new InvalidDataException("Smudges specify different blend modes.Try using different smudge types for smudges that use different blend modes.");

            render = new TerrainSpriteLayer(w, wr, sheet, blendMode, wr.Palette(Info.Palette), wr.World.Type != WorldT.Editor);

            //Add map smudges
            foreach(var kv in Info.InitialSmudges)
            {
                var s = kv.Value;
                if (!smudges.ContainsKey(s.Type))
                    continue;

                var smudge = new Smudge
                {
                    Type = s.Type,
                    Depth = s.Depth,
                    Sprite = smudges[s.Type][s.Depth]

                };

                tiles.Add(kv.Key, smudge);
                render.Update(kv.Key, smudge.Sprite);
            }

        }


        void ITickRender.TickRender(WorldRenderer wr,Actor self)
        {
            var remove = new List<CPos>();
            foreach(var kv in dirty)
            {
                if (!self.World.FogObscures(kv.Key))
                {
                    //a null Sprite indicates a deleted smudge.
                    if (kv.Value.Sprite == null)
                        tiles.Remove(kv.Key);
                    else
                        tiles[kv.Key] = kv.Value;

                    render.Update(kv.Key, kv.Value.Sprite);

                    remove.Add(kv.Key);
                }
            }

            foreach(var r in remove)
            {
                dirty.Remove(r);
            }
        }


        void IRenderOverlay.Render(WorldRenderer wr)
        {
            render.Draw(wr.ViewPort);
        }

        public void AddSmudge(CPos loc){

            if (!world.Map.Contains(loc))
                return;

            if((!dirty.ContainsKey(loc) || dirty[loc].Sprite == null) && !tiles.ContainsKey(loc)){
                //no smudge; create a new one

                var st = smudges.Keys.Random(WarGame.CosmeticRandom);

                dirty[loc] = new Smudge() { Type = st, Depth = 0, Sprite = smudges[st][0] };

            }
            else{

                var tile = dirty.ContainsKey(loc) && dirty[loc].Sprite != null ? dirty[loc] : tiles[loc];
                var maxDepth = smudges[tile.Type].Length;
                if(tile.Depth< maxDepth -1){
                    tile.Depth++;
                    tile.Sprite = smudges[tile.Type][tile.Depth];
                }

                dirty[loc] = tile;

            }
        }


        public void RemoveSmudge(CPos loc){

            if (!world.Map.Contains(loc))
                return;

            var tile = dirty.ContainsKey(loc) ? dirty[loc] : new Smudge();

            tile.Sprite = null;

            dirty[loc] = tile;

        }

        void INotifyActorDisposing.Disposing(Actor self)
        {
            if (disposed)
                return;
            render.Dispose();
            disposed = true;
        }
    }
}