using System;
using System.Collections.Generic;
using EW.Graphics;
using System.Drawing;
namespace EW.Mods.Common.Graphics
{
    public class ModelPreview:IActorPreview
    {
        readonly ModelAnimation[] components;
        readonly float scale;
        readonly float[] lightAmbientColor;
        readonly float[] lightDiffuseColor;
        readonly WRot lightSource;
        readonly WRot camera;
        readonly PaletteReference colorPalette;
        readonly PaletteReference normalsPalette;
        readonly PaletteReference shadowPalette;
        readonly WVec offset;
        readonly int zOffset;

        public ModelPreview(ModelAnimation[] components,WVec offset,int zOffset,float scale,WAngle lightPitch,WAngle lightYaw,
            float[] lightAmbientColor,float[] lightDiffuseColor,WAngle cameraPitch,
            PaletteReference colorPalette,PaletteReference normalsPalette,PaletteReference shadowPalette)
        {
            this.components = components;
            this.scale = scale;
            this.lightAmbientColor = lightAmbientColor;
            this.lightDiffuseColor = lightDiffuseColor;

            lightSource = new WRot(WAngle.Zero, new WAngle(256) - lightPitch, lightYaw);
            camera = new WRot(WAngle.Zero, cameraPitch - new WAngle(256), new WAngle(256));
        }



        public void Tick() { }

        public IEnumerable<IRenderable> Render(WorldRenderer wr,WPos pos)
        {
            yield return new ModelRenderable(components, pos + offset, zOffset, camera, scale, lightSource, lightAmbientColor, lightDiffuseColor, colorPalette, normalsPalette, shadowPalette);
        }

        public IEnumerable<Rectangle> ScreenBounds(WorldRenderer wr,WPos pos)
        {
            foreach (var c in components)
                yield return c.ScreenBounds(pos, wr, scale);
        }


    }
}