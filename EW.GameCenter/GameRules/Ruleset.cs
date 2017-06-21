using System;
using System.Collections.Generic;
using EW.FileSystem;
using EW.Primitives;
using EW.GameRules;
using EW.Graphics;
using System.Threading.Tasks;
namespace EW
{
    /// <summary>
    /// 规则集
    /// </summary>
    public class Ruleset
    {
        
        public readonly TileSet TileSet;

        public readonly SequenceProvider Sequences;
        
        public readonly IReadOnlyDictionary<string, ActorInfo> Actors;
        public readonly IReadOnlyDictionary<string, WeaponInfo> Weapons;
        public readonly IReadOnlyDictionary<string, SoundInfo> Voices;
        public readonly IReadOnlyDictionary<string, SoundInfo> Notifications;
        public readonly IReadOnlyDictionary<string, MusicInfo> Music;

        public Ruleset(IReadOnlyDictionary<string,ActorInfo> actors,
                        IReadOnlyDictionary<string,WeaponInfo> weapons,
                        IReadOnlyDictionary<string,SoundInfo> voices,
                        IReadOnlyDictionary<string,SoundInfo> notifications,
                        IReadOnlyDictionary<string,MusicInfo> music,
                        TileSet tileSet,
                        SequenceProvider sequence)
        {
            Actors = actors;
            Weapons = weapons;
            Voices = voices;
            Notifications = notifications;
            TileSet = tileSet;
            Sequences = sequence;
            this.Music = music;
            foreach(var a in Actors.Values)
            {
                foreach(var t in a.TraitInfos<IRulesetLoaded>())
                {

                }
            }

        }

        /// <summary>
        ///  加载默认规则集
        /// </summary>
        /// <param name="modData"></param>
        /// <returns></returns>
        public static Ruleset LoadDefaults(ModData modData)
        {
            var m = modData.Manifest;
            var fs = modData.DefaultFileSystem;

            Ruleset ruleset = null;
            Action f = () =>
            {
                var actors = MergeOrDefault("Manifest,Rules", fs, m.Rules, null, null, k => new ActorInfo(modData.ObjectCreator, k.Key.ToLowerInvariant(), k.Value));

                var weapons = MergeOrDefault("Manifest,Weapons", fs, m.Weapons, null, null, k => new WeaponInfo(k.Key.ToLowerInvariant(), k.Value));

                var voices = MergeOrDefault("Manifest,Voices", fs, m.Voices, null, null, k => new SoundInfo(k.Value));

                var notifications = MergeOrDefault("Manifest,Notifications", fs, m.Notifications, null, null, k => new SoundInfo(k.Value));

                var music = MergeOrDefault("Manifest,Music", fs, m.Music, null, null, k => new MusicInfo(k.Key, k.Value));

                
                ruleset = new Ruleset(actors, weapons, voices, notifications, music, null, null);
            };

            if (modData.IsOnMainThread)
            {
                modData.HandleLoadingProgress();

                var loader = new Task(f);
                loader.Start();

                //Animate the loadscreen while we wait
                while (!loader.Wait(40))
                    modData.HandleLoadingProgress();
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

                var sequences = mapSequences == null ? modData.DefaultSequences[tileSet] : new SequenceProvider(modData.Game,fileSystem,modData,ts,mapSequences);

                //TODO:Add support for custom voxel sequences
                ruleset = new Ruleset(actors, weapons,voices,notifications,music,ts,sequences);
            };

            if (modData.IsOnMainThread)
            {
                modData.HandleLoadingProgress();

                var loader = new Task(f);
                loader.Start();

                //Animate the loadscreen while we wait
                while (!loader.Wait(40))
                    modData.HandleLoadingProgress();
            }
            else
            {
                f();
            }

            return ruleset;
        }


        public static Ruleset LoadDefaultsForTileSet(ModData modData,string tileSet)
        {
            var dr = modData.DefaultRules;
            var ts = modData.DefaultTileSets[tileSet];
            var sequences = modData.DefaultSequences[tileSet];
            return new Ruleset(dr.Actors, dr.Weapons, dr.Voices, dr.Notifications, dr.Music, ts, sequences);
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
        static IReadOnlyDictionary<string,T> MergeOrDefault<T>(string name,IReadOnlyFileSystem fileSystem,IEnumerable<string> files,MiniYaml additional,
            IReadOnlyDictionary<string,T> defaults,Func<MiniYamlNode,T> makeObject)
        {
            if (additional == null && defaults != null)
                return defaults;

            var result = MiniYaml.Load(fileSystem, files, additional).ToDictionaryWithConflictLog(k => k.Key.ToLowerInvariant(), makeObject, "LoadFromManifest<" + name + ">");

            return new ReadOnlyDictionary<string, T>(result);
        }
    }
}