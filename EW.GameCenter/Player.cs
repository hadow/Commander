using System;
using System.Collections.Generic;

namespace EW
{
    public enum WinState { Undefined,Won,Lost}

    public enum PowerState { Normal,Low,Critical}


    public class Player
    {
        public WinState WinState = WinState.Undefined;

        public readonly string InternalName;

        public readonly Actor PlayerActor;
    }
}