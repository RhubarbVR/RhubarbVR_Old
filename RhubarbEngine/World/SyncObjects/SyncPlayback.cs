using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.Net;

namespace RhubarbEngine.World
{
	public class SyncPlayback : Sync<Playback>
	{
		public event Func<double> stateChange;

		public override Playback Defalut()
		{
			return new Playback { Speed = 1f, Looping = true, Offset = world.WorldTime };
		}

		public override void UpdatedValue()
		{
			ClipLength = stateChange?.Invoke() ?? double.NegativeInfinity;
		}

		public double ClipLength { get; set; } = double.NegativeInfinity;

		public double Position
		{
			get { return ProccessPosition(); }
			set
			{
				base.Value = new Playback { Looping = base.Value.Looping, Offset = world.WorldTime, Playing = base.Value.Playing, Speed = base.Value.Speed, Position = value };
			}
		}

		public bool Playing => (((RawPos() < ClipLength) || Looping) || Stream) && Value.Playing;

		public bool Stream { get { return ClipLength >= double.PositiveInfinity; } }

		public double ProccessPosition()
		{
			if (Value.Playing)
			{
				if (Stream)
				{
					return 1f;
				}
				if (Looping)
				{
					return RawPos() % ClipLength;
				}
				if (RawPos() > ClipLength)
				{
					return ClipLength;
				}
				else
				{
					return RawPos();
				}
			}
			return Value.Position;
		}

		public double RawPos()
		{
			return ((Value.Offset - world.WorldTime) * Speed) + Value.Position;
		}

		public override void LoadedFromBytes(bool networked)
		{
			base.LoadedFromBytes(networked);
			if (!networked)
			{
				Value = new Playback { Looping = Value.Looping, Offset = world.WorldTime, Playing = Value.Playing, Speed = Value.Speed, Position = Value.Position };
			}
		}
		public override Playback SaveToBytes(bool netsync)
		{
			return new Playback { Looping = Value.Looping, Offset = 0f, Playing = Value.Playing, Speed = Value.Speed, Position = RawPos() };

		}
		public bool Looping
		{
			get
			{
				return Value.Looping;
			}
			set
			{
				base.Value = new Playback { Looping = value, Offset = base.Value.Offset, Playing = true, Speed = base.Value.Speed, Position = base.Value.Position };
			}
		}
		public float Speed
		{
			get
			{
				return Value.Speed;
			}
			set
			{
				base.Value = new Playback { Looping = base.Value.Looping, Offset = base.Value.Offset, Playing = true, Speed = value, Position = base.Value.Position };
			}
		}

		public void Skip()
		{

		}

		public void Play()
		{
			Value = new Playback { Looping = Value.Looping, Offset = world.WorldTime, Playing = true, Speed = Value.Speed, Position = 0f };
		}
		public void Stop()
		{
			Value = new Playback { Looping = Value.Looping, Offset = world.WorldTime, Playing = false, Speed = Value.Speed, Position = 0f };
		}
		public void Pause()
		{
			Value = new Playback { Looping = Value.Looping, Offset = Value.Offset, Playing = false, Speed = Value.Speed, Position = Position };
		}
		public void Resume()
		{
			Value = new Playback { Looping = Value.Looping, Offset = Value.Offset, Playing = true, Speed = Value.Speed, Position = Value.Position };
		}
		public SyncPlayback(World _world, IWorldObject _parent, bool newref = true) : base(_world, _parent, newref)
		{

		}

		public SyncPlayback(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent, newref)
		{

		}
	}
}
