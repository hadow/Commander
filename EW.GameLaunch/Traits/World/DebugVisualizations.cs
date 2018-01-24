using System;

namespace EW.Traits
{
    public class DebugVisualizationsInfo:TraitInfo<DebugVisualizations>{}

    public class DebugVisualizations
    {
        public bool CombatGeometry;
        public bool RenderGeometry;
        public bool ScreenMap;
        public bool DepthBuffer;
        public bool ActorTags;
    }
}