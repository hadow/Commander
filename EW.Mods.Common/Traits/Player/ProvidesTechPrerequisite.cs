﻿using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ProvidesTechPrerequisiteInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ProvidesTechPrerequisite(); }
    }
    class ProvidesTechPrerequisite
    {
    }
}