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


        private Shader[] _shaders;

        internal ConstantBuffer[] ConstantBuffers { get; private set; }
        private readonly bool _isClone;
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
            return header;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void ReadEffect(BinaryReader reader)
        {
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

                var buffer = new ConstantBuffer(GraphicsDevice, sizeInBytes, parameters, offsets, name);

                ConstantBuffers[c] = buffer;
            }

            //shader objects
            var shaders = (int)reader.ReadByte();
            _shaders = new Shader[shaders];

            for(var s = 0; s < shaders; s++)
            {
                _shaders[s] = new Shader(GraphicsDevice, reader);
            }
        }
    }
}