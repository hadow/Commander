using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Support;
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
                    break;
                case SquadT.Air:

                    break;
                case SquadT.Protection:

                    break;
            }
        }

        public void Update()
        {

        }

    }
}