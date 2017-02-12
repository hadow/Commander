using System;
using System.Collections.Generic;


namespace RA.Mobile.Platforms.Graphics
{

    internal struct VertexAttribute
    {
        public VertexElementUsage usage;
        public int index;
        public string name;
        public int location;
    }

    internal partial class Shader
    {

        public VertexAttribute[] Attributes { get; private set; }

    }
}