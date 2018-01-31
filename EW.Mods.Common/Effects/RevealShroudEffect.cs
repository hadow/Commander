using System;
using System.Collections.Generic;
using System.Linq;
using EW.Effects;
using EW.Graphics;
using EW.Traits;

namespace EW.Mods.Common.Effects
{
    public class RevealShroudEffect:IEffect
    {


        static readonly PPos[] NoCells = { };

        readonly WPos pos;
        readonly Player player;
        readonly Shroud.SourceType sourceType;
        readonly WDist revealRadius;
        readonly Stance validStances;
        readonly int duration;

        int ticks;

        public RevealShroudEffect(WPos pos,WDist radius,Shroud.SourceType type,Player forPlayer,Stance stances,int delay =0,int duration = 50)
        {
            this.pos = pos;
            player = forPlayer;
            revealRadius = radius;
            validStances = stances;
            sourceType = type;
            this.duration = duration;
            ticks = -delay;
        }


        void AddCellsToPlayerShroud(Player p,PPos[] uv)
        {
            if (!validStances.HasStance(p.Stances[player]))
                return;
            p.Shroud.AddSource(this, sourceType, uv);
        }

        void RemoveCellsFromPlayerShroud(Player p)
        {
            p.Shroud.RemoveSource(this);
        }

        void IEffect.Tick(World world)
        {
            if(ticks == 0)
            {
                var cells = ProjectedCells(world);
                foreach (var p in world.Players)
                    AddCellsToPlayerShroud(p, cells);

            }

            if(ticks == duration)
            {
                foreach (var p in world.Players)
                    RemoveCellsFromPlayerShroud(p);

                world.AddFrameEndTask(w => w.Remove(this));
            }

            ticks++;
        }


        PPos[] ProjectedCells(World world)
        {
            var map = world.Map;
            var range = revealRadius;
            if (range == WDist.Zero)
                return NoCells;

            return Shroud.ProjectedCellsInRange(map, pos, range).ToArray();
        }


        IEnumerable<IRenderable> IEffect.Render(WorldRenderer wr)
        {
            return SpriteRenderable.None;
        }

    }
}