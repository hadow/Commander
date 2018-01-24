using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Traits.Render;
namespace EW.Mods.Common.Traits
{

    class WithMuzzleOverlayInfo : ConditionalTraitInfo,Requires<RenderSpritesInfo>,Requires<AttackBaseInfo>,Requires<ArmamentInfo>
    {


        public readonly bool IgnoreOffset = false;

        public override object Create(ActorInitializer init)
        {
            return new WithMuzzleOverlay(init.Self, this);
        }
    }
    class WithMuzzleOverlay:ConditionalTrait<WithMuzzleOverlayInfo>,INotifyAttack,IRender,ITick
    {
        readonly Dictionary<Barrel, bool> visible = new Dictionary<Barrel, bool>();

        readonly Dictionary<Barrel, AnimationWithOffset> anims = new Dictionary<Barrel, AnimationWithOffset>();

        readonly Func<int> getFacing;

        readonly Armament[] armaments;

        public WithMuzzleOverlay(Actor self,WithMuzzleOverlayInfo info) : base(info) {

            var render = self.Trait<RenderSprites>();
            var facing = self.TraitOrDefault<IFacing>();

            armaments = self.TraitsImplementing<Armament>().Where(arm => arm.Info.MuzzleSequence != null).ToArray();

            foreach(var arm in armaments){

                foreach(var b in arm.Barrels){

                    var barrel = b;
                    var turreted = self.TraitsImplementing<Turreted>().FirstOrDefault(t => t.Name == arm.Info.Turret);

                    if (turreted != null)
                    {
                        getFacing = () => turreted.TurretFacing;

                    }
                    else if (facing != null)
                        getFacing = () => facing.Facing;
                    else
                        getFacing = () => 0;

                    var muzzleFlash = new Animation(self.World, render.GetImage(self), getFacing);
                    visible.Add(barrel,false);
                    anims.Add(barrel,new AnimationWithOffset(muzzleFlash,
                                                             ()=>info.IgnoreOffset ? WVec.Zero:arm.MuzzleOffset(self,barrel),
                                                             ()=>IsTraitDisabled || !visible[barrel],
                                                             p=>RenderUtils.ZOffsetFromCenter(self,p,2)));



                }
            }



        
        }

        void INotifyAttack.Attacking(Actor self, Target target, Armament armament, Barrel barrel)
        {

        }

        void INotifyAttack.PreparingAttack(Actor self,Target target,Armament a,Barrel barrel){}


        IEnumerable<IRenderable> IRender.Render(Actor self,WorldRenderer wr){

            foreach(var arm in armaments){

                var palette = wr.Palette(arm.Info.MuzzlePalette);
                foreach(var b in arm.Barrels){

                    var anim = anims[b];
                    if (anim.DisableFunc != null && anim.DisableFunc())
                        continue;

                    foreach(var r in anim.Render(self,wr,palette,1f)){
                        yield return r;
                    }
                }
            }
                
        }

        IEnumerable<Rectangle> IRender.ScreenBounds(Actor self,WorldRenderer wr){
            yield break;
        }


        void ITick.Tick(Actor self){

            foreach (var a in anims.Values)
                a.Animation.Tick();
        }


    }
}