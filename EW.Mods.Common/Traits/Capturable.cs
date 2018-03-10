using System;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;

namespace EW.Mods.Common.Traits
{
    [Desc("This actor can be captured by a unit with Captures: trait.")]
    public class CapturableInfo : ConditionalTraitInfo
    {

        public readonly HashSet<string> Types = new HashSet<string>();

        public readonly Stance ValidStance = Stance.Neutral | Stance.Enemy;

        public readonly int CaptureThreshold = 50;

        public readonly bool CancelActivity = false;


		public override object Create(ActorInitializer init)
		{
            return new Capturable(this);

		}

        public bool CanBeTargetedBy(Actor captor,Player owner){

            var c = captor.Info.TraitInfoOrDefault<CapturesInfo>();
            if (c == null)
                return false;

            var stance = owner.Stances[captor.Owner];

            if (!ValidStance.HasStance(stance))
                return false;

            if (!c.CaptureTypes.Overlaps(Types))
                return false;
            return true;
        }
	}
    public class Capturable:ConditionalTrait<CapturableInfo>,INotifyCapture
    {


        public bool BeingCaptured { get; private set; }

        public Capturable(CapturableInfo info):base(info){}


        void INotifyCapture.OnCapture(Actor self, Actor captor, Player oldOwner, Player newOwner){


            BeingCaptured = true;

            self.World.AddFrameEndTask(w => BeingCaptured = false);

            if(Info.CancelActivity){
                var stop = new Order("Stop", self, false);

                foreach (var t in self.TraitsImplementing<IResolveOrder>())
                    t.ResolveOrder(self, stop);
            }

        }

        public bool CanBeTargetedBy(Actor captor,Player owner){

            if (IsTraitDisabled)
                return false;

            return Info.CanBeTargetedBy(captor, owner);
        }


    }
}