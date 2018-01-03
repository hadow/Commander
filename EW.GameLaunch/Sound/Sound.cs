using System;
using System.IO;
using System.Collections.Generic;
using EW.FileSystem;
using EW.Framework.Audio;
using EW.Primitives;
using EW.GameRules;
using EW.Framework;
namespace EW
{

    //public interface ISoundSource:IDisposable { }
    public interface ISoundLoader
    {
        bool TryParseSound(Stream stream, out ISoundFormat sound);
    }

    public interface ISoundFormat : IDisposable
    {
        int Channels { get; }

        int SampleBits { get; }

        int SampleRate { get; }

        float LengthInSeconds { get; }

        Stream GetPCMInputStream();

    }

    public enum SoundType { World,UI}
    public sealed class Sound:IDisposable
    {
        //readonly SoundEffectInstance soundEffectInstance;

        readonly ISoundEngine soundEngine;

        ISoundLoader[] loaders;

        Cache<string, ISoundSource> sounds;

        IReadOnlyFileSystem fileSystem;

        ISoundSource rawSource;

        ISound music;

        ISound video;

        MusicInfo currentMusic;
        Dictionary<uint, ISound> currentSounds;

        public bool MusicPlaying { get; private set; }
        public bool DisableWorldSounds { get; set; }
        public MusicInfo CurrentMusic { get { return currentMusic; } }

        Action onMusicComplete;

        float soundVolumeModifier = 1.0f;
        public float SoundVolume
        {
            get { return WarGame.Settings.Sound.SoundVolume; }
            set
            {
                WarGame.Settings.Sound.SoundVolume = value;
                soundEngine.SetSoundVolume(InternalSoundVolume, music, video);
            }
        }
        float InternalSoundVolume { get { return SoundVolume * soundVolumeModifier; } }
        public Sound(SoundSettings soundSettings)
        {
            soundEngine = OpenALSoundController.GetInstance;

            if (soundSettings.Mute)
                MuteAudio();
        }


        public float MusicVolume
        {
            get
            {
                return WarGame.Settings.Sound.MusicVolume;
            }
            set
            {
                WarGame.Settings.Sound.MusicVolume = value;
                if (music != null)
                    music.Volume = value;
            }
        }

        public void Tick()
        {
            if (MusicPlaying && music.Complete)
            {
                StopMusic();
                onMusicComplete();
            }
        }

        public void StopMusic()
        {
            
        }

        T LoadSound<T>(string filename,Func<ISoundFormat,T> loadFormat)
        {
            if (!fileSystem.Exists(filename))
            {
                Android.Util.Log.Debug("sound", "LoadSound,file does not exist:{0}", filename);
                return default(T);
            }
            using(var stream = fileSystem.Open(filename))
            {
                ISoundFormat soundFormat;
                foreach(var loader in loaders)
                {
                    stream.Position = 0;
                    if(loader.TryParseSound(stream,out soundFormat))
                    {
                        var source = loadFormat(soundFormat);
                        soundFormat.Dispose();
                        return source;
                    }
                }
            }
            throw new InvalidDataException(filename + " is not a valid sound file!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loaders"></param>
        /// <param name="fileSystem"></param>
        public void Initialize(ISoundLoader[] loaders,IReadOnlyFileSystem fileSystem)
        {
            StopMusic();
            soundEngine.StopAllSounds();

            if (sounds != null)
                foreach (var soundSource in sounds.Values)
                    if (soundSource != null)
                        soundSource.Dispose();

            this.loaders = loaders;
            this.fileSystem = fileSystem;
            Func<ISoundFormat, ISoundSource> loadIntoMemory = soundFormat =>soundEngine.AddSoundSourceFromMemory(
                soundFormat.GetPCMInputStream().ReadAllBytes(),soundFormat.Channels,soundFormat.SampleBits,soundFormat.SampleRate);

            sounds = new Cache<string, ISoundSource>(filename => LoadSound(filename,loadIntoMemory));

            currentSounds = new Dictionary<uint, ISound>();
            video = null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="player"></param>
        /// <param name="name"></param>
        /// <param name="headRelative"></param>
        /// <param name="pos"></param>
        /// <param name="volumeModifier"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        ISound Play(SoundType type,Player player,string name,bool headRelative,Vector3 pos,float volumeModifier = 1f,bool loop = false)
        {
            if (string.IsNullOrEmpty(name) || (DisableWorldSounds && type == SoundType.World))
                return null;

            if (player != null && player != player.World.LocalPlayer)
                return null;

            return soundEngine.Play2D(sounds[name], loop, headRelative, pos, InternalSoundVolume * volumeModifier, true);
        }

        public void PlayMusic(MusicInfo mi)
        {
            PlayMusicThen(mi, () => { });
        }


        public void PlayMusicThen(MusicInfo mi,Action then)
        {

            if (mi == null || !mi.Exists)
                return;

            onMusicComplete = then;

            if(mi==currentMusic && music != null)
            {
                soundEngine.PauseSound(music, false);
                MusicPlaying = true;
                return;
            }

            StopMusic();

            Func<ISoundFormat, ISound> stream = soundFormat =>
             soundEngine.Play2DStream(soundFormat.GetPCMInputStream(), 
             soundFormat.Channels, soundFormat.SampleBits, soundFormat.SampleRate, false, true, Vector3.Zero, MusicVolume);

            music = LoadSound(mi.Filename, stream);

            if(music == null)
            {
                onMusicComplete = null;
                return;
            }

            currentMusic = mi;
            MusicPlaying = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetListenerPosition(WPos position)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void StopAudio()
        {

        }

        public void StopVideo()
        {
            if (video != null)
                soundEngine.StopSound(video);
        }

        /// <summary>
        /// 
        /// </summary>
        public void MuteAudio()
        {
            soundEngine.Volume = 0f;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnmuteAudio()
        {
            soundEngine.Volume = 1f;
        }

        public void Dispose()
        {
            StopAudio();
            if (sounds != null)
                foreach (var soundSource in sounds.Values)
                    if (soundSource != null)
                        soundSource.Dispose();

            soundEngine.Dispose();
        }
    }
}