using System;
using OpenTK.Graphics.ES20;

namespace EW.Xna.Platforms.Graphics
{
    public sealed partial class SamplerStateCollection
    {

        private void PlatformSetSamplerState(int index)
        {
        }

        private void PlatformClear()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        internal void PlatformSetSamplers(GraphicsDevice device)
        {
            for(var i = 0; i < _actualSamplers.Length; i++)
            {
                var sampler = _actualSamplers[i];
                var texture = device.Textures[i];

                if(sampler != null && texture != null && sampler != texture.glLastSamplerState)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    GraphicsExtensions.CheckGLError();

                    sampler.Activate(device, texture.glTarget, texture.LevelCount > 1);
                    texture.glLastSamplerState = sampler;
                }
            }
        }

    }
}