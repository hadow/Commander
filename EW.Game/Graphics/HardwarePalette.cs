using System;
using System.Collections.Generic;

namespace EW.Graphics
{
    public sealed class HardwarePalette:IDisposable
    {
        public int Height { get; private set; }
        readonly Dictionary<string, ImmutablePalette> paletes = new Dictionary<string, ImmutablePalette>();
        readonly Dictionary<string, MutablePalette> modifiablePalettes = new Dictionary<string, MutablePalette>();

        public bool Contains(string name)
        {
            return modifiablePalettes.ContainsKey(name) || paletes.ContainsKey(name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="p"></param>
        /// <param name="allowModifiers"></param>
        public void AddPalette(string name,ImmutablePalette p,bool allowModifiers)
        {

        }
        public void Dispose()
        {

        }
    }
}