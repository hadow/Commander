using System;


namespace EW.Traits
{
    public enum TargetT
    {
        Invalid,
        Actor,
        Terrain,
        FrozenActor,
    }

    public struct Target
    {
        public static readonly Target[] None = { };
        public static readonly Target Invalid = new Target { type = TargetT.Invalid };

        public static Target FromCell(World w,CPos c,SubCell subCell= SubCell.FullCell)
        {
            return new Target { pos = w.Map.CenterOfSubCell(c, subCell), type = TargetT.Terrain };
        }


        TargetT type;
        Actor actor;

        public Actor Actor { get { return actor; } }
        
        FrozenActor frozen;

        public FrozenActor FrozenActor { get { return frozen; } }
        WPos pos;
        int generation;

        public bool IsValidFor(Actor targeter)
        {
            return true;
        }

        public TargetT Type
        {
            get
            {
                if(type == TargetT.Actor)
                {
                    if (!actor.IsInWorld || actor.IsDead)
                        return TargetT.Invalid;

                    if (actor.Generation != generation)
                        return TargetT.Invalid;
                }
                return type;
            }
        }

        public WPos CenterPosition
        {
            get
            {
                switch (Type)
                {
                    case TargetT.Actor:
                        return actor.CenterPosition;
                    case TargetT.FrozenActor:
                        return actor.CenterPosition;
                    case TargetT.Terrain:
                        return pos;
                    default:
                    case TargetT.Invalid:
                        throw new InvalidOperationException("Attempting to query the position of an invalid Target");
                }
            }
        }
    }
}