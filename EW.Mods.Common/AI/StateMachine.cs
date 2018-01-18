using System;


namespace EW.Mods.Common.AI
{
    interface IState
    {
        void Activate(Squad bot);
        void Tick(Squad bot);
        void Deactivate(Squad bot);
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

        public void ChangeState(Squad squad,IState newState,bool rememberPrevious){

            if (rememberPrevious)
                previousState = currentState;

            if (currentState != null)
                currentState.Deactivate(squad);

            if (newState != null)
                currentState = newState;

            if (currentState != null)
                currentState.Activate(squad);

        }

        public void RevertToPreviousState(Squad squad,bool saveCurrentState){

            ChangeState(squad,previousState,saveCurrentState);
        }

    }
}