using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class TurretedInfo : ITraitInfo,UsesInit<TurretFacingInit>,Requires<BodyOrientationInfo>
    {
        public virtual object Create(ActorInitializer init) { return new Turreted(); }
    }
    public class Turreted:ITick,INotifyCreated,ISync,IDeathActorInitModifier
    {

    }

    public class TurretFacingInit : IActorInit<int>
    {
        [FieldLoader.FieldFromYamlKey]
        readonly int value = 128;

        public TurretFacingInit() { }

        public TurretFacingInit(int init) { value = init; }

        public int Value(World world) { return value; }
    }
}