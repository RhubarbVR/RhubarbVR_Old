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
		public override string token => "NULL";

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

		private bool _noq;

		public virtual string token { get; }
		public virtual IReadOnlyList<Peer> peers { get; }


		public NetModule(World world, bool noq = false)
		{
			_world = world;
			_noq = noq;
		}

		public List<NetData> NetQueue = new List<NetData>();

		public virtual void DropQ()
		{
			if (_noq)
			{ NetQueue.Clear(); return; };
			List<ulong> trains = new List<ulong>();
			List<NetData> netData = new List<NetData>();
			foreach (var item in NetQueue)
			{
				DataNodeGroup node = new DataNodeGroup();
				node.setValue("data", item.data);
				node.setValue("level", new DataNode<int>((int)item.reliabilityLevel));
				node.setValue("id", new DataNode<ulong>(item.id));
				if (item.reliabilityLevel == ReliabilityLevel.LatestOnly || item.reliabilityLevel == ReliabilityLevel.Unreliable)
				{
					if (!trains.Contains(item.id))
					{
						trains.Add(item.id);
						sendData(node, item);
					}
				}
				else
				{
					sendData(node, item);
				}
				netData.Add(item);
			}
			foreach (var item in netData)
			{
				NetQueue.Remove(item);
			}
		}


		public virtual void netupdate()
		{
			if (NetQueue.Count != 0)
			{
				DropQ();
			}
		}

		public virtual void sendData(DataNodeGroup node, NetData item)
		{

		}

		public void addToQueue(ReliabilityLevel _reliabilityLevel, DataNodeGroup _data, ulong _id)
		{
			if (_noq)
				return;
			var netdata = new NetData(_reliabilityLevel, _data, _id);
			NetQueue.Add(netdata);
		}
	}
}
