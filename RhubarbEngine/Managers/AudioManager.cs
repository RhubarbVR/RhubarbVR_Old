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

namespace RhubarbEngine.Managers
{
    public interface IAudioManager : IManager
    {
        int SamplingRate { get; }
        Stopwatch Stopwatch { get; }
        int AudioFrameSize { get; }
        int AudioFrameSizeInBytes { get; }
        Thread Task { get; }

        void CleanUp();
    }


    public class AudioManager : IAudioManager
    {
		private IEngine _engine;

		private bool _running = false;
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

        public Thread Task;

        public unsafe IManager Initialize(IEngine _engine)
		{
            if(!_engine.Audio)
            {
                return this;
            }


            Task = new Thread(WorkerThread);

            return this;
		}

        private void WorkerThread()
        {

        }

        public void Update()
        {
        }

        public void CleanUp()
        {
        }
    }
}
