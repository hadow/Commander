using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
namespace EW.Mods.Common
{
	public interface IRenderActorPreviewInfo : ITraitInfo
	{
        IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init);
	}



    public interface IConditionConsumerInfo:ITraitInfo{}
}
