using System;
using System.Threading;
using System.Numerics;

namespace OpenAL
{
    /// <summary>
    /// Audio capture device.
    /// </summary>
    public class CaptureDevice : IDisposable
    {
        public CaptureDevice(string deviceName)
        {
            DeviceName = deviceName;
        }
        ~CaptureDevice()
        {
            Dispose();
        }

        /// <summary>
        /// Open a capture stream to capture audio from the device.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="format">Audio format.</param>
        /// <param name="bufferSizeMs">Buffer size in milliseconds. Read operations will provide data in mulitples of this (ie. 10ms would provide data in multiples of 10 like 10ms, 20ms, 30ms depending on how often data is read).</param>
        /// <returns></returns>
        public CaptureStream OpenStream(int sampleRate, OpenALAudioFormat format, int bufferSizeMs)
        {
            return new CaptureStream(sampleRate, format, DeviceName, bufferSizeMs);
        }

        /// <summary>
        /// Gets the format.
        /// </summary>
        public OpenALAudioFormat Format { get; private set; }

        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        public string DeviceName { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is CaptureDevice))
                return false;
            return ((CaptureDevice)obj).DeviceName == DeviceName;
        }

        public void Dispose()
        {
            
        }
    }
}
