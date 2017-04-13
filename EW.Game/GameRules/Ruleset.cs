using System;
using System.Collections.Generic;
using EW.FileSystem;
using EW.Primitives;
using EW.GameRules;
using EW.Graphics;
namespace EW
{
    /// <summary>
    /// πÊ‘ÚºØ
    /// </summary>
    public class Ruleset
    {
        
        public readonly EW.Primitives.IReadOnlyDictionary<string, ActorInfo> Actors;
        public readonly EW.Primitives.IReadOnlyDictionary<string, WeaponInfo> Weapons;
        public readonly EW.Primitives.IReadOnlyDictionary<string, SoundInfo> Voices;
        public readonly EW.Primitives.IReadOnlyDictionary<string, SoundInfo> Notifications;
        public readonly EW.Primitives.IReadOnlyDictionary<string, MusicInfo> Music;

        public Ruleset(EW.Primitives.IReadOnlyDictionary<string,ActorInfo> actors,
                        EW.Primitives.IReadOnlyDictionary<string,WeaponInfo> weapons)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modData"></param>
        /// <returns></returns>
        public static Ruleset LoadDefaults(ModData modData)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modData"></param>
        /// <param name="fileSystem"></param>
        /// <param name="tileSet"></param>
        /// <param name="mapRules"></param>
        /// <param name="mapWeapons"></param>
        /// <param name="mapVoices"></param>
        /// <param name="mapNotifications"></param>
        /// <param name="mapMusic"></param>
        /// <param name="mapSequences"></param>
        /// <returns></returns>
        public static Ruleset Load(ModData modData,IReadOnlyFileSystem fileSystem,string tileSet,
            MiniYaml mapRules,MiniYaml mapWeapons,MiniYaml mapVoices,MiniYaml mapNotifications,MiniYaml mapMusic,MiniYaml mapSequences)
        {
            var m = modData.Manifest;
            var dr = modData.DefaultRules;

            Ruleset ruleset = null;

            Action f = () =>
            {

                var actors = MergeOrDefault("Rules", fileSystem, m.Rules, mapRules, dr.Actors, k => new ActorInfo(modData.ObjectCreator,k.Key.ToLowerInvariant(),k.Value));

                var weapons = MergeOrDefault("Weapons", fileSystem, m.Weapons, mapWeapons, dr.Weapons, k => new WeaponInfo(k.Key.ToLowerInvariant(), k.Value));

                var voices = MergeOrDefault("Voices", fileSystem, m.Voices, mapVoices, dr.Voices, k => new SoundInfo(k.Value));

                var notifications = MergeOrDefault("Notifications", fileSystem, m.Notifications, mapNotifications, dr.Notifications, k => new SoundInfo(k.Value));

                var music = MergeOrDefault("Music", fileSystem, m.Music, mapMusic, dr.Music, k => new MusicInfo(k.Key, k.Value));

                var ts = modData.DefaultTileSets[tileSet];

                var sequences = mapSequences == null ? modData.DefaultSequences[tileSet] : new SequenceProvider();

                ruleset = new Ruleset(actors, weapons);
            };

            if (modData.IsOnMainThread)
            {

            }
            else
            {
                f();
            }

            return ruleset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="fileSystem"></param>
        /// <param name="files"></param>
        /// <param name="additional"></param>
        /// <param name="defaults"></param>
        /// <param name="makeObject"></param>
        /// <returns></returns>
        static EW.Primitives.IReadOnlyDictionary<string,T> MergeOrDefault<T>(string name,IReadOnlyFileSystem fileSystem,IEnumerable<string> files,MiniYaml additional,
            EW.Primitives.IReadOnlyDictionary<string,T> defaults,Func<MiniYamlNode,T> makeObject)
        {
            if (additional == null && defaults != null)
                return defaults;

            var result = MiniYaml.Load(fileSystem, files, additional);
        }
    }
}