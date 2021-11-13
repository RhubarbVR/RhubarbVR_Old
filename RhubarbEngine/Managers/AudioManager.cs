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
using RNumerics;

namespace RhubarbEngine.Managers
{
    public unsafe interface IAudioManager : IManager
    {
        int SamplingRate { get; }
        Stopwatch Stopwatch { get; }
        int AudioFrameSize { get; }
        int AudioFrameSizeInBytes { get; }
        PlaybackDevice Device { get; set; }

        void CleanUp();
    }


    public unsafe class AudioManager : IAudioManager
    {
        

        private IEngine _engine;

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


        public PlaybackDevice Device { get; set; }

        public unsafe IManager Initialize(IEngine _engine)
		{
            this._engine = _engine;
            if(!_engine.Audio)
            {
                return this;
            }

            try
            {
                OpenALHelper.Start();
                if (OpenALHelper.PlaybackDevices.Length <= 0)
                {
                    throw new Exception("No playback devices found");
                }
                _engine.Logger.Log($"Starting with audio playback with {OpenALHelper.PlaybackDevices[0].DeviceName}",true);
                Device = OpenALHelper.PlaybackDevices[0];
                Device.InitListener();
            }
            catch
            {
                _engine.Audio = false;
            }
            return this;
		}

        public void Update()
        {
            if(_engine.WorldManager.LocalWorld is null)
            {
                return;
            }

            Device.Listener.Velocity = (Device.Listener.Position - _engine.WorldManager.LocalWorld.HeadTrans.Translation) * (float)_engine.PlatformInfo.DeltaSeconds;

            Device.Listener.Position = _engine.WorldManager.LocalWorld.HeadTrans.Translation;
            Matrix4x4.Decompose(_engine.WorldManager.LocalWorld.HeadTrans, out _, out var rot, out _);

            Device.Listener.Orientation = new Orientation
            {
                At = ((Quaternionf)rot).AxisZ.ToSystemNumrics() * -1,
                Up = Vector3f.AxisY.ToSystemNumrics()
            };

        }

        public void CleanUp()
        {
        }
    }
}
