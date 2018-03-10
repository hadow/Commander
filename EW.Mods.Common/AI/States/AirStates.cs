using System;
namespace EW.Mods.Common.AI
{



    abstract class AirStateBase:StateBase{

        static readonly string[] AirTargetTypes = new[] { "Air" };
        protected const int MissileUnitMultiplier = 3;


    }


    class AirIdleState:AirStateBase,IState{

        void IState.Activate(Squad bot){}
        void IState.Tick(Squad bot){


        }

        void IState.Deactivate(Squad bot){
            
        }
    }

    class AirAttackState:AirStateBase,IState{

        void IState.Activate(Squad bot){}

        void IState.Tick(Squad bot){


        }

        void IState.Deactivate(Squad bot){}
    }

    class AirFleeState:AirStateBase,IState{

        void IState.Activate(Squad bot){}

        void IState.Tick(Squad bot){


        }
        void IState.Deactivate(Squad bot){}
    }
}
