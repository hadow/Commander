using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using EW.FileSystem;
using EW.Primitives;
namespace EW
{
    public interface IGlobalModData { }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SpriteSequenceFormat : IGlobalModData
    {
        public readonly string Type;

        public readonly IReadOnlyDictionary<string, MiniYaml> Metadata;

        public SpriteSequenceFormat(MiniYaml yaml)
        {
            Type = yaml.Value;
            Metadata = new ReadOnlyDictionary<string, MiniYaml>(yaml.ToDictionary());
        }
    }

    public sealed class ModelSequenceFormat : IGlobalModData
    {
        public readonly string Type;

        public readonly IReadOnlyDictionary<string, MiniYaml> Metadata;

        public ModelSequenceFormat(MiniYaml yaml)
        {
            Type = yaml.Value;
            Metadata = new ReadOnlyDictionary<string, MiniYaml>(yaml.ToDictionary());
        }
    }
    public class ModMetadata
    {
        public string Title;
        public string Description;
        public string Version;
        public string Author;
        public bool Hidden;

    }
    /// <summary>
    /// 运行一个Mode需要加载的内容清单
    /// </summary>
    public class Manifest
    {
        public readonly string Id;
        public readonly IReadOnlyPackage Package;
        public readonly ModMetadata Metadata;

        public readonly IReadOnlyDictionary<string, string> Packages;
        public readonly IReadOnlyDictionary<string, string> MapFolders;

        public readonly MiniYaml LoadScreen;
        public readonly Dictionary<string, string> RequiresMods;

        /// <summary>
        /// 
        /// </summary>
        public readonly string[] SpriteFormats = { };

        public readonly string[] PackageFormats = { };

        public readonly string[] SoundFormats = { };

        public readonly Dictionary<string, Pair<string, int>> Fonts;

        readonly Dictionary<string, MiniYaml> yaml;

        public readonly string[] Rules, 
                                ServerTraits,
                                Sequences,
                                ModelSequences,
                                Cursors,
                                Chrome,
                                Assemblies,
                                ChromeLayout,
                                ChromeMetrics,
                                Weapons,
                                Voices,
                                Notifications,
                                Music,
                                Translations,
                                TileSets,
                                Missions,
                                MapCompatibility;

        readonly string[] reservedModuleNames = { "Metadata", "Folders", "MapFolders","Cursors",
                                                "Packages", "Rules", "Sequences","ModelSequences",
                                                "Assemblies", "Weapons","RequiresMods","Fonts",
                                                "SoundFormats","SpriteFormats","Missions","Music","Videos","Notifications",
                                                "TileSets","Chrome","ChromeLayout","Voices","Translations","ServerTraits",
                                                "LoadScreen","ChromeMetrics","SupportsMapsFrom","PackageFormats"};

        /// <summary>
        /// 
        /// </summary>
        readonly TypeDictionary modules = new TypeDictionary();

        bool customDataLoaded;

        public Manifest(string modId,IReadOnlyPackage package)
        {
            Id = modId;
            Package = package;

            yaml = new MiniYaml(null, MiniYaml.FromStream(Package.GetStream("mod.yaml"), "mod.yaml")).ToDictionary();

            Metadata = FieldLoader.Load<ModMetadata>(yaml["Metadata"]);

            MapFolders = YamlDictionary(yaml, "MapFolders");

            MiniYaml packages;
            if (yaml.TryGetValue("Packages", out packages))
                Packages = packages.ToDictionary(x => x.Value).AsReadOnly();
            Rules = YamlList(yaml, "Rules");
            Sequences = YamlList(yaml, "Sequences");
            ModelSequences = YamlList(yaml, "ModelSequences");
            Assemblies = YamlList(yaml, "Assemblies");
            Weapons = YamlList(yaml, "Weapons");
            Voices = YamlList(yaml, "Voices");
            Notifications = YamlList(yaml, "Notifications");
            Music = YamlList(yaml, "Music");
            TileSets = YamlList(yaml, "TileSets");
            Missions = YamlList(yaml, "Missions");
            ServerTraits = YamlList(yaml, "ServerTraits");
            ChromeLayout = YamlList(yaml, "ChromeLayout");
            ChromeMetrics = YamlList(yaml, "ChromeMetrics");
            Fonts = yaml["Fonts"].ToDictionary(my =>
            {
                var nd = my.ToDictionary();
                return Pair.New(nd["Font"].Value, Exts.ParseIntegerInvariant(nd["Size"].Value));
            });
            if (!yaml.TryGetValue("LoadScreen", out LoadScreen))
                throw new InvalidDataException("'LoadScreen' section is not defined.");

            var compat = new List<string> { Id };

            if (yaml.ContainsKey("SupportsMapsFrom"))
                compat.AddRange(yaml["SupportsMapsFrom"].Value.Split(',').Select(c => c.Trim()));

            MapCompatibility = compat.ToArray();

            if (yaml.ContainsKey("SpriteFormats"))
            {
                SpriteFormats = FieldLoader.GetValue<string[]>("SpriteFormats", yaml["SpriteFormats"].Value);
            }

            if (yaml.ContainsKey("PackageFormats"))
            {
                PackageFormats = FieldLoader.GetValue<string[]>("PackageFormats", yaml["PackageFormats"].Value);
            }

            if (yaml.ContainsKey("SoundFormats"))
            {
                SoundFormats = FieldLoader.GetValue<string[]>("SoundFormats", yaml["SoundFormats"].Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yaml"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static string[] YamlList(Dictionary<string,MiniYaml> yaml,string key)
        {
            if (!yaml.ContainsKey(key))
                return new string[] { };
            return yaml[key].ToDictionary().Keys.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yaml"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static IReadOnlyDictionary<string,string> YamlDictionary(Dictionary<string,MiniYaml> yaml,string key)
        {
            if (!yaml.ContainsKey(key))
                return new ReadOnlyDictionary<string, string>();

            var inner = yaml[key].ToDictionary(my => my.Value);
            return new ReadOnlyDictionary<string, string>(inner);

        }

        /// <summary>
        /// 加载自定义YAML Data
        /// </summary>
        /// <param name="oc"></param>
        public void LoadCustomData(ObjectCreator oc)
        {
            foreach(var kv in yaml)
            {
                if (reservedModuleNames.Contains(kv.Key))
                    continue;

                var t = oc.FindType(kv.Key);

                if (t == null || !typeof(IGlobalModData).IsAssignableFrom(t))
                    throw new InvalidOperationException("'{0}' is not a valid mod manifest entry.".F(kv.Key));

                IGlobalModData module;
                var ctor = t.GetConstructor(new[] { typeof(MiniYaml) });

                if(ctor != null)
                {
                    module = (IGlobalModData)ctor.Invoke(new object[] { kv.Value });
                }
                else
                {
                    module = oc.CreateObject<IGlobalModData>(kv.Key);
                    FieldLoader.Load(module, kv.Value);
                }
                modules.Add(module);
            }

            customDataLoaded = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : IGlobalModData
        {
            if (!customDataLoaded)
                throw new InvalidOperationException("Attempted to call Manifest.Get() before loading custom data");

            var module = modules.GetOrDefault<T>();

            if (module == null)
            {
                module = (T)WarGame.ModData.ObjectCreator.CreateBasic(typeof(T));
                modules.Add(module);
            }
            return module;
        }


        /// <summary>
        /// Load an uncached IGlobalModData instance directly from the manifest yaml.
        /// This should only be used by external mods that want to query data from this mod.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="oc">Oc.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Get<T>(ObjectCreator oc) where T:IGlobalModData{


            MiniYaml data;
            var t = typeof(T);
            if(!yaml.TryGetValue(t.Name,out data)){
                return (T)oc.CreateBasic(t);
            }

            IGlobalModData module;
            var ctor = t.GetConstructor(new[] { typeof(MiniYaml) });
            if(ctor !=null){
                module = (IGlobalModData)ctor.Invoke(new object[] { data.Value });
            }
            else{

                module = oc.CreateObject<IGlobalModData>(t.Name);
                FieldLoader.Load(module,data);
            }

            return (T)module;
        }

        public bool Contains<T>() where T : IGlobalModData
        {
            return modules.Contains<T>();
        }



        
    }
}