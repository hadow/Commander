using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class VoicedInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Voiced();
        }
    }
    public class Voiced
    {
    }
}