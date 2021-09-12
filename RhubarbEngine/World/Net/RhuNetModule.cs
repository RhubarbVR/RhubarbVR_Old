using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using RhubarbEngine.World.DataStructure;

using RhuNet;

using RhuNetShared;

namespace RhubarbEngine.World.Net
{
	public class RhuPeer : Peer
	{
		public RhuNetModule rhuNetModule;
		public IPEndPoint endPoint;

		public ClientInfo clientInfo;

		public RhuPeer(RhuNetModule _rhuNetModule, IPEndPoint _endPoint, ClientInfo _clientInfo)
		{
			rhuNetModule = _rhuNetModule;
			endPoint = _endPoint;
			clientInfo = _clientInfo;
		}
		public override void Send(byte[] val, ReliabilityLevel reliableOrdered)
		{
			switch (reliableOrdered)
			{
				case ReliabilityLevel.Unreliable:
					rhuNetModule.rhuClient.SendMessageUDP(new Data(val), endPoint);
					break;
				default:
					rhuNetModule.rhuClient.SendMessageUDP(new Data(val), endPoint);
					break;
			}
		}
	}

	public class RhuNetModule : NetModule
	{

		public override void Connect(string token)
		{
			rhuClient.ConnectToToken(token);
		}

		public RhuClient rhuClient;

        public override string Token
        {
            get
            {
                return rhuClient.Token;
            }
        }

        public override IReadOnlyList<Peer> Peers { get { return rhuPeers; } }

		public List<RhuPeer> rhuPeers = new List<RhuPeer>();

		public RhuNetModule(World world) : base(world)
		{
			Console.WriteLine("Starting net");
			rhuClient = new RhuClient("5.135.157.47", 50, "User UUID");
			rhuClient.OnMessageReceived += Thing;
			rhuClient.OnClientAdded += Trains;
			rhuClient.OnResultsUpdate += RhuClient_OnResultsUpdate;
			rhuClient.OnClientConnection += RhuClient_OnClientConnection;
			rhuClient.dataRecived += RhuClient_dataRecived;
			rhuClient.ConnectOrDisconnect();
		}

		private void RhuClient_dataRecived(Data arg1, IPEndPoint arg2)
		{
			_world.NetworkReceiveEvent(arg1.data, GetPeerFromEndPoint(arg2));
		}

		private RhuPeer GetPeerFromEndPoint(IPEndPoint e)
		{
			foreach (var item in rhuPeers)
			{
				if (item.endPoint == e)
				{
					return item;
				}
			}
			return null;
		}

		private void RhuClient_OnClientConnection(object sender, System.Net.IPEndPoint e)
		{
			if (((ClientInfo)sender).ID == rhuClient.LocalClientInfo.ID)
            {
                return;
            }

            var p = new RhuPeer(this, e, (ClientInfo)sender);
			rhuPeers.Add(p);
			_world.PeerConnectedEvent(p);
		}

		private void RhuClient_OnResultsUpdate(object sender, string e)
		{
			Console.WriteLine(e);
		}

		private void Trains(object sender, ClientInfo e)
		{
			Console.WriteLine(e.Name);
		}

		private void Thing(object sender, MessageReceivedEventArgs e)
		{
			Console.WriteLine(e.message.Content);
		}

		public void UdpToAll(IP2PBase p2PBase)
		{
			foreach (var item in rhuPeers)
			{
				rhuClient.SendMessageUDP(p2PBase, item.endPoint);
			}
		}

		public override void SendData(DataNodeGroup node, NetData item)
		{
			switch (item.reliabilityLevel)
			{
				case ReliabilityLevel.Unreliable:
					UdpToAll(new Data(node.getByteArray()));
					break;
				default:
					UdpToAll(new Data(node.getByteArray()));
					break;
			}
		}

	}
}
