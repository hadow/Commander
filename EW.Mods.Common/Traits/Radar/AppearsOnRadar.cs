﻿using System;


namespace EW.Mods.Common.Traits
{

    public class AppearsOnRadarInfo : ITraitInfo
    {

        public object Create(ActorInitializer init) { return new AppearsOnRadar(); }
    }

    public class AppearsOnRadar
    {
    }
}