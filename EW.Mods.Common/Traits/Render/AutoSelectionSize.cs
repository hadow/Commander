using System;
namespace EW.Mods.Common.Traits
{
    public class AutoSelectionSizeInfo:ITraitInfo,Requires<RenderSpritesInfo>{

        public object Create(ActorInitializer init){
            return new AutoSelectionSize(this);
        }
    }


    public class AutoSelectionSize
    {
        public AutoSelectionSize(AutoSelectionSizeInfo info)
        {
        }
    }
}
