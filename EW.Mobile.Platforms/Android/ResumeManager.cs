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

        GraphicsDevice device;

        SpriteBatch spriteBatch;

        string resumeTextureName;

        Texture2D resumeTexture;



    }
}