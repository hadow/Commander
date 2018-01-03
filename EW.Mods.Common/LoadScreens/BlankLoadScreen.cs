using System;
using System.Collections.Generic;
using EW.Support;
using EW.Framework;
namespace EW.Mods.Common.LoadScreens
{
    public class BlankLoadScreen:ILoadScreen
    {
        ModData modData;

        public LaunchArguments Launch;

        public virtual void Init(ModData modData,Dictionary<string,string> info)
        {
            this.modData = modData;
        }

        public virtual void Display()
        {
            if (WarGame.Renderer == null)
                return;

            //Draw a black screen
            WarGame.Renderer.BeginFrame(Int2.Zero, 1f);
            WarGame.Renderer.EndFrame();
        }


        public void StartGame(Arguments args)
        {
            Launch = new LaunchArguments(args);
            if (Launch.Benchmark)
            {

            }

            var connect = Launch.GetConnectAddress();
            if (!string.IsNullOrEmpty(connect))
            {

            }

            //Load a replay directly
            if (!string.IsNullOrEmpty(Launch.Replay))
            {

            }
            WarGame.LoadShellMap();
        }

        public bool RequiredContentIsInstalled()
        {
            return false;
        }


        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}