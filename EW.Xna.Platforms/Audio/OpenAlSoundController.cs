using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
#if ANDROID
using Android.Content.PM;
using Android.Content;
using Android.Media;
using Android.OS;
#endif

#if GLES
using OpenTK;
using OpenTK.Audio.OpenAL;
#endif

namespace EW.Xna.Platforms.Audio
{

    internal static class ALHelper
    {
        public static void CheckError(string message="",params object[] args)
        {
            ALError error;

            if((error = AL.GetError()) != ALError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = string.Format(message, args);

                throw new InvalidOperationException(message + " (Reason: " + AL.GetError() + ")");
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OpenAlSoundController:IDisposable
    {
        private static OpenAlSoundController _instance = null;

        private IntPtr _device;

        ContextHandle _context;

        ContextHandle NullContext = ContextHandle.Zero;

        private AlcError _lastOpenALError;
        private int[] allSourcesArray;


        private List<int> availableSourcesCollection;
        private List<int> inUseSourcesCollection;

        private bool _bSoundAvailable = false;

        internal const int MAX_NUMBER_OF_SOURCES = 32;

        bool _isDisposed;

        private Exception _SoundInitException;

#if ANDROID
        private const int DEFAULT_FREQUENCY = 48000;
        private const int DEFAULT_UPDATE_SIZE = 512;
        private const int DEFAULT_UPDATE_BUFFER_COUNT = 2;
#endif
#if ANDROID

        const string Lib = "openal32.dll";
        const CallingConvention Style = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "alcDevicePauseSOFT", ExactSpelling = true, CallingConvention = Style)]
        unsafe static extern void alcDevicePauseSOFT(IntPtr device);

        [DllImport(Lib, EntryPoint = "alcDeviceResumeSOFT", ExactSpelling = true, CallingConvention = Style)]
        unsafe static extern void alcDeviceResumeSOFT(IntPtr device);



        void Acivity_Paused(object sender, EventArgs e)
        {
            alcDevicePauseSOFT(_device);
        }

        void Acivity_Resumed(object sender, EventArgs e)
        {
            alcDeviceResumeSOFT(_device);
        }
#endif

        private OpenAlSoundController()
        {
            if (!OpenSoundController())
                return;

            _bSoundAvailable = true;

            allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];

            AL.GenSources(allSourcesArray);

            ALHelper.CheckError("Failed to generate sources");

            availableSourcesCollection = new List<int>(allSourcesArray);
            inUseSourcesCollection = new List<int>();
        }

        ~OpenAlSoundController()
        {
            Dispose(false);
        }

        public static OpenAlSoundController GetInstance
        {
            get
            {
                if (_instance == null)
                    _instance = new OpenAlSoundController();
                return _instance;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool OpenSoundController()
        {
            try
            {
                _device = Alc.OpenDevice(string.Empty);
            }
            catch(Exception ex)
            {

            }

            if(CheckALError("Could not open Al device"))
            {
                return false;
            }

            if(_device != IntPtr.Zero)
            {
#if ANDROID
                AndroidGameActivity.Paused += Acivity_Paused;
                AndroidGameActivity.Resumed += Acivity_Resumed;

#endif
                int frequency = DEFAULT_FREQUENCY;
                int updateSize = DEFAULT_UPDATE_SIZE;
                int updateBuffers = DEFAULT_UPDATE_BUFFER_COUNT;
                if(Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
                {
                    var audioManager = Game.Activity.GetSystemService(Context.AudioService) as AudioManager;
                    if (audioManager != null)
                    {
                        var result = audioManager.GetProperty(AudioManager.PropertyOutputSampleRate);
                        if (!string.IsNullOrEmpty(result))
                            frequency = int.Parse(result, System.Globalization.CultureInfo.InvariantCulture);
                        result = audioManager.GetProperty(AudioManager.PropertyOutputFramesPerBuffer);
                        if (!string.IsNullOrEmpty(result))
                            updateSize = int.Parse(result, System.Globalization.CultureInfo.InvariantCulture);    
                    }

                    if ((int)Build.VERSION.SdkInt >= 19)
                    {
                        updateBuffers = 1;
                    }
                }
                else
                {

                }

                const int AlcFrequency = 0x1007;
                const int AlcUpdateSize = 0x1014;
                const int AlcUpdateBuffers = 0x1015;

                int[] attribute = new[]
                {
                    AlcFrequency,frequency,
                    AlcUpdateSize,updateSize,
                    AlcUpdateBuffers,updateBuffers,
                    0
                };

                _context = Alc.CreateContext(_device, attribute);

                if(CheckALError("Could not create Al context"))
                {
                    CleanUpOpenAl();
                    return false;
                }

                if(_context != NullContext)
                {
                    Alc.MakeContextCurrent(_context);
                    if(CheckALError("Could not make AL context current"))
                    {
                        CleanUpOpenAl();
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReserveSource()
        {
            if(!CheckInitState())
            {

            }

            int sourceNumber;

            lock(availableSourcesCollection)
            {
                if(availableSourcesCollection.Count == 0)
                {

                }

                sourceNumber = availableSourcesCollection.Last();
                inUseSourcesCollection.Add(sourceNumber);
                availableSourcesCollection.Remove(sourceNumber);
            }
            return sourceNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal bool CheckInitState()
        {
            if (!_bSoundAvailable)
            {
                if(_SoundInitException != null)
                {
                    Exception e = _SoundInitException;
                    _SoundInitException = null;
                    throw new NoAudioHardwareException("No audio hardware available",e);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public bool CheckALError(string operation)
        {
            _lastOpenALError = Alc.GetError(_device);

            if (_lastOpenALError == AlcError.NoError)
                return false;

            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        private void CleanUpOpenAl()
        {
            Alc.MakeContextCurrent(NullContext);

            if(_context != NullContext)
            {
                Alc.DestroyContext(_context);
                _context = NullContext;
            }
            if(_device != IntPtr.Zero)
            {
                Alc.CloseDevice(_device);
                _device = IntPtr.Zero;
            }

            _bSoundAvailable = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_bSoundAvailable)
                    {
                        for(int i = 0; i < allSourcesArray.Length; i++)
                        {
                            AL.DeleteSource(allSourcesArray[i]);
                            ALHelper.CheckError("Failed to delete source");
                        }
                        CleanUpOpenAl();
                    }
                }
                _isDisposed = true;
            }
        }
    }
}