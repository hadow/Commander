using System;
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
            public readonly string[] Sources = { };
            public readonly bool Required;
            public readonly string Download;
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





    }
}