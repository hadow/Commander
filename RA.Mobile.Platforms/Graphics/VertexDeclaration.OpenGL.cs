using System;
using System.Collections;
using System.Collections.Generic;
#if GLES
using OpenTK.Graphics.ES20;
#endif

namespace RA.Mobile.Platforms.Graphics
{
    public partial class VertexDeclaration
    {
        class VertexDeclarationAttributeInfo
        {
            internal class Element
            {
                public int Offset;
                public int AttributeLocation;
                public int NumberOfElements;
                public VertexAttribPointerType VertexAttribPointerT;
                public bool Normalized;
            }
            internal bool[] EnabledAttributes;
            internal List<Element> Elements;

            internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
            {
                EnabledAttributes = new bool[maxVertexAttributes];
                Elements = new List<Element>();
            }

        }

        Dictionary<int, VertexDeclarationAttributeInfo> shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        internal void Apply()
        {

        }


    }
}