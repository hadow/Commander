using System;
using System.Collections.Generic;


namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class InstalledMods:EW.Primitives.IReadOnlyDictionary<string,Manifest>
    {

        readonly Dictionary<string, Manifest> mods;

    }
}