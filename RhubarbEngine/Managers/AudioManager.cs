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

        public Stopwatch stopwatch { get; private set; }

        public const int SamplingRate = 44100;

        public const int AudioFrameSize = 1024;
        public const int AudioFrameSizeInBytes = AudioFrameSize * sizeof(float);
        //OpenAL
        private static IntPtr alAudioDevice;
        private static IntPtr alAudioContext;
        private MemoryStream audioStream;

        public const int BufferFormatStereoFloat32 = 0x10011;

        //Steam Audio
        private static IntPtr iplContext;
        private static IPL.AudioFormat iplFormatMono;
        private static IPL.AudioFormat iplFormatStereo;
        private static IPL.AudioBuffer iplOutputBuffer;
        private static IPL.RenderingSettings iplRenderingSettings;

        public uint sourceId;

        private uint[] alBuffers;
        private byte[] frameInputBuffer;


        public unsafe IManager initialize(Engine _engine)
        {
            engine = _engine;
            audioStream = new MemoryStream(23);

            stopwatch = new Stopwatch();

            alBuffers = new uint[2];
            frameInputBuffer = new byte[AudioFrameSizeInBytes];
            PrepareOpenAL();
            PrepareSteamAudio();

            AL.GenBuffers(alBuffers.Length, alBuffers);
            AL.GenSource(out sourceId);

            stopwatch.Start();

            return this;
        }
        private static void PrepareSteamAudio()
        {
            //Steam Audio Initialization

            IPL.CreateContext(null, null, null, out iplContext);

            Logger.Log("Created SteamAudio context.");

            iplRenderingSettings = new IPL.RenderingSettings
            {
                samplingRate = SamplingRate,
                frameSize = AudioFrameSize
            };

            //Audio Formats

            iplFormatMono = new IPL.AudioFormat
            {
                channelLayoutType = IPL.ChannelLayoutType.Speakers,
                channelLayout = IPL.ChannelLayout.Mono,
                channelOrder = IPL.ChannelOrder.Interleaved
            };
            iplFormatStereo = new IPL.AudioFormat
            {
                channelLayoutType = IPL.ChannelLayoutType.Speakers,
                channelLayout = IPL.ChannelLayout.Stereo,
                channelOrder = IPL.ChannelOrder.Interleaved
            };

            IntPtr outputDataPtr = Marshal.AllocHGlobal(AudioFrameSizeInBytes * 2);

            iplOutputBuffer = new IPL.AudioBuffer
            {
                format = iplFormatStereo,
                numSamples = iplRenderingSettings.frameSize,
                interleavedBuffer = outputDataPtr
            };

            Logger.Log("SteamAudio is ready.");
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

                AL.BufferData(bufferId, BufferFormatStereoFloat32, iplOutputBuffer.interleavedBuffer, AudioFrameSizeInBytes * 2, iplRenderingSettings.samplingRate);

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

        private static void CheckALErrors()
        {
            var error = AL.GetError();

            if (error != AL.Error.NoError)
            {
                throw new Exception($"OpenAL Error: {error}");
            }
        }

        private static void PrepareOpenAL()
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
        private static void UnloadOpenAL()
        {
            ALC.DestroyContext(alAudioContext);
            ALC.CloseDevice(alAudioDevice);
        }


        private static void UnloadSteamAudio()
        {
            IPL.DestroyContext(ref iplContext);
            IPL.Cleanup();
        }

        public void unloadAll()
        {
            UnloadOpenAL();
            UnloadSteamAudio();
        }
    }
}
