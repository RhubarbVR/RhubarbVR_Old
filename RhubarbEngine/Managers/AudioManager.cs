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

        public const int SamplingRate = 48000;

        public const int AudioFrameSize = 2048;
        public const int AudioFrameSizeInBytes = (AudioFrameSize * sizeof(float));
        //OpenAL
        private IntPtr alAudioDevice;
        private IntPtr alAudioContext;

        public const int BufferFormatStereoFloat32 = 0x10011;

        //Steam Audio
        public IntPtr iplContext;
        private IPL.AudioFormat iplFormatMono;
        private IPL.AudioFormat iplFormatStereo;
        public IPL.AudioBuffer iplOutputBuffer;
        private IPL.RenderingSettings iplRenderingSettings;

        public uint sourceId;

        private uint[] alBuffers;

        public Task task;

        public unsafe IManager initialize(Engine _engine)
        {
            engine = _engine;

            stopwatch = new Stopwatch();

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
        private void PrepareSteamAudio()
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


        public void Updater()
        {
            while (running)
            {
                try
                {
                    if (engine.worldManager != null)
                    {
                        RunOutput();
                        Update();
                    }
                }
                catch(Exception e)
                {
                    Logger.Log("Audio Error" + e.ToString(), true);
                }
                Thread.Sleep(1);
            }
        }

        public void RunOutput()
        {
            int i = 0;
            var comps = new List<IPL.AudioBuffer>();
            foreach (var world in engine.worldManager.worlds)
            {
                if(world.Focus != World.World.FocusLevel.Background)
                {
                    foreach (var comp in world?.updateLists.audioOutputs ?? new List<Components.Audio.AudioOutput>())
                    {
                        comp.AudioUpdate();
                        comps.Add(comp.iplOutputBuffer);
                        i++;
                    }

                }
            }
            if (i == 0) return;
            var bufs = comps.ToArray();
            IPL.MixAudioBuffers(bufs.Length, ref bufs[0], iplOutputBuffer);
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
            IPL.DestroyContext(ref iplContext);
            IPL.Cleanup();
        }

        public void unloadAll()
        {
            running = false;
            UnloadOpenAL();
            UnloadSteamAudio();
        }
    }
}
