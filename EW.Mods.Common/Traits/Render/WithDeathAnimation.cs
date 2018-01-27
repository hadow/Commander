using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class WithDeathAnimationInfo : ConditionalTraitInfo,Requires<RenderSpritesInfo>
    {
        [SequenceReference(null,true)]
        public readonly string DeathSequence = "die";

        [PaletteReference("DeathPaletteIsPlayerPalette")]
        public readonly string DeathSequencePalette = "player";

        public readonly bool DeathPaletteIsPlayerPalette = true;

        public readonly bool UseDeathTypeSuffix = true;

        /// <summary>
        /// Sequence to play when this actor is crushed.
        /// </summary>
        [SequenceReference]
        public readonly string CrushedSequence = null;

        [PaletteReference("CrushedPaletteIsPlayerPalette")]
        public readonly string CrushedSequencePalette = "effect";

        public readonly bool CrushedPaletteIsPlayerPalette = false;


        public readonly Dictionary<string, string[]> DeathTypes = new Dictionary<string, string[]>();

        [SequenceReference]
        public readonly string FallbackSequence = null;

        public override object Create(ActorInitializer init)
        {
            return new WithDeathAnimation(init.Self, this);
        }
    }
    public class WithDeathAnimation:ConditionalTrait<WithDeathAnimationInfo>,INotifyKilled,INotifyCrushed
    {

        readonly RenderSprites rs;

        bool crushed;

        public WithDeathAnimation(Actor self,WithDeathAnimationInfo info) : base(info)
        {
            rs = self.Trait<RenderSprites>();
        }


        public void Killed(Actor self,AttackInfo attackInfo)
        {
            //Actors with Crushable trait will spawn CrushedSequence.
            if (crushed || IsTraitDisabled)
                return;

            var palette = Info.DeathSequencePalette;
            if (Info.DeathPaletteIsPlayerPalette)
                palette += self.Owner.InternalName;

            //Killed by some non-standard means
            if(attackInfo.Damage.DamageTypes.Count == 0)
            {
                if (Info.FallbackSequence != null)
                    SpawnDeathAnimation(self, self.CenterPosition, rs.GetImage(self), Info.FallbackSequence, palette);

                return;
            }

            var sequence = Info.DeathSequence;

            if (Info.UseDeathTypeSuffix)
            {
                var damageType = Info.DeathTypes.Keys.FirstOrDefault(attackInfo.Damage.DamageTypes.Contains);
                if (damageType == null)
                    return;

                sequence += Info.DeathTypes[damageType].Random(self.World.SharedRandom);
            }

            SpawnDeathAnimation(self, self.CenterPosition, rs.GetImage(self), sequence, palette);
        }

        void INotifyCrushed.OnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {
            crushed = true;
            if (Info.CrushedSequence == null)
                return;

            var crushPalette = Info.CrushedSequencePalette;
            if (Info.CrushedPaletteIsPlayerPalette)
                crushPalette += self.Owner.InternalName;

            SpawnDeathAnimation(self, self.CenterPosition, rs.GetImage(self), Info.CrushedSequence, crushPalette);
        }


        void INotifyCrushed.WarnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {

        }


        public void SpawnDeathAnimation(Actor self,WPos pos,string image,string sequence,string palette)
        {
            self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, w, image, sequence, palette)));
        }




    }
}