using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.AI
{
    abstract class StateBase
    {

        protected virtual bool ShouldFlee(Squad squad,Func<IEnumerable<Actor>,bool> flee){
            
            if (!squad.IsValid)
                return false;

            var randomSquadUnit = squad.Units.Random(squad.Random);
            var dangerRadius = squad.Bot.Info.DangerScanRadius;
            var units = squad.World.FindActorsInCircle(randomSquadUnit.CenterPosition, WDist.FromCells(dangerRadius)).ToList();

            //If there are any own building within the DangerRadius,don't flee
            //PERF:Avoid LINQ
            foreach(var u in units){
                if (u.Owner == squad.Bot.Player && u.Info.HasTraitInfo<BuildingInfo>())
                    return false;
                
            }

            var enemyAroundUnit = units.Where(unit => squad.Bot.Player.Stances[unit.Owner] == Stance.Enemy && unit.Info.HasTraitInfo<AttackBaseInfo>());
            if (!enemyAroundUnit.Any())
                return false;

            return flee(enemyAroundUnit);
        }


        public static bool BusyAttack(Actor a){

            if (a.IsIdle)
                return false;

            var activity = a.CurrentActivity;
            var type = activity.GetType();
            if (type == typeof(Attack) || type == typeof(FlyAttack))
                return true;

            var next = activity.NextActivity;
            if (next == null)
                return false;

            var nextType = next.GetType();
            if (nextType == typeof(Attack) || nextType == typeof(FlyAttack))
                return true;

            return false;
        }

        /// <summary>
        /// Gos to random own building.
        /// </summary>
        /// <param name="squad">Squad.</param>
        protected static void GoToRandomOwnBuilding(Squad squad){

            var loc = RandomBuildingLocation(squad);
            foreach (var a in squad.Units)
                squad.Bot.QueueOrder(new Order("Move", a, Target.FromCell(squad.World, loc), false));
        }


        protected static CPos RandomBuildingLocation(Squad squad){

            var location = squad.Bot.GetRandomBaseCenter();
            var buildings = squad.World.ActorsHavingTrait<Building>().Where(a => a.Owner == squad.Bot.Player).ToList();

            if (buildings.Count > 0)
                location = buildings.Random(squad.Random).Location;

            return location;
        }

    }
}