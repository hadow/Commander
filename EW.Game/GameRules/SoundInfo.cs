using System;

namespace EW.GameRules
{
    public class SoundInfo
    {

        public SoundInfo(MiniYaml y)
        {
            FieldLoader.Load(this, y);
        }
    }
}