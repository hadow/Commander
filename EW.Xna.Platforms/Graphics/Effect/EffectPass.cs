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

        public EffectAnnotationCollection Annotations { get; private set; }
        public string Name { get; private set; }
        internal EffectPass(Effect effect,string name,
            Shader vertexShader,Shader pixelShader,BlendState blendState,
            DepthStencilState depthStencilState,RasterizerState rasterizerState,EffectAnnotationCollection annotations)
        {
            _effect = effect;
            Name = name;

            _vertexShader = vertexShader;
            _pixelShader = pixelShader;

            _blendState = blendState;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;

            Annotations = annotations;
        }

        internal EffectPass(Effect effect,EffectPass cloneSource)
        {

            _effect = effect;

            Name = cloneSource.Name;
            _blendState = cloneSource._blendState;
            _depthStencilState = cloneSource._depthStencilState;
            _rasterizerState = cloneSource._rasterizerState;
            Annotations = cloneSource.Annotations;
            _vertexShader = cloneSource._vertexShader;
            _pixelShader = cloneSource._pixelShader;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Apply()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="textures"></param>
        /// <param name="samplerStates"></param>
        private void SetShaderSamplers(Shader shader,TextureCollection textures,SamplerStateCollection samplerStates)
        {

        }
    }
}