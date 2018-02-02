using System;
using System.Collections.Generic;
using EW.Traits;
using System.Drawing;
using System.Linq;
using EW.Mods.Common.Activities;
using EW.Mods.Common.Orders;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    class DemolitionInfo : ITraitInfo
    {
        [Desc("Delay to demolish the target once the explosive device is planted. " +
            "Measured in game ticks. Default is 1.8 seconds.")]
        public readonly int DetonationDelay = 45;

        [Desc("Number of times to flash the target.")]
        public readonly int Flashes = 3;

        [Desc("Delay before the flashing starts.")]
        public readonly int FlashesDelay = 4;

        [Desc("Interval between each flash.")]
        public readonly int FlashInterval = 4;

        [Desc("Duration of each flash.")]
        public readonly int FlashDuration = 3;
        
        [Desc("Voice string when planting explosive charges.")]
        [VoiceReference]
        public readonly string Voice = "Action";

        public readonly string Cursor = "c4";
        public object Create(ActorInitializer init) { return new Demolition(this); }
    }
    class Demolition
    {

        readonly DemolitionInfo info;

        public Demolition(DemolitionInfo info)
        {
            this.info = info;
        }
        
    }
}