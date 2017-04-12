using System;
using EW.Xna.Platforms;

namespace EW.Mods.Common.Traits
{
    public class CustomSelectionSizeInfo : ITraitInfo
    {
        [FieldLoader.Require]
        public readonly int[] CustomBounds = null;

        public object Create(ActorInitializer init)
        {
            return new CustomSelectionSize(this);
        }
    }


    public class CustomSelectionSize:IAutoSelectionSize
    {
        readonly CustomSelectionSizeInfo info;


        public CustomSelectionSize(CustomSelectionSizeInfo info) { this.info = info; }
        public Vector2 SelectionSize(Actor self)
        {
            return new Vector2(info.CustomBounds[0], info.CustomBounds[1]);
        }

    }
}