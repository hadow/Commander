using System;
using System.Collections.Generic;
using System.Linq;
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


        public static Target FromPos(WPos p)
        {
            return new Target { pos = p, type = TargetT.Terrain };
        }


        public static Target FromCell(World w,CPos c,SubCell subCell= SubCell.FullCell)
        {
            return new Target { pos = w.Map.CenterOfSubCell(c, subCell), type = TargetT.Terrain };
        }

        public static Target FromActor(Actor a)
        {
            if (a == null)
                return Invalid;

            return new Target
            {
                actor = a,
                type = TargetT.Actor,
                generation = a.Generation,
            };
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

        static readonly WPos[] NoPositions = { };

        public IEnumerable<WPos> Positions
        {
            get
            {
                switch (Type)
                {
                    case TargetT.Actor:

                        if (!actor.Targetables.Any(Exts.IsTraitEnabled))
                            return new[] { actor.CenterPosition};

                        var targetablePositions = actor.TraitsImplementing<ITargetablePositions>().Where(Exts.IsTraitEnabled);
                        if (targetablePositions.Any())
                        {
                            var target = this;
                            return targetablePositions.SelectMany(tp => tp.TargetablePositions(target.actor));
                        }
                        return new[] { actor.CenterPosition };
                    case TargetT.FrozenActor:
                        return new[] { frozen.CenterPosition };
                    case TargetT.Terrain:
                        return new[] { pos };
                    default:
                    case TargetT.Invalid:
                        return NoPositions;
                }
            }
        }

        public bool IsInRange(WPos origin,WDist range)
        {
            if (Type == TargetT.Invalid)
                return false;

            return Positions.Any(t => (t - origin).HorizontalLengthSquared <= range.LengthSquared);
        }
    }
}