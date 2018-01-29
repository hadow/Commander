using System;
using System.Collections.Generic;
using EW.Traits;
using EW.GameRules;
using System.Linq;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Trait for music handling,Attach this to the world actor.
    /// </summary>
    public class MusicPlaylistInfo : ITraitInfo
    {

        public readonly string StartingMusic = null;

        public readonly string VictoryMusic = null;


        public readonly string DefeatMusic = null;

        public readonly string BackgroundMusic = null;

        public readonly bool DisableWorldSounds = false;



        public object Create(ActorInitializer init)
        {
            return new MusicPlaylist(init.World,this);
        }
    }
    public class MusicPlaylist:INotifyActorDisposing
    {
        readonly MusicPlaylistInfo info;
        readonly World world;

        readonly MusicInfo[] random;
        readonly MusicInfo[] playlist;

        public readonly bool IsMusicInstalled;
        public readonly bool IsMusicAvailable;

        public bool CurrentSongIsBackground { get; private set; }

        MusicInfo currentSong;
        MusicInfo currentBackgroundSong;
        public MusicPlaylist(World world,MusicPlaylistInfo info)
        {
            this.info = info;
            this.world = world;

            if (info.DisableWorldSounds)
                WarGame.Sound.DisableWorldSounds = true;

            IsMusicInstalled = world.Map.Rules.InstalledMusic.Any();
            if (!IsMusicInstalled)
                return;

            playlist = world.Map.Rules.InstalledMusic.Where(a => !a.Value.Hidden).Select(a => a.Value).ToArray();

            random = playlist.Shuffle(WarGame.CosmeticRandom).ToArray();
            IsMusicAvailable = playlist.Any();


            if (SongExists(info.BackgroundMusic))
            {
                currentSong = currentBackgroundSong = world.Map.Rules.Music[info.BackgroundMusic];
                CurrentSongIsBackground = true;
            }
            else
            {
                currentSong = random.FirstOrDefault();
            }

            if (SongExists(info.StartingMusic))
            {
                currentSong = world.Map.Rules.Music[info.StartingMusic];
                CurrentSongIsBackground = false;
            }

            //Play();
        }


        bool SongExists(string song)
        {
            return !string.IsNullOrEmpty(song) &&
                world.Map.Rules.Music.ContainsKey(song) &&
                world.Map.Rules.Music[song].Exists;
        }

        bool SongExists(MusicInfo song)
        {
            return song != null && song.Exists;
        }


        void Play()
        {
            if (!SongExists(currentSong))
                return;

            WarGame.Sound.PlayMusicThen(currentSong, () =>
            {
                if (!CurrentSongIsBackground && !WarGame.Settings.Sound.Repeat)
                    currentSong = GetNextSong();

                //Play();
            });
        }


        public void Play(MusicInfo music)
        {
            if (music == null)
                return;

            currentSong = music;
            CurrentSongIsBackground = false;

            Play();
        }


        public void Play(MusicInfo music,Action onComplete)
        {
            if (music == null)
                return;

            currentSong = music;
            CurrentSongIsBackground = false;
            WarGame.Sound.PlayMusicThen(music, onComplete);
        }


        public MusicInfo GetNextSong()
        {
            return GetSong(false);
        }

        public MusicInfo GetPrevSong()
        {
            return GetSong(true);
        }

        MusicInfo GetSong(bool reverse)
        {
            if (!IsMusicAvailable)
                return null;

            var songs = WarGame.Settings.Sound.Shuffle ? random : playlist;

            var next = reverse ? songs.Reverse().SkipWhile(m => m != currentSong).
                Skip(1).FirstOrDefault() ?? songs.Reverse().FirstOrDefault() : songs.SkipWhile(m => m != currentSong).Skip(1).FirstOrDefault() ?? songs.FirstOrDefault();

            if (SongExists(next))
                return next;

            return null;
        }
        public void Disposing(Actor self)
        {
            if (currentSong != null)
                WarGame.Sound.StopMusic();

            WarGame.Sound.DisableWorldSounds = false;
        }
    }
}