using System;
using System.Collections.Generic;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// 
    /// </summary>
    partial class GamePlatform
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static GamePlatform PlatformCreate(Game game)
        {
#if IOS
            return new IOSGamePlatforme(game);
#elif ANDROID
            return new AndroidGamePlatform(game);
#endif
        }

    }
}