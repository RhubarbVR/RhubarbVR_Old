using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;
using RhubarbEngine.Components.Audio;
using System.Diagnostics;
using RhubarbEngine.Managers;
using OpusDotNet;
using System.Threading;

namespace RhubarbEngine.World
{

	public class OpusStream : UserStream, IWorldObject, ISyncMember, IAudioSource
	{
        public Sync<int> frequency;

        public Sync<int> device;

        public Sync<bool> stereo;

        public Sync<Application> type;

        private OpusDecoder _opusDecoder;

        private OpusEncoder _opusEncoder;

        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);
            frequency = new Sync<int>(this, newRefIds)
            {
                Value = 48000
            };
            stereo = new Sync<bool>(this, newRefIds);
            type = new Sync<Application>(this, newRefIds)
            {
                Value = Application.VoIP
            };
            device = new Sync<int>(this, newRefIds)
            {
                Value = -1
            };
            device.Changed += ValChanged;
            frequency.Changed += ValChanged;
            stereo.Changed += ValChanged;
            type.Changed += ValChanged;

        }

        public override void OnLoaded()
        {
            base.OnLoaded();
            ReLoad();
        }

        private void ValChanged(IChangeable obj)
        {
            ReLoad();
        }

        private void UnLoad()
        {
            if(_opusDecoder is not null)
            {
                _opusDecoder = null;
            }
            if (_opusEncoder is not null)
            {
                _opusEncoder = null;
            }
        }

        private void ReLoad()
        {
            Console.WriteLine("Opus Reload");
            UnLoad();
            if(this.parent.Parent == World.LocalUser)
            {
                Console.WriteLine("Opus Encode");
                LoadEncoder();
            }
            else
            {
                Console.WriteLine("Opus Decode");
                LoadDecoder();
            }
            Reload?.Invoke();
        }

        public override void ReceiveData(DataNodeGroup data, Peer peer)
        {
            if(_opusDecoder is null)
            {
                return;
            }
            var CompresedData = ((DataNode<byte[]>)data.GetValue("Data")).Value;
            _opusDecoder.Decode(CompresedData, CompresedData.Length, _readBuffer, _readBuffer.Length);
            Update?.Invoke();
        }

        private byte[] _readBuffer;

        private OpenAL.CaptureStream _captureStream;

        public event Action Update;
        public event Action Reload;

        public bool IsActive
        {
            get
            {
                return (_opusDecoder is not null) || (_opusEncoder is not null);
            }
        }

        public int ChannelCount
        {
            get
            {
                return (stereo.Value) ? 2 : 1;
            }
        }

        public byte[] FrameInputBuffer
        {
            get
            {
                return _readBuffer;
            }
        }

        private void LoadEncoder()
        {
            try
            {
                _opusEncoder = new OpusEncoder(type.Value, frequency.Value, (stereo.Value) ? 2:1);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Loading OpuseEncoder " + e.ToString());
            }
            Console.WriteLine("Starting Devices:");
            try
            {
                if (device.Value == -1)
                {
                    _captureStream = Engine.AudioManager.CapDevice.OpenStream(frequency.Value, (stereo.Value) ? OpenAL.OpenALAudioFormat.Mono16Bit : OpenAL.OpenALAudioFormat.Stereo16Bit, 10);
                }
                else
                {
                    _captureStream = OpenAL.OpenALHelper.CaptureDevices[device.Value].OpenStream(frequency.Value, (stereo.Value) ? OpenAL.OpenALAudioFormat.Mono16Bit : OpenAL.OpenALAudioFormat.Stereo16Bit, 10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start Devices:" + e.ToString());
            }
            _readBuffer = new byte[10 * frequency.Value * sizeof(short)];
            Console.WriteLine("Start listener");
            try
            {
                Task.Run(Loop);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to start lister erroe:" + e.ToString());
            }
        }

        private void Loop()
        {
            while(_captureStream is not null)
            {
                try
                {
                    if (!_captureStream.CanRead)
                    {
                        return;
                    }

                    _captureStream.Read(_readBuffer);
                    Update?.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error " + e.ToString());
                }
                Thread.Sleep(10);
            }
        }

        private void LoadDecoder()
        {
            _readBuffer = new byte[10 * frequency.Value * sizeof(short)];
            _opusDecoder = new OpusDecoder(frequency.Value, (stereo.Value) ? 2 : 1);
        }

        public OpusStream()
		{

		}

		public OpusStream(World _world, IWorldObject _parent, bool newref = true) : base(_world, _parent, newref)
		{

		}

		public OpusStream(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent, newref)
		{

		}
	}
}
