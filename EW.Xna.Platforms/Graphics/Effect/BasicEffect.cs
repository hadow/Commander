using System;

namespace EW.Xna.Platforms.Graphics
{
    public class BasicEffect:Effect
    {

        public BasicEffect(GraphicsDevice device) : base(device, EffectResource.BasicEffect.Bytecode)
        {
            CacheEffectParameters(null);


        }


        #region Effect Parameters

        EffectParameter diffuseColorParam;
        EffectParameter textureParam;
        EffectParameter worldParam;
        EffectParameter worldViewProjParam;

        #endregion
        Matrix world = Matrix.Identity;
        Matrix projection = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix worldView;
        Vector3 diffuseColor = Vector3.One;
        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;
        float alpha = 1;
        Vector3 emissiveColor = Vector3.Zero;
        Vector3 ambientLightColor = Vector3.Zero;
        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }

        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        void CacheEffectParameters(BasicEffect cloneSource)
        {
            diffuseColorParam = Parameters["DiffuseColor"];
            worldParam = Parameters["World"];
            worldViewProjParam = Parameters["WorldViewProj"];
        }

        protected internal override void OnApply()
        {
            dirtyFlags = EffectHelpers.SetWorldViewProj(dirtyFlags, ref world, ref view, ref projection, ref worldView, worldViewProjParam);

            if((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(false, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, null);
                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }
        }
    }
}