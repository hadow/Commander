using System;
using EW.Xna.Platforms.Graphics;
namespace EW.Xna.Platforms.Content
{
    internal class EffectReader:ContentTypeReader<Effect>
    {
        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int dataSize = input.ReadInt32();
            byte[] data = input.ContentManager.GetScratchBuffer(dataSize);
            input.Read(data, 0, dataSize);
            var effect = new Effect(input.GraphicsDevice, data, 0, dataSize);
            effect.Name = input.AssetName;
            return effect;

        }


    }
}