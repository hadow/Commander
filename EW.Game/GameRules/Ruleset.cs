using System;
using RA.FileSystem;
using RA.Primitives;
namespace RA
{
    /// <summary>
    /// 
    /// </summary>
    public class Ruleset
    {
        public readonly IReadOnlyDictionary<string, ActorInfo> Actors;
        public readonly IReadOnlyDictionary<string, WeaponInfo> Weapons;


        public Ruleset(IReadOnlyDictionary<string,ActorInfo> actors,IReadOnlyDictionary<string,WeaponInfo> weapons)
        {
            Actors = actors;
            Weapons = weapons;

            foreach(var a in Actors.Values)
            {
                foreach(var t in a.TraitInfos<IRulesetLoaded>())
                {

                }
            }

        }
    }
}