using System;
using System.Collections.Generic;
using System.Linq;
using EW.Orders;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Common.Traits;
using EW.Framework.Touch;
namespace EW.Mods.Common.Orders
{
    public class GuardOrderGenerator:GenericSelectTarget
    {

        public GuardOrderGenerator(IEnumerable<Actor> subjects, string order, string cursor, GestureType type)
            : base(subjects, order, cursor, type) { }

        protected override IEnumerable<Order> OrderInner(World world,CPos xy,GestureSample gs){

            if (gs.GestureType != ExpectedGestureType)
                yield break;

            var target = FriendlyGuardableUnits(world, gs).FirstOrDefault();
            if (target == null || Subjects.All(s => s.IsDead))
                yield break;

            world.CancelInputMode();

            var queued = false;

            foreach (var subject in Subjects)
                if (subject != target)
                    yield return new Order(OrderName, subject, Target.FromActor(target), queued);

        }


        public override void Tick(World world)
        {
            if (Subjects.All(s => s.IsDead || !s.Info.HasTraitInfo<GuardInfo>()))
                world.CancelInputMode();
        }




        static IEnumerable<Actor> FriendlyGuardableUnits(World world,GestureSample gs){
            
            return world.ScreenMap.ActorsAtMouse(gs).Select(a => a.Actor)
                        .Where(a => !a.IsDead && a.AppearsFriendlyTo(world.LocalPlayer.PlayerActor)
                               && a.Info.HasTraitInfo<GuardableInfo>() && !world.FogObscures(a));
        }



    }
}