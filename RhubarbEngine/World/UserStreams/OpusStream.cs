using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;
using Concentus;
using Concentus.Common.CPlusPlus;
using Concentus.Enums;
using Concentus.Structs;
using System.Diagnostics;
using RhubarbEngine.Managers;

namespace RhubarbEngine.World
{

	public class CodecStatistics
	{
		private readonly double _avgDecay = 0.003;

		private double _averageBitrate = 0;

		public double Bitrate
		{
			get
			{
				return _averageBitrate;
			}
			set
			{
				_averageBitrate = (_averageBitrate * (1 - _avgDecay)) + (value * _avgDecay);
			}
		}

        public string Mode { get; set; } = "Hybrid";

        public int Bandwidth { get; set; } = 48000;

        private double _avgEncodeSpeed = 1;

		public double EncodeSpeed
		{
			get
			{
				return _avgEncodeSpeed;
			}
			set
			{
				_avgEncodeSpeed = (_avgEncodeSpeed * (1 - _avgDecay)) + (value * _avgDecay);
			}
		}

		private double _avgDecodeSpeed = 1;

		public double DecodeSpeed
		{
			get
			{
				return _avgDecodeSpeed;
			}
			set
			{
				_avgDecodeSpeed = (_avgDecodeSpeed * (1 - _avgDecay)) + (value * _avgDecay);
			}
		}
	}


	public interface IOpusCodec
	{
		void SetBitrate(int bitrate);
		void SetComplexity(int complexity);
		void SetPacketLoss(int loss);
		void SetApplication(OpusApplication application);
		void SetFrameSize(double frameSize);
		void SetVBRMode(bool vbr, bool constrained);
		byte[] Compress(byte[] input);
		byte[] Decompress(byte[] input);
		CodecStatistics GetStatistics();
	}

	public class OpusStream : UserStream, IWorldObject, ISyncMember, IOpusCodec
	{

		public Sync<int> bitrate;

		public Sync<int> complexity;

		public Sync<bool> vbr;

		public Sync<bool> cvbr;

		public Sync<int> channels;
        private OpusEncoder _encoder;
		private OpusDecoder _decoder;
		private readonly CodecStatistics _statistics = new CodecStatistics();
		private readonly Stopwatch _timer = new Stopwatch();

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
            bitrate = new Sync<int>(this, newRefIds)
            {
                Value = 64
            };
            complexity = new Sync<int>(this, newRefIds)
            {
                Value = 5
            };
            vbr = new Sync<bool>(this, newRefIds)
            {
                Value = false
            };
            cvbr = new Sync<bool>(this, newRefIds)
            {
                Value = false
            };
            channels = new Sync<int>(this, newRefIds)
            {
                Value = 1
            };
            channels.Changed += Channels_Changed;
			cvbr.Changed += Channels_Changed;
			vbr.Changed += Channels_Changed;
			complexity.Changed += Channels_Changed;
			bitrate.Changed += Channels_Changed;
		}

		private void Channels_Changed(IChangeable obj)
		{
			Update();
		}

		public void Update()
		{
			_encoder = OpusEncoder.Create(Engine.audioManager.SamplingRate, channels.Value, OpusApplication.OPUS_APPLICATION_AUDIO);
			_encoder.Bitrate = (bitrate.Value * 1024);
			_encoder.Complexity = (complexity.Value);
			_encoder.UseVBR = vbr.Value;
			_encoder.UseConstrainedVBR = cvbr.Value;
			_encoder.EnableAnalysis = true;
			_decoder = OpusDecoder.Create(48000, 1);
		}
		public CodecStatistics GetStatistics()
		{
			return _statistics;
		}

		private int GetFrameSize()
		{
			return Engine.audioManager.AudioFrameSizeInBytes;
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


        void ISyncMember.ReceiveData(DataNodeGroup data, Peer peer)
		{

		}

		public void SetBitrate(int bitrate)
		{

		}

		public void SetComplexity(int complexity)
		{

		}

		public void SetPacketLoss(int loss)
		{

		}

		public void SetApplication(OpusApplication application)
		{

		}

		public void SetFrameSize(double frameSize)
		{

		}

		public void SetVBRMode(bool vbr, bool constrained)
		{

		}

		public byte[] Compress(byte[] input)
		{
			return new byte[] { 0 };
		}

		public byte[] Decompress(byte[] input)
		{
			return new byte[] { 0 };
		}
	}
}
