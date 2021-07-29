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
        public NUllNetModule(World world) : base(world)
        {

        }
    }

    public abstract class NetModule
    {
        public World _world;

        public NetModule(World world)
        {
            _world = world;
        }

        public List<NetData> NetQueue = new List<NetData>();

        public virtual void DropQ()
        {
            List<ulong> trains = new List<ulong>();
            List<NetData> netData = new List<NetData>();
            foreach (var item in NetQueue)
            {
                DataNodeGroup node = new DataNodeGroup();
                node.setValue("data", item.data);
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
            var netdata = new NetData(_reliabilityLevel, _data, _id);
            NetQueue.Add(netdata);
        }
    }
}
