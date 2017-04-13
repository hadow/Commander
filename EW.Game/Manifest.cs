using System;
using System.Collections.Generic;


namespace EW
{

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

        public readonly string[] Rules, ServerTraits, Sequences,VoxelSequences,Cursors,Chrome,Assemblies,ChromeLayout,Weapons,Voices,Notifications,Music,Translations,TileSets,Missions;


    }
}