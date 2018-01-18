using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Support;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.AI
{
    public enum SquadT
    {
        Assault,
        Air,
        Rush,
        Protection,
        Naval
    }
    public class Squad
    {
        public List<Actor> Units = new List<Actor>();

        public SquadT Type;

        internal World World;

        internal HackyAI Bot;

        internal MersenneTwister Random;

        internal Target Target;

        internal StateMachine FuzzyStateMachine;


        public Squad(HackyAI bot,SquadT type):this(bot,type,null){}


        public Squad(HackyAI bot,SquadT type,Actor target)
        {
            Bot = bot;
            World = bot.World;
            Random = bot.Random;
            Type = type;
            Target = Target.FromActor(target);
            FuzzyStateMachine = new StateMachine();

            switch (type)
            {
                case SquadT.Assault:
                case SquadT.Rush:
                    FuzzyStateMachine.ChangeState(this, new GroundUnitsIdleState(), true);
                    break;
                case SquadT.Air:

                    break;
                case SquadT.Protection:

                    break;
            }
        }

        public void Update()
        {
            if (IsValid)
                FuzzyStateMachine.Update(this);
        }

        public bool IsValid{ get { return Units.Any();}}


        public Actor TargetActor{
            get { return Target.Actor; }
            set{
                Target = Target.FromActor(value);
            }
        }

        public bool IsTargetValid{
            get{
                return Target.IsValidFor(Units.FirstOrDefault()) && !Target.Actor.Info.HasTraitInfo<HuskInfo>();
            }
        }

        public bool IsTargetVisible{
            get{
                return TargetActor.CanBeViewedByPlayer(Bot.Player);
            }
        }

        public WPos CenterPosition
        {
            get
            {
                return Units.Select(u => u.CenterPosition).Average();
            }
        }

    }
}