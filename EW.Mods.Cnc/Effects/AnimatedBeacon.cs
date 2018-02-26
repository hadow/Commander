using System;
using System.Collections.Generic;
using System.Linq;
using EW.Effects;
using EW.Graphics;
using EW.Scripting;


namespace EW.Mods.Cnc.Effects
{
    public class AnimatedBeacon:IEffect,IEffectAboveShroud
    {


        readonly Player owner;
        readonly WPos position;
        readonly string beaconPalette;
        readonly bool isPlayerPalette;
        readonly Animation beacon;
        readonly int duration;

        int delay;
        int tick;

        public AnimatedBeacon(Player owner,WPos position,int duration,
            string beaconPalette,bool isPlayerPalette,string beaconImage,string beaconSequence,int delay = 0)
        {
            this.owner = owner;
            this.position = position;
            this.beaconPalette = beaconPalette;
            this.isPlayerPalette = isPlayerPalette;
            this.duration = duration;
            this.delay = delay;

            if(!string.IsNullOrEmpty(beaconSequence))
            {
                beacon = new Animation(owner.World, beaconImage);
                beacon.PlayRepeating(beaconSequence);
            }
        }


        void IEffect.Tick(World world)
        {
            if (delay-- > 0)
                return;

            if (beacon != null)
                beacon.Tick();

            if (duration > 0 && duration <= tick++)
                owner.World.AddFrameEndTask(w => w.Remove(this));
        }

        IEnumerable<IRenderable> IEffect.Render(WorldRenderer wr)
        {
            return SpriteRenderable.None;
        }

        IEnumerable<IRenderable> IEffectAboveShroud.RenderAboveShroud(WorldRenderer wr)
        {
            if (delay > 0)
                return SpriteRenderable.None;

            if (beacon == null)
                return SpriteRenderable.None;

            if (!owner.IsAlliedWith(owner.World.RenderPlayer))
                return SpriteRenderable.None;

            var palette = wr.Palette(isPlayerPalette ? beaconPalette + owner.InternalName : beaconPalette);
            return beacon.Render(position, palette);
        }


    }
}