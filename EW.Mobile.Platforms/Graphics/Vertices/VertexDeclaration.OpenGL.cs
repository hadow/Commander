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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="offset"></param>
        /// <param name="programHash"></param>
        internal void Apply(Shader shader,IntPtr offset,int programHash)
        {
            VertexDeclarationAttributeInfo attrInfo;
            if(!shaderAttributeInfo.TryGetValue(programHash,out attrInfo))
            {
                attrInfo = new VertexDeclarationAttributeInfo(GraphicsDevice.MaxVertexAttributes);

                foreach(var ve in InternalVertexElements)
                {
                    var attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);
                    if (attributeLocation >= 0)
                    {
                        attrInfo.Elements.Add(new VertexDeclarationAttributeInfo.Element {

                            Offset = ve.Offset,
                            AttributeLocation = attributeLocation,
                            NumberOfElements = ve.VertexElementFormat.OpenGLNumberOfElements(),
                            VertexAttribPointerT = ve.VertexElementFormat.OpenGLVertexAttribPointerT(),
                            Normalized = ve.OpenGLVertexAttribNormalized(),


                        });
                        attrInfo.EnabledAttributes[attributeLocation] = true;
                    }
                }

                shaderAttributeInfo.Add(programHash, attrInfo);
            }

            ///
            foreach(var element in attrInfo.Elements)
            {
                GL.VertexAttribPointer(element.AttributeLocation, element.NumberOfElements, element.VertexAttribPointerT, element.Normalized, this.VertexStride, (IntPtr)(offset.ToInt64() + element.Offset));
                GraphicsExtensions.CheckGLError();
            }
            GraphicsDevice.SetVertexAttributeArray(attrInfo.EnabledAttributes);
        }


    }
}