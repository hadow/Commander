﻿using System;


namespace EW.Mods.Common.Traits
{


    public class BuildingInfluenceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new BuildingInfluence();
        }
    }
    public class BuildingInfluence
    {
    }
}