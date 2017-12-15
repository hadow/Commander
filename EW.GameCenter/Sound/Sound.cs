using System;
using System.IO;
using EW.FileSystem;
using EW.Graphics;
using EW.Primitives;
using EW.GameRules;
namespace EW
{

    public interface ISoundSource:IDisposable { }
    public interface ISoundLoader
    {
        bool TryParseSound(Stream stream, out ISoundForamt sound);
    }

    public interface ISoundForamt : IDisposable
    {
        int Channels { get; }

        int SampleBits { get; }

        int SampleRate { get; }

        float LengthInSeconds { get; }

        Stream GetPCMInputStream();

    }
    public sealed class Sound:IDisposable
    {
        //readonly SoundEffectInstance soundEffectInstance;
        ISoundLoader[] loaders;

        Cache<string, ISoundSource> sounds;

        IReadOnlyFileSystem fileSystem;

        MusicInfo currentMusic;

        Action onMusicComplete;

        public bool MusicPlaying { get; private set; }

        public MusicInfo CurrentMusic { get { return currentMusic; } }

        public Sound(SoundSettings soundSettings)
        {

        }

        public void Tick()
        {
            if (MusicPlaying)
            {
                StopMusic();
                onMusicComplete();
            }
        }

        public void StopMusic()
        {
            
        }

        T LoadSound<T>(string filename,Func<ISoundForamt,T> loadFormat)
        {
            if (!fileSystem.Exists(filename))
            {
                return default(T);
            }
            using(var stream = fileSystem.Open(filename))
            {
                ISoundForamt soundFormat;
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

            if (sounds != null)
                foreach (var soundSource in sounds.Values)
                    if (soundSource != null)
                        soundSource.Dispose();

            this.loaders = loaders;
            this.fileSystem = fileSystem;
            Func<ISoundForamt, ISoundSource> loadIntoMemory = null;
            sounds = new Cache<string, ISoundSource>(filename => LoadSound(filename,loadIntoMemory));
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

        /// <summary>
        /// 
        /// </summary>
        public void MuteAudio()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void UnmuteAudio()
        {

        }

        public void Dispose()
        {

        }
    }
}