using System;


namespace EW.Mods.Common.AI
{
    interface IState
    {
        void Activate(Squad bot);
        void Tick(Squad bot);
        void Deactive(Squad bot);
    }
    class StateMachine
    {
        IState currentState;
        IState previousState;

        public void Update(Squad squad)
        {
            if (currentState != null)
                currentState.Tick(squad);
        }
    }
}