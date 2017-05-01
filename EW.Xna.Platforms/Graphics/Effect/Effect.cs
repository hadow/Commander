using System;
using System.IO;

namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class Effect:GraphicsResource
    {
        struct MGFXHeader
        {
            public static readonly int MGFXSignature = (BitConverter.IsLittleEndian)? 0x5846474D : 0x4D474658;

            public const int MGFXVersion = 8;

            public int Signature;
            public int Version;
            public int Profile;
            public int EffectKey;
            public int HeaderSize;
        }


        public EffectParameterCollection Parameters { get; private set; }

        public EffectTechniqueCollection Techniques { get; private set; }

        public EffectTechnique CurrentTechnique { get; private set; }
        private Shader[] _shaders;

        internal ConstantBuffer[] ConstantBuffers { get; private set; }
        private readonly bool _isClone;

        public Effect(GraphicsDevice graphicsDevice,byte[] effectCode) : this(graphicsDevice, effectCode, 0, effectCode.Length) { }

        internal Effect(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentException("graphicsDevice");

            this.GraphicsDevice = graphicsDevice;
        }


        public Effect(GraphicsDevice graphicsDevice,byte[] effectCode,int index,int count) : this(graphicsDevice)
        {
            MGFXHeader header = ReadHeader(effectCode, index);
            var effectKey = header.EffectKey;
            var headerSize = header.HeaderSize;

            Effect cloneSource;
            if(!graphicsDevice.EffectCache.TryGetValue(effectKey,out cloneSource))
            {
                using (var stream = new MemoryStream(effectCode, index + headerSize, count - headerSize, false))
                    using(var reader = new BinaryReader(stream))
                {
                    cloneSource = new Effect(graphicsDevice);
                    cloneSource.ReadEffect(reader);

                    graphicsDevice.EffectCache.Add(effectKey, cloneSource);
                }
                    
            }

            _isClone = true;
            Clone(cloneSource);
        }

        private void Clone(Effect cloneSource)
        {
            Parameters = cloneSource.Parameters.Clone();
            Techniques = cloneSource.Techniques.Clone(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effectCode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private MGFXHeader ReadHeader(byte[] effectCode,int index)
        {
            MGFXHeader header;
            header.Signature = BitConverter.ToInt32(effectCode, index);
            index += 4;
            header.Version = (int)effectCode[index++];
            header.Profile = (int)effectCode[index++];
            header.EffectKey = BitConverter.ToInt32(effectCode, index);
            index += 4;
            header.HeaderSize = index;
            
            return header;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void ReadEffect(BinaryReader reader)
        {
            //Constant Buffer
            var buffers = (int)reader.ReadByte();
            ConstantBuffers = new ConstantBuffer[buffers];

            for(var c = 0; c < buffers; c++)
            {
                var name = reader.ReadString();
                var sizeInBytes = (int)reader.ReadInt16();

                var parameters = new int[reader.ReadByte()];
                var offsets = new int[parameters.Length];

                for(var i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = (int)reader.ReadByte();
                    offsets[i] = (int)reader.ReadUInt16();
                }

                var buffer = new ConstantBuffer(GraphicsDevice,
                    sizeInBytes, parameters, offsets, name);

                ConstantBuffers[c] = buffer;
            }

            //shader objects
            var shaders = (int)reader.ReadByte();
            _shaders = new Shader[shaders];

            for(var s = 0; s < shaders; s++)
            {
                _shaders[s] = new Shader(GraphicsDevice, reader);
            }

            //read in the parameter
            Parameters = ReadParameters(reader);

            //read the techniques
            var techniqueCount = (int)reader.ReadByte();
            var techniques = new EffectTechnique[techniqueCount];
            for(var t = 0; t < techniqueCount; t++)
            {
                var name = reader.ReadString();

                var annotations = ReadAnnotation(reader);

                var passes = ReadPasses(reader, this, _shaders);

                techniques[t] = new EffectTechnique(this, name, passes, annotations);
            }
            Techniques = new EffectTechniqueCollection(techniques);
            //CurrentTechnique = Techniques[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static EffectAnnotationCollection ReadAnnotation(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            if (count == 0)
                return EffectAnnotationCollection.Empty;

            var annotations = new EffectAnnotation[count];

            return new EffectAnnotationCollection(annotations);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static EffectParameterCollection ReadParameters(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            if (count == 0)
                return EffectParameterCollection.Empty;

            var parameters = new EffectParameter[count];

            for(var i = 0; i < count; i++)
            {
                var class_ = (EffectParameterClass)reader.ReadByte();
                var type = (EffectParameterType)reader.ReadByte();
                var name = reader.ReadString();
                var semantic = reader.ReadString();
                var annotations = ReadAnnotation(reader);

                var rowCount = (int)reader.ReadByte();
                var columnCount = (int)reader.ReadByte();

                var elements = ReadParameters(reader);
                var structMembers = ReadParameters(reader);

                object data = null;
                if(elements.Count == 0 && structMembers.Count == 0)
                {
                    switch (type)
                    {
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
                        case EffectParameterType.Single:
                            {
                                var buffer = new float[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.ReadSingle();
                                data = buffer;

                            }
                            break;
                        case EffectParameterType.String:
                            throw new NotSupportedException();
                        default:
                            break;
                        
                    }
                }
                parameters[i] = new EffectParameter(class_, type, name, rowCount, columnCount, semantic, annotations, elements, structMembers, data);
            }

            return new EffectParameterCollection(parameters);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="effect"></param>
        /// <param name="shaders"></param>
        /// <returns></returns>
        private static EffectPassCollection ReadPasses(BinaryReader reader,Effect effect,Shader[] shaders)
        {
            var count = (int)reader.ReadByte();
            var passes = new EffectPass[count];

            for(var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var annotations = ReadAnnotation(reader);

                //Get the vertex shader
                Shader vertexShader = null;
                var shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    vertexShader = shaders[shaderIndex];

                //Get the pixel shader
                Shader pixelShader = null;
                shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    pixelShader = shaders[shaderIndex];

                BlendState blend = null;
                DepthStencilState depth = null;
                RasterizerState raster = null;
                if (reader.ReadBoolean())
                {
                    blend = new BlendState
                    {

                    };
                }
                if (reader.ReadBoolean())
                {
                    depth = new DepthStencilState
                    {

                    };
                }

                if (reader.ReadBoolean())
                {
                    raster = new RasterizerState
                    {

                    };
                }

                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, raster,annotations);
                    

            }

            return new EffectPassCollection(passes);
        }

        protected internal override void GraphicsDeviceResetting()
        {
            for (var i = 0; i < ConstantBuffers.Length; i++)
                ConstantBuffers[i].Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!_isClone)
                    {
                        if(_shaders != null)
                        {
                            foreach (var shader in _shaders)
                                shader.Dispose();
                        }
                    }

                    if (ConstantBuffers != null)
                    {
                        foreach (var buffer in ConstantBuffers)
                            buffer.Dispose();
                        ConstantBuffers = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}