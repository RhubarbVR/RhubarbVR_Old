using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Numerics;

namespace OpenAL
{
    /// <summary>
    /// A stream for capturing audio samples from an input device.
    /// The device will be opened the first time Read is called (incl. BeingRead and ReadAsync).
    /// </summary>
    public class CaptureStream : Stream
    {
        /// <summary>
        /// Capture device.
        /// </summary>
        private IntPtr _device = IntPtr.Zero;

        /// <summary>
        /// Number of bytes per sample.
        /// </summary>
        private readonly int _bytesPerSample;

        /// <summary>
        /// Set to true when capturing has started.
        /// </summary>
        private bool _capturing;

        /// <summary>
        /// Lock object to prevent closing the capture device while attempting to read from it.
        /// </summary>
        private object _readWriteLock = new object();

        private int _samplesPerBuffer;

        /// <summary>
        /// Create a capture stream on given device.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="format">Capture format.</param>
        /// <param name="deviceName">Name of device to capture audio from.</param>
        /// <param name="bufferSizeMs">Size of the audio buffer in milliseconds.</param>
        internal CaptureStream(int sampleRate, OpenALAudioFormat format, string deviceName, int bufferSizeMs)
        {
            if (deviceName == null) throw new ArgumentNullException("deviceName");

            var samplesPerBuffer = sampleRate / (1000 / bufferSizeMs);
            _samplesPerBuffer = samplesPerBuffer;

            _bytesPerSample = 1;
            switch (format)
            {
                case OpenALAudioFormat.Mono16Bit:
                    _bytesPerSample = 2;
                    break;
                case OpenALAudioFormat.Stereo8Bit:
                    _bytesPerSample = 2;
                    break;
                case OpenALAudioFormat.Stereo16Bit:
                    _bytesPerSample = 4;
                    break;
            }

            var bufferSize = samplesPerBuffer * _bytesPerSample;
            _device = API.alcCaptureOpenDevice(deviceName, (uint)sampleRate, format, bufferSize * 4);
        }

        public override bool CanRead
        {
            get
            {
                return _device != IntPtr.Zero;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            if (_device == IntPtr.Zero)
                throw new ObjectDisposedException("_device");

            lock (_readWriteLock)
            {
                if (!_capturing)
                {
                    if (!CanRead)
                        return 0;
                    API.alcCaptureStart(_device);
                    _capturing = true;
                }

                var samplesAvailable = GetSamplesAvailable();
                while (samplesAvailable < _samplesPerBuffer)
                {
                    //  Closed/Disposed?
                    if (!CanRead)
                        return 0;

                    samplesAvailable = GetSamplesAvailable();
                    Thread.Sleep(1);
                }
                var bytesAvaileable = samplesAvailable*_bytesPerSample;
                var copyByteCount = bytesAvaileable;
                if (bytesAvaileable > count) copyByteCount = count;

                var copySampleCount = copyByteCount/_bytesPerSample;
                //  Offset is 0, we can skip a temporary buffer here
                if (offset == 0)
                {
                    ReadIntoBuffer(buffer, copySampleCount);
                }
                else
                {
                    var tempBuffer = new byte[copyByteCount];
                    ReadIntoBuffer(tempBuffer, copySampleCount);
                    Buffer.BlockCopy(tempBuffer, 0, buffer, offset, copyByteCount);
                }

                return copyByteCount;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            base.Close();
            DestroyDevice();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DestroyDevice();
        }

        void DestroyDevice()
        {
            if (_device == IntPtr.Zero)
                return;
            lock (_readWriteLock)
            {
                if (_capturing)
                {
                    API.alcCaptureStop(_device);
                    _capturing = false;
                }
                API.alcCaptureCloseDevice(_device);
                _device = IntPtr.Zero;
            }
        }

        int GetSamplesAvailable()
        {
            if (_device == IntPtr.Zero)
                return 0;

            int samples;
            API.alcGetIntegerv(_device, ALCEnum.ALC_CAPTURE_SAMPLES, 4, out samples);
            //  todo: error checking
            return samples;
        }

        /// <summary>
        /// Copy samples into a buffer.
        /// </summary>
        /// <param name="buffer">Buffer to copy samples into.</param>
        /// <param name="samples">Number of samples to copy.</param>
        unsafe void ReadIntoBuffer(byte[] buffer, int samples)
        {
            fixed (byte* bbuff = buffer)
            {
                var buffPtr = new IntPtr((void*)bbuff);
                API.alcCaptureSamples(_device, buffPtr, samples);
            }
        }
    }
}
