using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using RVRAudioNative;

namespace RVRAudioNative
{
    public enum DeviceFlags
    {
        None,
        Hrtf,
    }
}

namespace RhubarbEngine.Managers
{
    public unsafe interface IAudioManager : IManager
    {
        int SamplingRate { get; }
        Stopwatch Stopwatch { get; }
        int AudioFrameSize { get; }
        int AudioFrameSizeInBytes { get; }
        Thread Task { get; }
        rvrAudioListener* DefaultListener { get; }

        void CleanUp();
    }


    public unsafe class AudioManager : IAudioManager
    {
		private readonly IEngine _engine;

		private bool _running;
        public Stopwatch Stopwatch { get; private set; }

        private rvrAudioDevice* _audioDevice;

        public int SamplingRate
        {
            get
            {
                return _engine?.SettingsObject.AudioSettings.SamplingRate ?? 48000;
            }
        }

        public int AudioFrameSize
        {
            get
            {
                return _engine?.SettingsObject.AudioSettings.AudioFrameSize ?? 2048;
            }
        }

        public int AudioFrameSizeInBytes
        {
            get
            {
                return AudioFrameSize * sizeof(float);
            }
        }

        Thread IAudioManager.Task
        {
            get
            {
                return Task;
            }
        }

        public rvrAudioListener* DefaultListener { get; set; }

        public Thread Task;

        public unsafe IManager Initialize(IEngine _engine)
		{
            if(!_engine.Audio)
            {
                return this;
            }
            Forward = new Vector3(0, 0, 1);
            Up = new Vector3(0, 1, 0);

            Task = new Thread(WorkerThread);

            try
            {
                _audioDevice = NativeAudio.rvrAudioCreate(null, (int)DeviceFlags.Hrtf);
                DefaultListener = NativeAudio.rvrAudioListenerCreate(_audioDevice);
                NativeAudio.rvrAudioListenerEnable(DefaultListener);
                _running = true;
            }
            catch
            {
                _running = false;
            }
            return this;
		}

        public Vector3 Velocity;

        private Vector3 _forward;

        public Vector3 Forward
        {
            get
            {
                return _forward;
            }
            set
            {
                if (value == Vector3.Zero)
                {
                    throw new InvalidOperationException("The value of the Forward vector can not be (0,0,0)");
                }

                _forward = Vector3.Normalize(value);
            }
        }
        private Vector3 _up;

        public Vector3 Up
        {
            get
            {
                return _up;
            }
            set
            {
                if (value == Vector3.Zero)
                {
                    throw new InvalidOperationException("The value of the Up vector can not be (0,0,0)");
                }

                _up = Vector3.Normalize(value);
            }
        }



        private void WorkerThread()
        {
            while (_running)
            {

            }
        }

        public void Update()
        {
            var Position = _engine.WorldManager.LocalWorld.HeadTrans.Translation;
            var Forward = _forward;
            var Up = _up;
            var Velocity = this.Velocity;
            NativeAudio.rvrAudioListenerPush3D(DefaultListener, (float*)&Position, (float*)&Forward, (float*)&Up, (float*)&Velocity);
        }

        public void CleanUp()
        {
        }
    }
}
