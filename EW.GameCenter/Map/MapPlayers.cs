using System;
using System.Linq;
using System.Collections.Generic;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class MapPlayers
    {

        public readonly Dictionary<string, PlayerReference> Players;

        public MapPlayers(IEnumerable<MiniYamlNode> playerDefinitions)
        {
            Players = playerDefinitions.Select(pr => new PlayerReference(new MiniYaml(pr.Key, pr.Value.Nodes))).ToDictionary(player => player.Name);
        }

    }
}