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
		private double _avgDecay = 0.003;

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

		private string _mode = "Hybrid";

		public string Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				_mode = value;
			}
		}

		private int _bandwidth = 48000;

		public int Bandwidth
		{
			get
			{
				return _bandwidth;
			}
			set
			{
				_bandwidth = value;
			}
		}

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

		private int _packetLoss = 0;

		private OpusEncoder _encoder;
		private OpusDecoder _decoder;
		private CodecStatistics _statistics = new CodecStatistics();
		private Stopwatch _timer = new Stopwatch();

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			bitrate = new Sync<int>(this, newRefIds);
			bitrate.value = 64;
			complexity = new Sync<int>(this, newRefIds);
			complexity.value = 5;
			vbr = new Sync<bool>(this, newRefIds);
			vbr.value = false;
			cvbr = new Sync<bool>(this, newRefIds);
			cvbr.value = false;
			channels = new Sync<int>(this, newRefIds);
			channels.value = 1;
			channels.Changed += Channels_Changed;
			cvbr.Changed += Channels_Changed;
			vbr.Changed += Channels_Changed;
			complexity.Changed += Channels_Changed;
			bitrate.Changed += Channels_Changed;
		}

		private void Channels_Changed(IChangeable obj)
		{
			update();
		}

		public void update()
		{
			_encoder = OpusEncoder.Create(engine.audioManager.SamplingRate, channels.value, OpusApplication.OPUS_APPLICATION_AUDIO);
			_encoder.Bitrate = (bitrate.value * 1024);
			_encoder.Complexity = (complexity.value);
			_encoder.UseVBR = vbr.value;
			_encoder.UseConstrainedVBR = cvbr.value;
			_encoder.EnableAnalysis = true;
			_decoder = OpusDecoder.Create(48000, 1);
		}
		public CodecStatistics GetStatistics()
		{
			return _statistics;
		}

		private int GetFrameSize()
		{
			return engine.audioManager.AudioFrameSizeInBytes;
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


		public void ReceiveData(DataNodeGroup data, Peer peer)
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
