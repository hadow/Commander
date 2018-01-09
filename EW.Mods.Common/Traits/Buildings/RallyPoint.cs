using System;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Effects;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Used to waypoint units after production or repair is finished.
    /// 
    /// </summary>
    public class RallyPointInfo : ITraitInfo
    {

        public readonly string Image = "rallypoint";

        [SequenceReference("Image")]
        public readonly string FlagSequence = "flag";

        [SequenceReference("Image")]
        public readonly string CirclesSequence = "circles";

        public readonly string Cursor = "ability";

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = "player";


        public readonly bool IsPlayerPalette = true;

        public readonly CVec Offset = new CVec(1, 3);
        public object Create(ActorInitializer init) { return new RallyPoint(init.Self,this); }
    }

    public class RallyPoint:ISync,INotifyCreated
    {
        const uint ForceSet = 1;

        [Sync]
        public CPos Location;


        public readonly RallyPointInfo Info;

        public string PaletteName { get; private set; }


        public RallyPoint(Actor self,RallyPointInfo info)
        {
            Info = info;
            ResetLocation(self);
            PaletteName = info.IsPlayerPalette ? info.Palette + self.Owner.InternalName : info.Palette;
        }


        void INotifyCreated.Created(Actor self)
        {
            self.World.Add(new RallyPointIndicator(self, this, self.Info.TraitInfos<ExitInfo>().ToArray()));
        }


        public void ResetLocation(Actor self)
        {
            Location = self.Location + Info.Offset;
        }
    }
}