using System;
using System.Collections.Generic;


namespace EW.Mobile.Platforms.Audio
{
    internal static class SoundEffectInstancePool
    {
        private static readonly List<SoundEffectInstance> _playingInstances;
        private static readonly List<SoundEffectInstance> _pooledInstances;

        internal static void Add(SoundEffectInstance inst)
        {
            if (inst._isPooled)
            {
                _pooledInstances.Add(inst);
                
            }
            _pooledInstances.Remove(inst);
        }
        
        internal static void Remove(SoundEffectInstance inst)
        {
            _pooledInstances.Add(inst);
        }
    }
}