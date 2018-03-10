using System;
using System.Linq;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.AI
{
    abstract class GroundStatesBase:StateBase
    {
        /// <summary>
        /// 逃跑
        /// </summary>
        /// <returns><c>true</c>, if flee was shoulded, <c>false</c> otherwise.</returns>
        /// <param name="owner">Owner.</param>
        protected virtual bool ShouldFlee(Squad owner){

            return base.ShouldFlee(owner, enemies => !AttackOrFleeFuzzy.Default.CanAttack(owner.Units, enemies));
        }

        /// <summary>
        /// 查找最近的敌人
        /// </summary>
        /// <returns>The closest enemy.</returns>
        /// <param name="owner">Owner.</param>
        protected Actor FindClosestEnemy(Squad owner){

            return owner.Bot.FindClosestEnemy(owner.Units.FirstOrDefault().CenterPosition);
        }
    }

    /// <summary>
    /// Ground units idle state.
    /// </summary>
    class GroundUnitsIdleState:GroundStatesBase,IState
    {
        public void Activate(Squad owner){}

        public void Tick(Squad owner){

            if (!owner.IsValid)
                return;

            if(!owner.IsTargetValid)
            {
                var closestEnemy = FindClosestEnemy(owner);
                if (closestEnemy == null)
                    return;

                owner.TargetActor = closestEnemy;
            }

            var enemyUnits = owner.World.FindActorsInCircle(owner.TargetActor.CenterPosition, WDist.FromCells(owner.Bot.Info.IdleScanRadius))
                                  .Where(unit => owner.Bot.Player.Stances[unit.Owner] == Stance.Enemy).ToList();

            if (enemyUnits.Count == 0)
                return;
            if(AttackOrFleeFuzzy.Default.CanAttack(owner.Units,enemyUnits))
            {
                foreach(var u in owner.Units){
                    owner.Bot.QueueOrder(new Order("AttackMove",u,Target.FromCell(owner.World,owner.TargetActor.Location),false));
                }

                //we have gathered sufficient units.Attack the nearest enemy unit.
                owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsAttackMoveState(),true);
            }
            else{
                owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsFleeState(),true);
            }
        }

        public void Deactivate(Squad owner){}

    }

    /// <summary>
    /// Ground units attack move state.
    /// </summary>
    class GroundUnitsAttackMoveState:GroundStatesBase,IState{

        public void Activate(Squad owner){}

        public void Tick(Squad owner)
        {
            if (!owner.IsValid)
                return;

            if(!owner.IsTargetValid)
            {
                var closestEnemy = FindClosestEnemy(owner);
                if (closestEnemy != null)
                    owner.TargetActor = closestEnemy;
                else
                {
                    owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsFleeState(),true);
                    return;
                }
            }

            var leader = owner.Units.ClosestTo(owner.TargetActor.CenterPosition);
            if (leader == null)
                return;

            var ownUnits = owner.World.FindActorsInCircle(leader.CenterPosition, WDist.FromCells(owner.Units.Count) / 3)
                                .Where(a => a.Owner == owner.Units.First().Owner && owner.Units.Contains(a)).ToHashSet();

            if(ownUnits.Count < owner.Units.Count){

                owner.Bot.QueueOrder(new Order("Stop",leader,false));

                //Since units have different movement speeds,they get separated while approaching the target.
                //let them regroup into tighter formation.
                //由于单位有不同的移动速度，在接近目标时分离，重新组成更紧密的队形
                foreach (var unit in owner.Units.Where(a => !ownUnits.Contains(a)))
                    owner.Bot.QueueOrder(new Order("AttackMove", unit, Target.FromCell(owner.World, leader.Location), false));
                
            }
            else{

                var enemies = owner.World.FindActorsInCircle(leader.CenterPosition, WDist.FromCells(owner.Bot.Info.AttackScanRadius))
                                   .Where(a => !a.IsDead && leader.Owner.Stances[a.Owner] == Stance.Enemy && a.GetEnabledTargetTypes().Any());

                var target = enemies.ClosestTo(leader.CenterPosition);
                if(target != null){
                    owner.TargetActor = target;
                    owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsAttackState(),true);

                }
                else{
                    foreach (var a in owner.Units)
                        owner.Bot.QueueOrder(new Order("AttackMove", a, Target.FromCell(owner.World, owner.TargetActor.Location), false));
                }

                if (ShouldFlee(owner))
                    owner.FuzzyStateMachine.ChangeState(owner, new GroundUnitsFleeState(), true);
            }
        }

        public void Deactivate(Squad owner){}
    }

    /// <summary>
    /// Ground units attack state.
    /// </summary>
    class GroundUnitsAttackState:GroundStatesBase,IState{

        public void Activate(Squad owner){}

        public void Tick(Squad owner){

            if (!owner.IsValid)
                return;
            if(!owner.IsTargetValid){

                var closestEnemy = FindClosestEnemy(owner);
                if (closestEnemy != null)
                    owner.TargetActor = closestEnemy;
                else{
                    owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsFleeState(),true);
                    return;
                }
            }

            foreach(var a in owner.Units){
                if (!BusyAttack(a))
                    owner.Bot.QueueOrder(new Order("Attack", a, Target.FromActor(owner.TargetActor), false));
            }

            if (ShouldFlee(owner))
                owner.FuzzyStateMachine.ChangeState(owner, new GroundUnitsFleeState(), true);
        }

        public void Deactivate(Squad owner){}
    }

    /// <summary>
    /// Ground units flee state.
    /// </summary>
    class GroundUnitsFleeState:GroundStatesBase,IState{
        
        public void Activate(Squad owner){}

        public void Tick(Squad owner){

            if (!owner.IsValid)
                return;

            GoToRandomOwnBuilding(owner);
            owner.FuzzyStateMachine.ChangeState(owner,new GroundUnitsIdleState(),true);

        }

        public void Deactivate(Squad owner){


        }
    }

}
