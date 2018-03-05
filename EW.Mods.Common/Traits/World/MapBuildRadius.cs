using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MapBuildRadiusInfo : ITraitInfo,ILobbyOptions
    {

        [Translate]
        [Desc("Descriptive label for the ally build radius checkbox in the lobby.")]
        public readonly string AllyBuildRadiusCheckboxLabel = "Build off Allies";

        [Translate]
        [Desc("Tooltip description for the ally build radius checkbox in the lobby.")]
        public readonly string AllyBuildRadiusCheckboxDescription = "Allow allies to place structures inside your build area";

        [Desc("Default value of the ally build radius checkbox in the lobby.")]
        public readonly bool AllyBuildRadiusCheckboxEnabled = true;

        [Desc("Prevent the ally build radius state from being changed in the lobby.")]
        public readonly bool AllyBuildRadiusCheckboxLocked = false;

        [Desc("Whether to display the ally build radius checkbox in the lobby.")]
        public readonly bool AllyBuildRadiusCheckboxVisible = true;

        [Desc("Display order for the ally build radius checkbox in the lobby.")]
        public readonly int AllyBuildRadiusCheckboxDisplayOrder = 0;

        [Translate]
        [Desc("Tooltip description for the build radius checkbox in the lobby.")]
        public readonly string BuildRadiusCheckboxLabel = "Limit Build Area";

        [Translate]
        [Desc("Tooltip description for the build radius checkbox in the lobby.")]
        public readonly string BuildRadiusCheckboxDescription = "Limits structure placement to areas around Construction Yards";

        [Desc("Default value of the build radius checkbox in the lobby.")]
        public readonly bool BuildRadiusCheckboxEnabled = true;

        [Desc("Prevent the build radius state from being changed in the lobby.")]
        public readonly bool BuildRadiusCheckboxLocked = false;

        [Desc("Display the build radius checkbox in the lobby.")]
        public readonly bool BuildRadiusCheckboxVisible = true;

        [Desc("Display order for the build radius checkbox in the lobby.")]
        public readonly int BuildRadiusCheckboxDisplayOrder = 0;

        IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
        {
            yield return new LobbyBooleanOption("allybuild", AllyBuildRadiusCheckboxLabel, AllyBuildRadiusCheckboxDescription,
                AllyBuildRadiusCheckboxVisible, AllyBuildRadiusCheckboxDisplayOrder, AllyBuildRadiusCheckboxEnabled, AllyBuildRadiusCheckboxLocked);

            yield return new LobbyBooleanOption("buildradius", BuildRadiusCheckboxLabel, BuildRadiusCheckboxDescription,
                BuildRadiusCheckboxVisible, BuildRadiusCheckboxDisplayOrder, BuildRadiusCheckboxEnabled, BuildRadiusCheckboxLocked);
        }

        public object Create(ActorInitializer init) { return new MapBuildRadius(this); }
    }

    public class MapBuildRadius:INotifyCreated
    {
        readonly MapBuildRadiusInfo info;

        public bool BuildRadiusEnabled { get; private set; }

        public bool AllyBuildRadiusEnabled { get; private set; }

        public MapBuildRadius(MapBuildRadiusInfo info){
            this.info = info;
        }


        void INotifyCreated.Created(Actor self){

            AllyBuildRadiusEnabled = self.World.LobbyInfo.GlobalSettings.OptionOrDefault("allybuild", info.AllyBuildRadiusCheckboxEnabled);

            BuildRadiusEnabled = self.World.LobbyInfo.GlobalSettings
                .OptionOrDefault("buildradius", info.BuildRadiusCheckboxEnabled);
        }
    }
}