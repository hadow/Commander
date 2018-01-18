using System;
namespace EW.Framework.Mobile
{
    /// <summary>
    /// Interface for a class that handles resuming after a device lost event.
    /// In particular, this allows the game to draw something to the screen whilst
    /// graphics content is reloaded - a potentially lengthy operation.
    /// </summary>
    public interface IResumeManager
    {
        /// <summary>
        /// Called at the start of the resume process. Textures should always be reloaded here.
        /// If using a ContentManager, it should be disposed and recreated.
        /// </summary>
        void LoadContent();

        /// <summary>
        /// Called whilst the game is resuming. Draw something to the screen here.
        /// </summary>
        void Draw();
    }
}
