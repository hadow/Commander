using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Primitives;
using EW.Graphics;
using EW.Mods.Common.Graphics;

namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Renders the cargo loaded into the unit.
    /// </summary>
    public class WithCargoInfo : ITraitInfo, Requires<CargoInfo>, Requires<BodyOrientationInfo>
    {
        public readonly WVec[] LocalOffset = { WVec.Zero };

        public readonly HashSet<string> DisplayTypes = new HashSet<string>();
    
        public object Create(ActorInitializer init) { return new WithCargo(init.Self,this); }
    }

    public class WithCargo:ITick,INotifyPassengerEntered,INotifyPassengerExited
    {

        readonly WithCargoInfo info;
        readonly Cargo cargo;
        readonly BodyOrientation body;
        readonly IFacing facing;
        int cachedFacing;

        public WithCargo(Actor self,WithCargoInfo info)
        {
            this.info = info;

            cargo = self.Trait<Cargo>();
            body = self.Trait<BodyOrientation>();
            facing = self.TraitOrDefault<IFacing>();

        }

        void ITick.Tick(Actor self)
        {
            if(facing.Facing != cachedFacing)
            {
                self.World.ScreenMap.AddOrUpdate(self);
                cachedFacing = facing.Facing;
            }
        }

        void INotifyPassengerEntered.OnPassengerEntered(Actor self, Actor passenger)
        {
            if(info.DisplayTypes.Contains(passenger.Trait<Passenger>().Info.CargoType))
            {
                self.World.ScreenMap.AddOrUpdate(self);
            }
        }


        void INotifyPassengerExited.OnPassengerExited(Actor self, Actor passenger)
        {
            self.World.ScreenMap.AddOrUpdate(self);
        }

        //IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
        //{
        //    var bodyOrientation = body.QuantizeOrientation(self, self.Orientation);
        //    var pos = self.CenterPosition;
        //    var i = 0;


        //}

    }
}