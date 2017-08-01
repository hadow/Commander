using System;
using System.Collections.Generic;
namespace EW
{
    public class GameSpeed
    {
        [Translate]
        public readonly string Name = "Default";

        public readonly int Timestep = 40;

        public readonly int OrderLatency = 3;//÷∏¡Ó—”≥Ÿ
    }


    public class GameSpeeds:IGlobalModData
    {
        [FieldLoader.LoadUsing("LoadSpeeds")]
        public readonly Dictionary<string, GameSpeed> Speeds;

        static object LoadSpeeds(MiniYaml y)
        {
            var ret = new Dictionary<string, GameSpeed>();
            foreach(var node in y.Nodes)
            {
                ret.Add(node.Key, FieldLoader.Load<GameSpeed>(node.Value));
                
            }
            return ret;
        }

    }
}