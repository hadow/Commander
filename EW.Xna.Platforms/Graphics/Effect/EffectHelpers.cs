using System;

namespace EW.Xna.Platforms.Graphics
{
    

    internal enum EffectDirtyFlags
    {
        WorldViewProj = 1,
        World = 2,
        EyePosition = 4,
        MaterialColor = 8,
        Fog = 16,
        FogEnable = 32,
        AlphaTest = 64,
        ShaderIndex = 128,
        All = -1,
    }

    internal static class EffectHelpers
    {

        internal static EffectDirtyFlags SetWorldViewProj(EffectDirtyFlags dirtyFlags,ref Matrix world,
            ref Matrix view,ref Matrix projection,ref Matrix worldView,EffectParameter worldViewProjParam)
        {
            if((dirtyFlags & EffectDirtyFlags.WorldViewProj) != 0)
            {
                Matrix worldViewProj;

                Matrix.Multiply(ref world, ref view, out worldView);

                Matrix.Multiply(ref worldView, ref projection, out worldViewProj);

                worldViewProjParam.SetValue(worldViewProj);
                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }

            return dirtyFlags;
        }

        internal static void SetMaterialColor(bool lightingEnabled,float alpha,ref Vector3 diffuseColor,ref Vector3 emissiveColor,ref Vector3 ambientLightColor,EffectParameter diffuseColorParam,EffectParameter emissiveColorParam)
        {
            if (lightingEnabled)
            {

            }
            else
            {
                Vector4 diffuse = new Vector4();

                diffuse.X = (diffuseColor.X + emissiveColor.X) * alpha;
                diffuse.Y = (diffuseColor.Y + emissiveColor.Y) * alpha;
                diffuse.Z = (diffuseColor.Z + emissiveColor.Z) * alpha;
                diffuse.W = alpha;

                diffuseColorParam.SetValue(diffuse);
            }
        }
    }

}