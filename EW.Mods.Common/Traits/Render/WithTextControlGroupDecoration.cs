using System;
using EW.Graphics;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    public class WithTextControlGroupDecorationInfo:ITraitInfo
    {

        public object Create(ActorInitializer init) { return new WithTextControlGroupDecoration(); }
    }


    public class WithTextControlGroupDecoration
    {

    }
}