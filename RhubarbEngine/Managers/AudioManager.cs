using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using SteamAudio;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using OpenAL;

namespace RhubarbEngine.Managers
{
    public class AudioManager : IManager
    {
        private Engine engine;

        private bool running = false;
        public Stopwatch stopwatch { get; private set; }

        public const int SamplingRate = 44100;

        public const int AudioFrameSize = 2048;

        public const int AudioFrameSizeInBytes = (AudioFrameSize * sizeof(float));
        //OpenAL
        private IntPtr alAudioDevice;
        private IntPtr alAudioContext;

        public const int BufferFormatStereoFloat32 = 0x10011;

        private IntPtr outBuff;
        
        public float[] ee = new float[AudioFrameSize * 2];

        public IPL._IPLHRTF_t hrtf;

        //Steam Audio
        public IPL._IPLContext_t iplContext;
        public IPL.AudioBuffer iplOutputBuffer;

        public IPL.AudioSettings iplAudioSettings = new IPL.AudioSettings { frameSize = AudioFrameSize,samplingRate = SamplingRate};


        public uint sourceId;

        private uint[] alBuffers;

        public Task task;

        public unsafe IManager initialize(Engine _engine)
        {
            engine = _engine;

            stopwatch = new Stopwatch();

            outBuff = Marshal.AllocHGlobal(AudioFrameSizeInBytes * 2);

            alBuffers = new uint[2];
            PrepareOpenAL();
            PrepareSteamAudio();

            AL.GenBuffers(alBuffers.Length, alBuffers);
            AL.GenSource(out sourceId);

            stopwatch.Start();

            Console.WriteLine("Starting Audio task");
            running = true;
            task = Task.Run(Updater);
            return this;
        }

        private void steamLogFunction(IPL.LogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message)
        {
            switch (level)
            {
                case IPL.LogLevel.Info:
                    Console.WriteLine("Steam Audio Info" + message);
                    break;
                case IPL.LogLevel.Warning:
                    Logger.Log("Steam Audio Warning" + message);
                    break;
                case IPL.LogLevel.Error:
                    Logger.Log("Steam Audio Error" + message,true);
                    break;
                case IPL.LogLevel.Debug:
                    Console.WriteLine("Steam Audio Debug" + message);
                    break;
                default:
                    break;
            }
        }
        private void PrepareSteamAudio()
        {
            //Steam Audio Initialization

            var set = new IPL.ContextSettings { logCallback = steamLogFunction,version = ((((uint)(4) << 16) | ((uint)(0) << 8) | ((uint)(0)))) };

            IPL.ContextCreate(ref set, out iplContext);


            var herfset = new IPL.HRTFSettings { type = IPL.HRTFType.Default};

            IPL.HRTFCreate(iplContext, ref iplAudioSettings, ref herfset, out hrtf);


            Logger.Log("Created SteamAudio context.");
            IPL.AudioBufferAllocate(engine.audioManager.iplContext, 2, AudioFrameSize, ref iplOutputBuffer);

            Logger.Log("SteamAudio is ready.");

        }
        

        public unsafe void Updater()
        {
            while (running)
            {
                IPL.AudioBufferDeinterleave(iplContext, ref ee[0], ref iplOutputBuffer);
                RunOutput();
                Update();
            }
        }

        public void RunOutput()
        {
            foreach (var world in engine.worldManager.worlds)
            {
                if(world.Focus != World.World.FocusLevel.Background)
                {
                    foreach (var comp in world.updateLists.audioOutputs)
                    {
                        comp.AudioUpdate();
                        IPL.AudioBufferMix(iplContext, ref comp.iplOutputBuffer, ref iplOutputBuffer);
                    }

                }
            }
        }

        public unsafe void Update()
        {
            AL.GetSource(sourceId, AL.GetSourceInt.BuffersProcessed, out int numProcessedBuffers);
            AL.GetSource(sourceId, AL.GetSourceInt.BuffersQueued, out int numQueuedBuffers);

            int buffersToAdd = alBuffers.Length - numQueuedBuffers + numProcessedBuffers;

            while (buffersToAdd > 0)
            {
                uint bufferId = alBuffers[buffersToAdd - 1];

                if (numProcessedBuffers > 0)
                {
                    AL.SourceUnqueueBuffers(sourceId, 1, &bufferId);

                    numProcessedBuffers--;
                }
                IPL.AudioBufferInterleave(iplContext, ref iplOutputBuffer, outBuff);
                AL.BufferData(bufferId, BufferFormatStereoFloat32, outBuff, AudioFrameSizeInBytes * 2, SamplingRate);
                AL.SourceQueueBuffers(sourceId, 1, &bufferId);
                CheckALErrors();

                buffersToAdd--;
            }

            //Start playback whenever it stops
            AL.GetSource(sourceId, AL.GetSourceInt.SourceState, out int sourceState);

            if ((AL.SourceState)sourceState != AL.SourceState.Playing)
            {
                AL.SourcePlay(sourceId);
            }

            CheckALErrors();
        }

        private void CheckALErrors()
        {
            var error = AL.GetError();

            if (error != AL.Error.NoError)
            {
                throw new Exception($"OpenAL Error: {error}");
            }
        }

        private void PrepareOpenAL()
        {
            alAudioDevice = ALC.OpenDevice(null);
            alAudioContext = ALC.CreateContext(alAudioDevice, null);

            if (!ALC.MakeContextCurrent(alAudioContext))
            {
                throw new InvalidOperationException("Unable to make context current");
            }

            Logger.Log("Created OpenAL context.");

            //Require float32 support.
            const string Float32Extension = "AL_EXT_float32";

            if (!AL.IsExtensionPresent(Float32Extension))
            {
                throw new Exception($"This program requires '{Float32Extension}' OpenAL extension to function.");
            }

            CheckALErrors();

            Logger.Log("OpenAL is ready.");
        }
        private void UnloadOpenAL()
        {
            ALC.DestroyContext(alAudioContext);
            ALC.CloseDevice(alAudioDevice);
        }


        private void UnloadSteamAudio()
        {
            IPL.AudioBufferFree(iplContext, ref iplOutputBuffer);
            IPL.ContextRelease(ref iplContext);
        }

        public void unloadAll()
        {
            running = false;
            UnloadOpenAL();
            UnloadSteamAudio();
        }
    }
}
