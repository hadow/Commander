using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
	public interface IRenderActorPreviewInfo : ITraitInfo
	{
        IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init);
	}



    public interface IConditionConsumerInfo:ITraitInfo{}

    public interface IDeathActorInitModifier
    {
        void ModifyDeathActorInit(Actor self, TypeDictionary inits);
    }

    public interface IActorPreviewInitModifier
    {
        void ModifyActorPreviewInit(Actor self, TypeDictionary inits);
    }

    public interface INotifyAttack { void Attacking(Actor self, Target target, Armament a, Barrel barrel); }


    public interface IAcceptResourcesInfo : ITraitInfo { }


}
