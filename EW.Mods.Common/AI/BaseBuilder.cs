using System;
using System.Linq;
using System.Collections.Generic;
using EW.Mods.Common.Traits;
using EW.NetWork;
using EW.Traits;

namespace EW.Mods.Common.AI
{
    public class BaseBuilder
    {
        readonly string category;
        readonly HackyAI ai;
        readonly World world;
        readonly Player player;
        readonly PowerManager playerPower;
        readonly PlayerResources playerResources;

        int waitTicks;
        Actor[] playerBuildings;
        int failCount;
        int failRetryTicks;
        int checkForBaseTicks;
        int cachedBases;
        int cachedBuildings;

        enum Water
        {
            NotChecked,
            EnoughWater,
            NotEnoughWater
        }
        Water waterState = Water.NotChecked;


        public BaseBuilder(HackyAI ai,string category,Player p,PowerManager pm,PlayerResources pr)
        {
            this.ai = ai;
            world = p.World;
            this.category = category;
            this.player = p;
            this.playerPower = pm;
            this.playerResources = pr;
            this.category = category;
            failRetryTicks = ai.Info.StructureProductionResumeDelay;

        }

        public void Tick()
        {

            // If failed to place something N consecutive times, wait M ticks until resuming building production
            if(failCount >= ai.Info.MaximumFailedPlacementAttempts && --failRetryTicks<=0){

                var currentBuildings = world.ActorsHavingTrait<Building>().Count(a => a.Owner == player);
                var baseProviders = world.ActorsHavingTrait<BaseProvider>().Count(a => a.Owner == player);

                // Only bother resetting failCount if either a) the number of buildings has decreased since last failure M ticks ago,
                // or b) number of BaseProviders (construction yard or similar) has increased since then.
                // Otherwise reset failRetryTicks instead to wait again.
                if (currentBuildings < cachedBuildings || baseProviders > cachedBases)
                    failCount = 0;
                else
                    failRetryTicks = ai.Info.StructureProductionResumeDelay;
            }


            if(waterState == Water.NotChecked){

                if (ai.EnoughWaterToBuildNaval())
                    waterState = Water.EnoughWater;
                else
                {
                    waterState = Water.NotEnoughWater;
                    checkForBaseTicks = ai.Info.CheckForNewBasesDelay;
                }
            }

            if(waterState == Water.NotEnoughWater && --checkForBaseTicks<=0){

                var currentBases = world.ActorsHavingTrait<BaseProvider>().Count(a => a.Owner == player);

                if(currentBases> cachedBases){
                    cachedBases = currentBases;
                    waterState = Water.NotChecked;
                }
            }


            // Only update once per second or so
            if (--waitTicks > 0)
                return;


            playerBuildings = world.ActorsHavingTrait<Building>().Where(a => a.Owner == player).ToArray();

            var active = false;

            foreach(var queue in ai.FindQueues(category)){
                if (TickQueue(queue))
                    active = true;
                
            }

            // Add a random factor so not every AI produces at the same tick early in the game.
            // Minimum should not be negative as delays in HackyAI could be zero.
            var randomFactor = ai.Random.Next(0, ai.Info.StructureProductionRandomBonusDelay);

            // Needs to be at least 4 * OrderLatency because otherwise the AI frequently duplicates build orders (i.e. makes the same build decision twice)
            waitTicks = active ? 4 * world.LobbyInfo.GlobalSettings.OrderLatency + ai.Info.StructureProductionActiveDelay + randomFactor
                : ai.Info.StructureProductionInactiveDelay + randomFactor;

        }


        bool TickQueue(ProductionQueue queue){


            var currentBuilding = queue.CurrentItem();
            // Waiting to build something

            if(currentBuilding == null && failCount<ai.Info.MaximumFailedPlacementAttempts){

                var item = ChooseBuildingToBuild(queue);
                if (item == null)
                    return false;

                ai.QueueOrder(Order.StartProduction(queue.Actor,item.Name,1));


            }
            else if(currentBuilding != null && currentBuilding.Done){

                var type = BuildingType.Building;
                if (world.Map.Rules.Actors[currentBuilding.Item].HasTraitInfo<AttackBaseInfo>())
                    type = BuildingType.Defense;
                else if (world.Map.Rules.Actors[currentBuilding.Item].HasTraitInfo<RefineryInfo>())
                    type = BuildingType.Refinery;

                var location = ai.ChooseBuildLocation(currentBuilding.Item, true, type);

                if(location == null)
                {

                    ai.QueueOrder(Order.CancelProduction(queue.Actor,currentBuilding.Item,1));
                    failCount += failCount;

                    // If we just reached the maximum fail count, cache the number of current structures
                    if(failCount == ai.Info.MaximumFailedPlacementAttempts){

                        cachedBuildings = world.ActorsHavingTrait<Building>().Count(a => a.Owner == player);
                        cachedBases = world.ActorsHavingTrait<BaseProvider>().Count(a => a.Owner == player);

                    }
                }
                else{

                    failCount = 0;
                    ai.QueueOrder(new Order("PlaceBuilding",player.PlayerActor,Target.FromCell(world,location.Value),false){

                        //Building to place
                        TargetString = currentBuilding.Item,

                        // Actor ID to associate the placement with
                        ExtraData = queue.Actor.ActorID,
                        SuppressVisualFeedback = true
                    });

                    return true;
                }
            }

            return true;

        }


        ActorInfo ChooseBuildingToBuild(ProductionQueue queue){

            var buildableThings = queue.BuildableItems();

            // This gets used quite a bit, so let's cache it here
            var power = GetProducibleBuilding(ai.Info.BuildingCommonNames.Power, buildableThings, a => a.TraitInfos<PowerInfo>().Where(i => i.EnabledByDefault).Sum(p => p.Amount));


            // First priority is to get out of a low power situation
            if(playerPower.ExcessPower < ai.Info.MinimumExcessPower)
            {

                if (power != null && power.TraitInfos<PowerInfo>().Where(i => i.EnabledByDefault).Sum(p => p.Amount) > 0)
                    return power;
            }

            // Next is to build up a strong economy
            if(!ai.HasAdequateProc() ||!ai.HasMinimumProc())
            {

                var refinery = GetProducibleBuilding(ai.Info.BuildingCommonNames.Refinery, buildableThings);
                if(refinery != null  && HasSufficientPowerForActor(refinery))
                {
                    return refinery;
                }

                if (power != null && refinery != null && !HasSufficientPowerForActor(refinery))
                    return power;
            }


            // Make sure that we can spend as fast as we are earning
            if(ai.Info.NewProductionCashThreshold > 0 && playerResources.Resources > ai.Info.NewProductionCashThreshold)
            {
                var production = GetProducibleBuilding(ai.Info.BuildingCommonNames.Production, buildableThings);
                if (production != null && HasSufficientPowerForActor(production))
                    return production;

                if (power != null && production != null && !HasSufficientPowerForActor(production))
                    return power;

            }

            // Only consider building this if there is enough water inside the base perimeter and there are close enough adjacent buildings
            if(waterState == Water.EnoughWater && ai.Info.NewProductionCashThreshold> 0 
               && playerResources.Resources > ai.Info.NewProductionCashThreshold && ai.CloseEnoughToWater()){

                var navalproduction = GetProducibleBuilding(ai.Info.BuildingCommonNames.NavalProduction, buildableThings);
                if(navalproduction != null && HasSufficientPowerForActor(navalproduction)){

                    return navalproduction;
                }

                if (power != null && navalproduction != null && !HasSufficientPowerForActor(navalproduction))
                    return power;
            }

            // Create some head room for resource storage if we really need it
            if(playerResources.Resources > 0.8 * playerResources.ResourceCapacity){


                var silo = GetProducibleBuilding(ai.Info.BuildingCommonNames.Silo, buildableThings);
                if (silo != null && HasSufficientPowerForActor(silo))
                    return silo;

                if (power != null && silo != null && !HasSufficientPowerForActor(silo))
                    return power;
                
            }

            // Build everything else
            foreach(var frac in ai.Info.BuildingFractions.Shuffle(ai.Random))
            {
                var name = frac.Key;
                // Can we build this structure?
                if (!buildableThings.Any(b => b.Name == name))
                    continue;

                // Do we want to build this structure?
                var count = playerBuildings.Count(a => a.Info.Name == name);
                if (count > frac.Value * playerBuildings.Length)
                    continue;

                if(ai.Info.BuildingLimits.ContainsKey(name) && ai.Info.BuildingLimits[name] <= count){
                    continue;
                }

                // If we're considering to build a naval structure, check whether there is enough water inside the base perimeter
                // and any structure providing buildable area close enough to that water.
                // TODO: Extend this check to cover any naval structure, not just production.
                if (ai.Info.BuildingCommonNames.NavalProduction.Contains(name)
                    && (waterState == Water.NotEnoughWater || !ai.CloseEnoughToWater()))
                    continue;

                // Will this put us into low power?
                var actor = world.Map.Rules.Actors[name];
                if(playerPower.ExcessPower < ai.Info.MinimumExcessPower || !HasSufficientPowerForActor(actor)){

                    // Try building a power plant instead
                    if(power!= null && power.TraitInfos<PowerInfo>().Where(i=>i.EnabledByDefault).Sum(pi=>pi.Amount)>0){
                        return power;
                    }
                }

                return actor;
            }

            return null;


        }

        bool HasSufficientPowerForActor(ActorInfo actorInfo){

            return (actorInfo.TraitInfos<PowerInfo>().Where(i => i.EnabledByDefault).Sum(p => p.Amount) + playerPower.ExcessPower) >= ai.Info.MinimumExcessPower;
        }


        ActorInfo GetProducibleBuilding(HashSet<string> actors,IEnumerable<ActorInfo> buildables,Func<ActorInfo,int> orderBy = null){

            var available = buildables.Where(actor =>
            {

                if (!actors.Contains(actor.Name))
                    return false;

                if (!ai.Info.BuildingLimits.ContainsKey(actor.Name))
                    return true;

                return playerBuildings.Count(a => a.Info.Name == actor.Name) < ai.Info.BuildingLimits[actor.Name];
            });

            if (orderBy != null)
                return available.MaxByOrDefault(orderBy);

            return available.RandomOrDefault(ai.Random);

        }
    }
}
