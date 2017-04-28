using System;
using System.IO;

namespace EW.Xna.Platforms.Graphics
{

    /// <summary>
    /// 
    /// </summary>
    internal class EffectResource
    {
        const string SpriteEffectName = "EW.Xna.Platforms.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo";

        public static readonly EffectResource SpriteEffect = new EffectResource(SpriteEffectName);

        private readonly object _locker = new object();
        private readonly string _name;
        private volatile byte[] _bytecode;

        private EffectResource(string name)
        {
            _name = name;
        }

        public byte[] Bytecode
        {
            get
            {
                if(_bytecode == null)
                {
                    lock (_locker)
                    {
                        if (_bytecode != null)
                            return _bytecode;

                        var assembly = typeof(EffectResource).Assembly;

                        var stream = assembly.GetManifestResourceStream(_name);
                        using(var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            _bytecode = ms.ToArray();
                        }
                    }
                }

                return _bytecode;
            }
        }


    }
}