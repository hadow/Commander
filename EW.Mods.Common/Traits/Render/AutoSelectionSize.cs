using System;
using EW.Traits;
using EW.Framework;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Automatically calculates the targetable area and screen map boundaries from the sprite size.
    /// 根据精灵大小自动计算可定位区域和屏幕图边界。
    /// </summary>
    public class AutoSelectionSizeInfo:ITraitInfo,Requires<RenderSpritesInfo>{

        public object Create(ActorInitializer init){
            return new AutoSelectionSize(this);
        }
    }


    public class AutoSelectionSize:IAutoSelectionSize
    {
        public AutoSelectionSize(AutoSelectionSizeInfo info)
        {
        }

        public Int2 SelectionSize(Actor self)
        {
            var rs = self.Trait<RenderSprites>();
            return rs.AutoSelectionSize(self);
        }
    }
}
