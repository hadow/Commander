using System;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms.Content;
namespace EW.Xna.Platforms
{
    public interface IResumeManager
    {
        void LoadContent();
        void Draw();
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResumeManager:IResumeManager
    {
        ContentManager content;
        GraphicsDevice device;

        SpriteBatch spriteBatch;

        string resumeTextureName;

        Texture2D resumeTexture;

        float rotation;
        float scale;
        float rotateSpeed;

        public ResumeManager(IServiceProvider services,SpriteBatch spriteBatch,string resumeTextureName,float scale,float rotateSpeed)
        {
            
            this.content = new ContentManager(services, "Content");
            this.device = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            this.spriteBatch = spriteBatch;
            this.resumeTextureName = resumeTextureName;
            this.scale = scale;
            this.rotateSpeed = rotateSpeed;
        }


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

            spriteBatch.Begin();
            spriteBatch.Draw(resumeTexture, new Vector2(sw / 2, sh / 2), null, Color.Red, rotation, new Vector2(tw / 2, th / 2), scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }
    }
}