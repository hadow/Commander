using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    class WithCrateBodyInfo : ITraitInfo, Requires<RenderSpritesInfo>
    {

        [Desc("Easteregg sequences to use in December.")]
        public readonly string[] XmasImages = { };

        [Desc("Terrain types on which to display WaterSequence.")]
        public readonly HashSet<string> WaterTerrainTypes = new HashSet<string> { "Water" };

        [SequenceReference] public readonly string IdleSequence = "idle";
        [SequenceReference] public readonly string WaterSequence = null;
        [SequenceReference] public readonly string LandSequence = null;

        public object Create(ActorInitializer init)
        {
            return new WithCrateBody(init.Self,this);
        }
    }

    class WithCrateBody:INotifyAddedToWorld,INotifyParachute
    {

        readonly Actor self;
        readonly Animation anim;
        readonly WithCrateBodyInfo info;


        public WithCrateBody(Actor self, WithCrateBodyInfo info)
        {
            this.self = self;
            this.info = info;

            var rs = self.Trait<RenderSprites>();
            var image = rs.GetImage(self);
            var images = info.XmasImages.Any() && DateTime.Today.Month == 12 ? info.XmasImages : new[] { image };

            anim = new Animation(self.World, images.Random(WarGame.CosmeticRandom));
            anim.Play(info.IdleSequence);
            rs.Add(anim);
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self){

            // Don't change animations while still in air
            if (!self.IsAtGroundLevel())
                return;

            PlaySequence();

        }

        void INotifyParachute.OnParachute(Actor self) { }

        void INotifyParachute.OnLanded(Actor self, Actor ignore)
        {
            PlaySequence();
        }


        void PlaySequence()
        {
            var onWater = info.WaterTerrainTypes.Contains(self.World.Map.GetTerrainInfo(self.Location).Type);
            var sequence = onWater ? info.WaterSequence : info.LandSequence;
            if (!string.IsNullOrEmpty(sequence))
                anim.PlayRepeating(sequence);
        }
    }
}