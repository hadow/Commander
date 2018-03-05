using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class LobbyPrerequisiteCheckboxInfo:ITraitInfo,ILobbyOptions
    {

        [FieldLoader.Require]
        [Desc("Internal id for this checkbox.")]
        public readonly string ID = null;

        [FieldLoader.Require]
        [Desc("Display name for this checkbox.")]
        public readonly string Label = null;

        [Desc("Description name for this checkbox.")]
        public readonly string Description = null;

        [Desc("Default value of the checkbox in the lobby.")]
        public readonly bool Enabled = false;

        [Desc("Prevent the checkbox from being changed from its default value.")]
        public readonly bool Locked = false;

        [Desc("Display the checkbox in the lobby.")]
        public readonly bool Visible = true;

        [Desc("Display order for the checkbox in the lobby.")]
        public readonly int DisplayOrder = 0;

        [FieldLoader.Require]
        [Desc("Prerequisites to grant when this checkbox is enabled.")]
        public readonly HashSet<string> Prerequisites = new HashSet<string>();


        IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
        {
            yield return new LobbyBooleanOption(ID, Label, Description,
                Visible, DisplayOrder, Enabled, Locked);
        }

        public object Create(ActorInitializer init) { return new LobbyPrerequisiteCheckbox(this); }
    }

    public class LobbyPrerequisiteCheckbox:INotifyCreated,ITechTreePrerequisite
    {


        readonly LobbyPrerequisiteCheckboxInfo info;

        HashSet<string> prerequisites = new HashSet<string>();

        public LobbyPrerequisiteCheckbox(LobbyPrerequisiteCheckboxInfo info){
            this.info = info;

        }

        void INotifyCreated.Created(Actor self){

            var enabled = self.World.LobbyInfo.GlobalSettings.OptionOrDefault(info.ID, info.Enabled);
            if (enabled)
                prerequisites = info.Prerequisites;

        }

        IEnumerable<string> ITechTreePrerequisite.ProvidesPrerequisites { get { return prerequisites; } }


    }
}