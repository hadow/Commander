using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MapOptionsInfo : ITraitInfo,IRulesetLoaded
    {

        public readonly string GameSpeed = "default";

        public object Create(ActorInitializer init) { return new MapOptions(this); }


        void IRulesetLoaded<ActorInfo>.RulesetLoaded(Ruleset rules, ActorInfo info)
        {
            var gameSpeeds = WarGame.ModData.Manifest.Get<GameSpeeds>().Speeds;
            if (!gameSpeeds.ContainsKey(GameSpeed))
                throw new YamlException("Invalid default game speed '{0}'.".F(GameSpeed));
        }
    }

    public class MapOptions:INotifyCreated
    {
        readonly MapOptionsInfo info;

        public string TechLevel { get; private set; }
        public GameSpeed GameSpeed { get; private set; }
        public MapOptions(MapOptionsInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            var speed = self.World.LobbyInfo.GlobalSettings.OptionOrDefault("gamespeed", info.GameSpeed);

            GameSpeed = WarGame.ModData.Manifest.Get<GameSpeeds>().Speeds[speed];
        }
    }
}