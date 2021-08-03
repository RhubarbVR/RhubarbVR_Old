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

        public override Playback defalut()
        {
            return new Playback{Speed = 1f,Looping = true,Offset = world.worldTime};
        }

        public override void UpdatedValue()
        {
            ClipLength = stateChange?.Invoke()?? double.NegativeInfinity;
        }

        public double ClipLength { get; set; } = double.NegativeInfinity;

        public double Position { get { return ProccessPosition(); } set
            {
                base.value = new Playback { Looping = base.value.Looping, Offset = world.worldTime, Playing = base.value.Playing, Speed = base.value.Speed, Position = value };
            }
        }

        public bool Playing => (((RawPos() < ClipLength)||Looping) || Stream) && value.Playing;

        public bool Stream { get { return ClipLength >= double.PositiveInfinity; } }

        public double ProccessPosition()
        {
            if (value.Playing)
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
            return value.Position;
        }

        public double RawPos()
        {
            return ((value.Offset - world.worldTime)*Speed) + value.Position;
        }

        public override void LoadedFromBytes(bool networked)
        {
            base.LoadedFromBytes(networked);
            if (!networked)
            {
                value = new Playback { Looping = value.Looping, Offset = world.worldTime, Playing = value.Playing, Speed = value.Speed, Position = value.Position };
            }
        }
        public override Playback SaveToBytes(bool netsync)
        {
            return new Playback { Looping = value.Looping, Offset = 0f, Playing = value.Playing, Speed = value.Speed, Position = RawPos() };

        }
        public bool Looping
        {
            get
            {
                return value.Looping;
            }
            set
            {
                base.value = new Playback { Looping = value, Offset = base.value.Offset, Playing = true, Speed = base.value.Speed, Position = base.value.Position };
            }
        }
        public float Speed
        {
            get
            {
                return value.Speed;
            }
            set
            {
                base.value = new Playback { Looping = base.value.Looping, Offset = base.value.Offset, Playing = true, Speed = value, Position = base.value.Position };
            }
        }

        public void Skip()
        {

        }

        public void Play()
        {
            value = new Playback { Looping = value.Looping, Offset = world.worldTime, Playing = true, Speed = value.Speed, Position = 0f };
        }
        public void Stop()
        {
            value = new Playback { Looping = value.Looping, Offset = world.worldTime, Playing = false, Speed = value.Speed, Position = 0f };
        }
        public void Pause()
        {
            value = new Playback { Looping = value.Looping, Offset = value.Offset, Playing = false, Speed = value.Speed, Position = Position };
        }
        public void Resume()
        {
            value = new Playback { Looping = value.Looping, Offset = value.Offset, Playing = true, Speed = value.Speed, Position = value.Position };
        }
        public SyncPlayback(World _world, IWorldObject _parent,bool newref = true) : base(_world, _parent, newref)
        {

        }

        public SyncPlayback(IWorldObject _parent, bool newref = true) : base(_parent.World, _parent,newref)
        {

        }
    }
}
