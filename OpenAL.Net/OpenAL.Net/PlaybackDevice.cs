using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenAL
{
    /// <summary>
    /// Audio playback device.
    /// </summary>
    public class PlaybackDevice : IDisposable
    {
        IntPtr _device = IntPtr.Zero;
        IntPtr _context = IntPtr.Zero;
        readonly List<PlaybackStream> _streams = new List<PlaybackStream>(); 

        public PlaybackDevice(string deviceName)
        {
            DeviceName = deviceName;
        }
        ~PlaybackDevice()
        {
            Dispose();
        }

        public PlaybackStream OpenStream(uint sampleRate, OpenALAudioFormat format)
        {
            EnsureDeviceIsOpen();
            var ret = new PlaybackStream(sampleRate, format, this, _context);
            lock(_streams)
                _streams.Add(ret);
            return ret;
        }

        internal void ClosedStream(PlaybackStream stream)
        {
            lock (_streams)
            {
                _streams.Remove(stream);
                if (_streams.Count == 0)
                {
                    CloseDevice();
                }
            }
        }

        void EnsureDeviceIsOpen()
        {
            if (_device != IntPtr.Zero) return;
            _device = API.alcOpenDevice(DeviceName);
            _context = API.alcCreateContext(_device, IntPtr.Zero);
        }

        private void CloseDevice()
        {
            if (_device == IntPtr.Zero)
                return;

            API.alcDestroyContext(_context);
            API.alcCloseDevice(_device);
            _device = IntPtr.Zero;
        }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        public string DeviceName { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PlaybackDevice))
                return false;
            return ((PlaybackDevice)obj).DeviceName == DeviceName;
        }

        public void Dispose()
        {

        }
    }
}
