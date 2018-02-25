using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public enum ObjectiveType { Primary,Secondary}

    public enum ObjectiveState { Incomplete,Completed,Failed}

    public class MissionObjective
    {
        public readonly ObjectiveType Type;
        public readonly string Description;
        public ObjectiveState State;

        public MissionObjective(ObjectiveType type, string description)
        {
            Type = type;
            Description = description;
            State = ObjectiveState.Incomplete;
        }
    }

    public class MissionObjectivesInfo : ITraitInfo
    {
        
        public object Create(ActorInitializer init) { return new MissionObjectives(init.World,this); }
    }
    public class MissionObjectives:INotifyObjectivesUpdated,ISync,IResolveOrder
    {
        public readonly MissionObjectivesInfo Info;

        readonly List<MissionObjective> objectives = new List<MissionObjective>();

        public ReadOnlyList<MissionObjective> Objectives;

        public MissionObjectives(World world,MissionObjectivesInfo info)
        {
            Info = info;
            Objectives = new ReadOnlyList<MissionObjective>(objectives);
        }
        

        public void MarkCompleted(Player player,int objectiveID)
        {

        }

        public void MarkFailed(Player player,int objectiveID)
        {

        }

        public void OnPlayerWon(Player player)
        {

        }


        public void OnPlayerLost(Player player)
        {

        }
        public void OnObjectiveAdded(Player player, int id) { }
        public void OnObjectiveCompleted(Player player, int id) { }
        public void OnObjectiveFailed(Player player, int id) { }

        void IResolveOrder.ResolveOrder(Actor self, NetWork.Order order)
        {

        }
    }



    public class ObjectivesPanelInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ObjectivesPanel(); }
    }

    public class ObjectivesPanel
    {

    }

}