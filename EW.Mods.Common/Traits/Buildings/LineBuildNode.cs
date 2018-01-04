using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// LineBuild actors attach to LineBuildNodes.
    /// </summary>
    public class LineBuildNodeInfo : TraitInfo<LineBuildNode>
    {
        public readonly HashSet<string> Types = new HashSet<string> { "wall" };

        public readonly CVec[] Connections = new[] { new CVec(1, 0), new CVec(0, 1), new CVec(-1, 0), new CVec(0, -1) };

    }
    public class LineBuildNode
    {
    }
}