using System;
using System.Collections.Generic;


namespace EW.Mods.Common.LoadScreens
{
    public class BlankLoadScreen:ILoadScreen
    {
        public virtual void Init(ModData modData,Dictionary<string,string> info)
        {

        }

        public virtual void Display()
        {

        }


        public void StartGame(Arguments args)
        {

        }

        public bool RequiredContentIsInstalled()
        {
            return false;
        }

        public void Dispose()
        {

        }

    }
}