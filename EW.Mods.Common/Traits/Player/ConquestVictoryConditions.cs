using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class ConquestVictoryConditionsInfo : ITraitInfo,Requires<MissionObjectivesInfo>
    {
        public object Create(ActorInitializer init) { return new ConquestVictoryConditions(init.Self,this); }
    }
    public class ConquestVictoryConditions:ITick,INotifyObjectivesUpdated
    {

        readonly ConquestVictoryConditionsInfo info;
        readonly MissionObjectives mo;
        Player[] otherPlayers;
        int objectiveID = -1;

        public ConquestVictoryConditions(Actor self,ConquestVictoryConditionsInfo cvcInfo)
        {
            info = cvcInfo;
            mo = self.Trait<MissionObjectives>();
        }


        void ITick.Tick(Actor self)
        {

        }

        void INotifyObjectivesUpdated.OnPlayerWon(Player winner)
        {

        }

        void INotifyObjectivesUpdated.OnPlayerLost(Player loser)
        {

        }

        void INotifyObjectivesUpdated.OnObjectiveFailed(Player player, int objectiveID)
        {

        }

        void INotifyObjectivesUpdated.OnObjectiveCompleted(Player player, int objectiveID) { }

        void INotifyObjectivesUpdated.OnObjectiveAdded(Player player, int objectiveID) { }

    }
}