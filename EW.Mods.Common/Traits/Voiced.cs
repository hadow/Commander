using System;
using System.Collections.Generic;

using EW.Traits;
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