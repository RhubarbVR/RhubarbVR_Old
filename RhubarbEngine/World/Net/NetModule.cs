using RhubarbEngine.World.DataStructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Net
{

	public class NUllNetModule : NetModule
	{
        public override string Token
        {
            get
            {
                return "NULL";
            }
        }

        public NUllNetModule(World world) : base(world, true)
		{

		}
	}

	public abstract class NetModule
	{
		public virtual void Connect(string token)
		{

		}


		public World _world;

		private readonly bool _noq;

		public virtual string Token { get; }
		public virtual IEnumerable<Peer> Peers { get; }


		public NetModule(World world, bool noq = false)
		{
			_world = world;
			_noq = noq;
		}

		public List<NetData> NetQueue = new();

		public virtual void DropQ()
		{
			if (_noq)
			{ NetQueue.Clear(); return; };
			var trains = new List<ulong>();
			var netData = new List<NetData>();
			foreach (var item in NetQueue)
			{
				var node = new DataNodeGroup();
				node.SetValue("data", item.data);
				node.SetValue("level", new DataNode<int>((int)item.reliabilityLevel));
				node.SetValue("id", new DataNode<ulong>(item.id));
				if (item.reliabilityLevel is ReliabilityLevel.LatestOnly or ReliabilityLevel.Unreliable)
				{
					if (!trains.Contains(item.id))
					{
						trains.Add(item.id);
						SendData(node, item);
					}
				}
				else
				{
					SendData(node, item);
				}
				netData.Add(item);
			}
			foreach (var item in netData)
			{
				NetQueue.Remove(item);
			}
		}


		public virtual void Netupdate()
		{
			if (NetQueue.Count != 0)
			{
				DropQ();
			}
		}

		public virtual void SendData(DataNodeGroup node, NetData item)
		{

		}

		public void AddToQueue(ReliabilityLevel _reliabilityLevel, DataNodeGroup _data, ulong _id)
		{
			if (_noq)
            {
                return;
            }

            var netdata = new NetData(_reliabilityLevel, _data, _id);
			NetQueue.Add(netdata);
		}
	}
}
