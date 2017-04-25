using System;
using System.Linq;
using System.Collections.Generic;

namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class ModContent:IGlobalModData
    {
        public class ModPackage
        {
            public readonly string Title;
            public readonly string[] TestFiles = { };
            public readonly string[] Sources = { };
            public readonly bool Required;
            public readonly string Download;


            public ModPackage(MiniYaml yaml)
            {
                Title = yaml.Value;
                FieldLoader.Load(this, yaml);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ModDownload
        {
            public readonly string Title;
            public readonly string URL;
            public readonly string MirrorList;
            public readonly Dictionary<string, string> Extreact;

            public ModDownload(MiniYaml yaml)
            {
                Title = yaml.Value;
                FieldLoader.Load(this, yaml);
            }
        }

        public readonly string InstallPromptMessage;
        public readonly string QuickDownload;
        public readonly string HeaderMessage;

        [FieldLoader.LoadUsing("LoadPackages")]
        public readonly Dictionary<string, ModPackage> Packages = new Dictionary<string, ModPackage>();

        static object LoadPackages(MiniYaml yaml)
        {
            var packages = new Dictionary<string, ModPackage>();
            var packageNode = yaml.Nodes.FirstOrDefault(n => n.Key == "Packages");
            if(packageNode != null)
            {
                foreach (var node in packageNode.Value.Nodes)
                    packages.Add(node.Key, new ModPackage(node.Value));
            }

            return packages;
        }

        [FieldLoader.LoadUsing("LoadDownloads")]
        public readonly string[] Downloads = { };

        static object LoadDownloads(MiniYaml yaml)
        {
            var downloadNode = yaml.Nodes.FirstOrDefault(n => n.Key == "Downloads");
            return downloadNode != null ? downloadNode.Value.Nodes.Select(n => n.Key).ToArray() : new string[0];
        }

        [FieldLoader.LoadUsing("LoadSources")]
        public readonly string[] Sources = { };

        static object LoadSources(MiniYaml yaml)
        {
            var sourceNode = yaml.Nodes.FirstOrDefault(n => n.Key == "Sources");
            return sourceNode !=null ?sourceNode.Value.Nodes.Select(n=>n.Key).ToArray():new string[0];
        }


    }
}