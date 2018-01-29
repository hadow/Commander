using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Primitives;

namespace EW.Mods.Common.Traits
{

    public class AppearsOnRadarInfo : ConditionalTraitInfo
    {
        public readonly bool UseLocation = false;

        /// <summary>
        /// Player stances who can view this actor on radar.
        /// </summary>
        public readonly Stance ValidStances = Stance.Ally | Stance.Neutral | Stance.Enemy;

        public override object Create(ActorInitializer init)
        {
            return new AppearsOnRadar(this);
        }
    }

    public class AppearsOnRadar:ConditionalTrait<AppearsOnRadarInfo>,IRadarSignature
    {
        IRadarColorModifier modifier;

        public AppearsOnRadar(AppearsOnRadarInfo info) : base(info) { }


        protected override void Created(Actor self)
        {
            base.Created(self);
            modifier = self.TraitsImplementing<IRadarColorModifier>().FirstOrDefault();
        }


        public void PopulateRadarSignatureCells(Actor self,List<Pair<CPos,Color>> destinationBuffer)
        {
            var viewer = self.World.RenderPlayer ?? self.World.LocalPlayer;

            if (IsTraitDisabled || (viewer != null && !Info.ValidStances.HasStance(self.Owner.Stances[viewer])))
                return;

            var color = WarGame.Settings.Game.UsePlayerStanceColor ? self.Owner.PlayerStanceColor(self) : self.Owner.Color.RGB;
            if (modifier != null)
                color = modifier.RadarColorOverride(self, color);

            if(Info.UseLocation)
            {
                destinationBuffer.Add(Pair.New(self.Location, color));
                return;
            }

            foreach (var cell in self.OccupiesSpace.OccupiedCells())
                destinationBuffer.Add(Pair.New(cell.First, color));
        }
    }
}