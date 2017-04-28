using System;


namespace EW.Xna.Platforms.Graphics
{
    public class EffectPass
    {
        private readonly Effect _effect;

        private readonly Shader _pixelShader;
        private readonly Shader _vertexShader;

        private readonly BlendState _blendState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;

        public string Name { get; private set; }
        internal EffectPass(Effect effect,string name,
            Shader vertexShader,Shader pixelShader,BlendState blendState,
            DepthStencilState depthStencilState,RasterizerState rasterizerState)
        {

        }
    }
}