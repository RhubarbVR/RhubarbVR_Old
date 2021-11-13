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
using OpenAL;
namespace RhubarbEngine.Managers
{
    public unsafe interface IAudioManager : IManager
    {
        int SamplingRate { get; }
        Stopwatch Stopwatch { get; }
        int AudioFrameSize { get; }
        int AudioFrameSizeInBytes { get; }
        Thread Task { get; }
        PlaybackDevice Device { get; set; }

        void CleanUp();
    }


    public unsafe class AudioManager : IAudioManager
    {
        

        private IEngine _engine;

		private bool _running;
        public Stopwatch Stopwatch { get; private set; }

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
                return AudioFrameSize * sizeof(short);
            }
        }

        Thread IAudioManager.Task
        {
            get
            {
                return Task;
            }
        }

        public Thread Task;

        public PlaybackDevice Device { get; set; }

        public unsafe IManager Initialize(IEngine _engine)
		{
            this._engine = _engine;
            if(!_engine.Audio)
            {
                return this;
            }

            Task = new Thread(WorkerThread);

            try
            {
                OpenALHelper.Start();
                if (OpenALHelper.PlaybackDevices.Length <= 0)
                {
                    throw new Exception("No playback devices found");
                }
                _engine.Logger.Log($"Starting with audio playback with {OpenALHelper.PlaybackDevices[0].DeviceName}",true);
                Device = OpenALHelper.PlaybackDevices[0];
                _running = true;
            }
            catch
            {
                _running = false;
                _engine.Audio = false;
            }
            return this;
		}




        private void WorkerThread()
        {
            while (_running)
            {
                try
                {
                    foreach (var item in _engine.WorldManager.Worlds)
                    {
                        if (item.Focus != World.World.FocusLevel.Background)
                        {
                            foreach (var audioOutput in item.updateLists.audioOutputs)
                            {
                            //    audioOutput.UpdateAudio();
                            }
                        }
                    }
                }
                catch 
                {
                }
                Thread.Sleep(5);
            }
        }

        public void Update()
        {
            var Position = _engine.WorldManager.LocalWorld.HeadTrans.Translation;

        }

        public void CleanUp()
        {
            _running = false;
        }
    }
}
