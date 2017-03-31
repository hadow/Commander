using System;
using EW.Mobile.Platforms.Graphics;
using EW.Mobile.Platforms.Content;
namespace EW.Mobile.Platforms
{
    public interface IResumeManager
    {
        void LoadContent();
        void Draw();
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResumeManager
    {
        ContentManager content;
        GraphicsDevice device;

        SpriteBatch spriteBatch;

        string resumeTextureName;

        Texture2D resumeTexture;


        /// <summary>
        /// 
        /// </summary>
        public virtual void LoadContent()
        {
            content.Unload();
            resumeTexture = content.Load<Texture2D>(resumeTextureName);
        }

        /// <summary>
        /// »æÖÆ
        /// </summary>
        public virtual void Draw()
        {
            int sw = device.PresentationParameters.BackBufferWidth;
            int sh = device.PresentationParameters.BackBufferHeight;

            int tw = resumeTexture.Width;
            int th = resumeTexture.Height;

            spriteBatch
        }
    }
}