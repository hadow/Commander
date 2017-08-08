using System;
using System.Collections.Generic;
using System.Linq;

using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class RotationPaletteEffectInfo : ITraitInfo
    {

        public readonly HashSet<string> Palettes = new HashSet<string>();

        public readonly HashSet<string> Tilesets = new HashSet<string>();

        public readonly HashSet<string> ExcludePalettes = new HashSet<string>();

        public readonly HashSet<string> ExcludeTilesets = new HashSet<string>();

        public readonly int RotationBase = 0x60;

        public readonly int RotationRange = 7;

        public readonly float RotationStep = .25f;

        public object Create(ActorInitializer init)
        {
            return new RotationPaletteEffect(init.World,this);
        }
    }
    class RotationPaletteEffect:ITick,IPaletteModifier
    {

        readonly RotationPaletteEffectInfo info;
        readonly uint[] rotationBuffer;
        readonly bool validTileset;
        readonly string tilesetId;
        float t = 0;

        public RotationPaletteEffect(World world,RotationPaletteEffectInfo info)
        {
            this.info = info;
            rotationBuffer = new uint[info.RotationRange];
            tilesetId = world.Map.Rules.TileSet.Id;

            validTileset = IsValidTileset();
        }


        public void Tick(Actor self)
        {
            if (!validTileset)
                return;

            t += info.RotationStep;
        }

        bool IsValidTileset()
        {
            if (info.Tilesets.Count == 0 && info.ExcludeTilesets.Count == 0)
                return true;

            if (info.Tilesets.Count == 0 && !info.ExcludeTilesets.Contains(tilesetId))
                return true;

            return info.Tilesets.Contains(tilesetId) && !info.ExcludeTilesets.Contains(tilesetId);
        }

        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> palettes)
        {
            if (!validTileset)
                return;

            var rotate = (int)t % info.RotationRange;
            if (rotate == 0)
                return;

            foreach(var kvp in palettes)
            {
                if ((info.Palettes.Count > 0 && !info.Palettes.Any(kvp.Key.StartsWith)) || (info.ExcludePalettes.Count > 0 && info.ExcludePalettes.Any(kvp.Key.StartsWith)))
                    continue;

                var palette = kvp.Value;

                for (var i = 0; i < info.RotationRange; i++)
                    rotationBuffer[(rotate + i) % info.RotationRange] = palette[info.RotationBase + i];

                for (var i = 0; i < info.RotationRange; i++)
                    palette[info.RotationBase + i] = rotationBuffer[i];

            }
        }

    }
}