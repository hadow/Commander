using System;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    public class FreeActorInfo : ITraitInfo
    {
        [ActorReference,FieldLoader.Require]
        public readonly string Actor = null;

        public readonly CVec SpawnOffset = CVec.Zero;

        /// <summary>
        /// Which direction the unit should face.
        /// </summary>
        public readonly int Facing = 0;
        

        public virtual object Create(ActorInitializer init) { return new FreeActor(init,this); }
    }
    public class FreeActor
    {

        public FreeActor(ActorInitializer init,FreeActorInfo info)
        {
            if (init.Contains<FreeActorInit>() && !init.Get<FreeActorInit>().ActorValue)
                return;

            init.Self.World.AddFrameEndTask(w =>
            {

                w.CreateActor(info.Actor, new TypeDictionary
                {
                    new ParentActorInit(init.Self),
                    new LocationInit(init.Self.Location+info.SpawnOffset),
                    new OwnerInit(init.Self.Owner),
                    new FacingInit(info.Facing),
                });

            });
        }
    }


    public class FreeActorInit : IActorInit<bool>
    {
        [FieldFromYamlKey]
        public readonly bool ActorValue = true;

        public FreeActorInit(bool init) { ActorValue = init; }

        public bool Value(World world) { return ActorValue; }
    }

    public class ParentActorInit : IActorInit<Actor>
    {
        public readonly Actor ActorValue;

        public ParentActorInit(Actor parent) { ActorValue = parent; }

        public Actor Value(World world) { return ActorValue; }
    }
}