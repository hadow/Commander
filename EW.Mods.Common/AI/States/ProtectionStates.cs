using System;
namespace EW.Mods.Common.AI
{

    class UnitsForProtectionIdleState:GroundStatesBase,IState{

        void IState.Activate(Squad bot){}

        void IState.Deactivate(Squad bot){}

        void IState.Tick(Squad bot){
            bot.FuzzyStateMachine.ChangeState(bot, new UnitsForProtectionAttackState(), true);
        }
    }

    class UnitsForProtectionAttackState:GroundStatesBase,IState{

        void IState.Activate(Squad bot){


        }

        void IState.Deactivate(Squad bot){


        }

        void IState.Tick(Squad bot){


        }
    }


    class UnitsForProtectionFleeState:GroundStatesBase,IState{

        void IState.Activate(Squad bot){}

        void IState.Deactivate(Squad bot){}

        void IState.Tick(Squad bot){
            
        }
    }
}
