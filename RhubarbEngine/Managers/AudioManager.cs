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
using NAudio;
using NAudio.Wave;

namespace RhubarbEngine.Managers
{
    public class AudioManager : IManager
    {
        private Engine engine;

        public bool OpenAl => engine?.settingsObject.AudioSettings.OpenAL ?? true;

        //NAudio
        private DirectSoundOut waveOutDevice;
        private BufferedWaveProvider mainOutputStream;

        private bool running = false;
        public Stopwatch stopwatch { get; private set; }

        public int SamplingRate => engine?.settingsObject.AudioSettings.SamplingRate?? 48000;

        public int AudioFrameSize => engine?.settingsObject.AudioSettings.AudioFrameSize ?? 2048;

        public int AudioFrameSizeInBytes => (AudioFrameSize * sizeof(float));
        //OpenAL
        private IntPtr alAudioDevice;
        private IntPtr alAudioContext;

        public const int BufferFormatStereoFloat32 = 0x10011;

        private IntPtr outBuff;

        public float[] ee;

        public IPL._IPLHRTF_t hrtf;

        //Steam Audio
        public IPL._IPLContext_t iplContext;
        public IPL.AudioBuffer iplOutputBuffer;

        public IPL.AudioSettings iplAudioSettings;


        public uint sourceId;

        private uint[] alBuffers;

        public Thread task;

        public int audioframeTimeMs;

        public unsafe IManager initialize(Engine _engine)
        {
            audioframeTimeMs = AudioFrameSize/(SamplingRate / 1000);
            ee = new float[AudioFrameSize * 2];
            iplAudioSettings = new IPL.AudioSettings { frameSize = AudioFrameSize, samplingRate = SamplingRate };
            engine = _engine;
            GC.KeepAlive(ee);

            stopwatch = new Stopwatch();

            outBuff = Marshal.AllocHGlobal(AudioFrameSizeInBytes * 2);

            if (OpenAl)
            {
                PrepareOpenAL();
            }
            else
            {
                PrepareNAudio();
            }

            PrepareSteamAudio();

            if (OpenAl)
            {
                AL.GenBuffers(alBuffers.Length, alBuffers);
                AL.GenSource(out sourceId);
            }
            stopwatch.Start();

            Console.WriteLine("Starting Audio task");
            running = true;
            if (OpenAl)
            {
                task = new Thread(OpenALUpdater);
            }
            else
            {
                task = new Thread(NaudioUpdater);
            }
            task.Name = "Audio";
            task.IsBackground = true;
            task.Priority = ThreadPriority.Highest;
            task.Start();
            return this;
        }

        private void PrepareNAudio()
        {
            managedArray = new byte[(AudioFrameSizeInBytes * 2)];
            GC.KeepAlive(managedArray);

            try
            {
                waveOutDevice = new DirectSoundOut(70);
            }
            catch (Exception driverCreateException)
            {
                Console.WriteLine(String.Format("{0}", driverCreateException.Message));
                return;
            }
            
            mainOutputStream = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SamplingRate, 2));
            mainOutputStream.BufferLength = AudioFrameSizeInBytes*4;   
            mainOutputStream.DiscardOnBufferOverflow = false;

            try
            {
                waveOutDevice.Init(mainOutputStream);
                waveOutDevice.Play();
            }
            catch (Exception initException)
            {
                Console.WriteLine(String.Format("{0}", initException.Message), "Error Initializing Output");
                return;
            }
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
        

        public unsafe void OpenALUpdater()
        {
            while (running)
            {
                try
                {
                    OpenALUpdate();
                }
                catch
                {

                }
            }
        }
        private unsafe void NaudioUpdater()
        {
            while (running)
            {
                try
                {
                    NaudioUpdate();
                }
                catch
                {

                }
            }
        }

        public unsafe void RunOutput()
        {
                IPL.AudioBufferDeinterleave(iplContext, ee, ref iplOutputBuffer);
                foreach (var world in engine.worldManager.worlds)
                {
                    if (world.Focus != World.World.FocusLevel.Background)
                    {
                        foreach (var comp in world.updateLists.audioOutputs)
                        {
                            if (comp.isNotCulled)
                            {
                                comp.AudioUpdate();
                                IPL.AudioBufferMix(iplContext, ref comp.iplOutputBuffer, ref iplOutputBuffer);
                            }
                        }

                    }
                }
                IPL.AudioBufferInterleave(iplContext, ref iplOutputBuffer, outBuff);
        }
        byte[] managedArray;
        public unsafe void NaudioUpdate()
        {
            int space = (int)(waveOutDevice.GetPosition()% managedArray.Length);
            if (space < (int)(AudioFrameSizeInBytes/2))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                RunOutput();
                Marshal.Copy(outBuff, managedArray, 0, managedArray.Length);
                mainOutputStream.AddSamples(managedArray, 0, managedArray.Length);
                watch.Stop();
                Thread.Sleep((audioframeTimeMs - (int)watch.ElapsedMilliseconds)-5);
            }
        }

        public unsafe void OpenALUpdate()
        {
            AL.GetSource(sourceId, AL.GetSourceInt.BuffersProcessed, out int numProcessedBuffers);
            AL.GetSource(sourceId, AL.GetSourceInt.BuffersQueued, out int numQueuedBuffers);

            int buffersToAdd = alBuffers.Length - numQueuedBuffers + numProcessedBuffers;

            if (buffersToAdd <= 0) return;

            RunOutput();

            while (buffersToAdd > 0)
            {
                uint bufferId = alBuffers[buffersToAdd - 1];

                if (numProcessedBuffers > 0)
                {
                    AL.SourceUnqueueBuffers(sourceId, 1, &bufferId);

                    numProcessedBuffers--;
                }

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
            alBuffers = new uint[engine.settingsObject.AudioSettings.BufferCount];

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
            if (OpenAl)
            {
                UnloadOpenAL();
            }
            UnloadSteamAudio();
        }
    }
}
