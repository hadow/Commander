using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class TransformOnCaptureInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new TransformOnCapture(); }
    }
    public class TransformOnCapture
    {
    }
}