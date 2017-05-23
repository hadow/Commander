using System;
using System.Drawing;
using System.Collections.Generic;
using EW.Graphics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW
{
    public sealed class Renderer:IDisposable
    {
        public SpriteRenderer SpriteRenderer;

        public SpriteRenderer WorldSpriteRenderer;

        public SpriteRenderer RgbaSpriteRenderer;

        public SpriteRenderer WorldRgbaSpriteRenderer;

        public RgbaColorRenderer RgbaColorRenderer;

        public RgbaColorRenderer WorldRgbaColorRenderer;

        Size? lastResolution;
        EW.Xna.Platforms.Point? lastScroll;
        float? lastZoom;


        public void BeginFrame(EW.Xna.Platforms.Point scroll,float zoom)
        {
            GraphicsDeviceManager.M.GraphicsDevice.Clear(EW.Xna.Platforms.Color.White);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="zoom"></param>
        public void SetViewportParams(EW.Xna.Platforms.Point scroll,float zoom)
        {

        }
        public void Dispose()
        {

        }
    }
}