using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    [Desc("Defines the FMVs that can be played by missions.")]
    public class MissionDataInfo : TraitInfo<MissionData>
    {
        [Desc("Briefing text displayed in the mission browser.")]
        public readonly string Briefing;

        [Desc("Played by the \"Background Info\" button in the mission browser.")]
        public readonly string BackgroundVideo;

        [Desc("Played by the \"Briefing\" button in the mission browser.")]
        public readonly string BriefingVideo;

        [Desc("Automatically played before starting the mission.")]
        public readonly string StartVideo;

        [Desc("Automatically played when the player wins the mission.")]
        public readonly string WinVideo;

        [Desc("Automatically played when the player loses the mission.")]
        public readonly string LossVideo;
    }

    public class MissionData { }
}