using System;


namespace EW.Xna.Platforms.Graphics
{
    public class EffectAnnotation
    {

        public EffectParameterClass ParameterClass { get; private set; }
        public EffectParameterType ParameterType { get; private set; }

        public string Name { get; private set; }

        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        public string Semantic { get; private set; }


    }
}