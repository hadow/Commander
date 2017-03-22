using System;

namespace EW.Mobile.Platforms
{

    /// <summary>
    /// ”Œœ∑ ±º‰
    /// </summary>
    public class GameTime
    {
        public TimeSpan TotalGameTime { get; set; }

        public TimeSpan ElapsedGameTime { get; set; }

        public bool IsRunningSlowly { get; set; }

        public GameTime()
        {
            TotalGameTime = TimeSpan.Zero;
            ElapsedGameTime = TimeSpan.Zero;
            IsRunningSlowly = false;
        }


    }
}