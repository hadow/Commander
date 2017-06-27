using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MusicPlaylistInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new MusicPlaylist();
        }
    }
    public class MusicPlaylist
    {
    }
}