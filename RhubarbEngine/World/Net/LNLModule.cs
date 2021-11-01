using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net.Sockets;
using System.Net.Http;
using System.Diagnostics;

namespace RhubarbEngine.World.Net
{
	public class LNLPeer : Peer
	{
		public LNLNetModule rhuNetModule;
		public IPEndPoint endPoint;
        public NetPeer netPeer;
		public LNLPeer(LNLNetModule _rhuNetModule, NetPeer peer)
		{
			rhuNetModule = _rhuNetModule;
            netPeer = peer;
        }
		public override void Send(byte[] val, ReliabilityLevel reliableOrdered)
		{

            switch (reliableOrdered)
            {
                case ReliabilityLevel.Unreliable:
                    netPeer.Send(val, DeliveryMethod.Unreliable);
                    break;
                case ReliabilityLevel.LatestOnly:
                    netPeer.Send(val, DeliveryMethod.ReliableUnordered);
                    break;
                case ReliabilityLevel.Reliable:
                    netPeer.Send(val, DeliveryMethod.ReliableOrdered);
                    break;
                default:
                    netPeer.Send(val, DeliveryMethod.Sequenced);
                    break;
            }
		}
        public int latency;
    }

	public class LNLNetModule : NetModule , INetEventListener,INatPunchListener
	{
        public NetManager netClient;

        public override void Connect(string token)
		{
            netClient.Connect("", 54, "RhubarbVR");
        }

        public override IEnumerable<Peer> Peers { get { return rhuPeers.Cast<Peer>(); } }

		public SynchronizedCollection<LNLPeer> rhuPeers = new();

        public static IPEndPoint MainPunchServer
        {
            get
            {
                return new IPEndPoint(IPAddress.Parse("5.135.157.47"), 50010);
                //return new IPEndPoint(IPAddress.Loopback, 50010);
            }
        }

        public LNLNetModule(World world, string sessionID) : base(world,false)
		{
			Console.WriteLine("Starting net");
            netClient = new NetManager(this)
            {
                IPv6Enabled = IPv6Mode.DualMode,
                NatPunchEnabled = true
            };
            netClient.NatPunchModule.Init(this);
            netClient.Start();
            hartBeeter.Start();
            Int(sessionID).ConfigureAwait(false);
        }

        private string _sessionToken;

        private async Task Int(string id)
        {
            _world.worldManager.Engine.Logger.Log("tried to start");
            _sessionToken = await _world.worldManager.Engine.NetApiManager.JoinSession(id);
            _world.worldManager.Engine.Logger.Log("Token: " + _sessionToken);
            netClient.NatPunchModule.SendNatIntroduceRequest(MainPunchServer, _sessionToken);
            _world.worldManager.Engine.Logger.Log("into");
        }

        public override void SendData(DataNodeGroup node, NetData item)
		{
            switch (item.reliabilityLevel)
            {
                case ReliabilityLevel.Unreliable:
                    netClient.SendToAll(node.GetByteArray(), DeliveryMethod.Sequenced);
                    break;
                case ReliabilityLevel.LatestOnly:
                    netClient.SendToAll(node.GetByteArray(),DeliveryMethod.ReliableSequenced);
                    break;
                case ReliabilityLevel.Reliable:
                    netClient.SendToAll(node.GetByteArray(), DeliveryMethod.ReliableOrdered);
                    break;
                default:
                    netClient.SendToAll(node.GetByteArray(), DeliveryMethod.Sequenced);
                    break;
            }
		}

        public LNLPeer GetLNLPeerFromNetPeer(NetPeer peer)
        {
            var lnlpeer = (LNLPeer)peer.Tag;
            if(lnlpeer is not null)
            {
                return lnlpeer;
            }
            foreach (var item in rhuPeers)
            {
                if(item.netPeer == peer)
                {
                    return item;
                }
            }
            return null;
        }

        public void OnPeerConnected(NetPeer peer)
        {
            _world.worldManager.Engine.Logger.Log("UserConnected");
            var cpeer = new LNLPeer(this, peer);
            peer.Tag = cpeer;
            rhuPeers.Add(cpeer);
            _world.PeerConnectedEvent(cpeer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var LNLpeer = GetLNLPeerFromNetPeer(peer);
            if (LNLpeer is null)
            {
                return;
            }
            rhuPeers.Remove(LNLpeer);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _world.worldManager.Engine.Logger.Log("Connection Error"+socketError.ToString(), true);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if((peer.ConnectionState is not ConnectionState.Connected) || peer.Tag is null)
            {
                return;
            }
            if(peer.Tag.GetType() != typeof(LNLPeer))
            {
                return;
            }
            var reliabilityLevel = deliveryMethod switch
            {
                DeliveryMethod.Unreliable => ReliabilityLevel.Unreliable,
                DeliveryMethod.ReliableUnordered => ReliabilityLevel.LatestOnly,
                DeliveryMethod.Sequenced => ReliabilityLevel.Reliable,
                DeliveryMethod.ReliableOrdered => ReliabilityLevel.Reliable,
                DeliveryMethod.ReliableSequenced => ReliabilityLevel.Reliable,
                _ => ReliabilityLevel.Unreliable,
            };
            _world.NetworkReceiveEvent(reader.GetRemainingBytes(), (LNLPeer)peer.Tag, reliabilityLevel);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            GetLNLPeerFromNetPeer(peer).latency = latency;
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            _world.worldManager.Engine.Logger.Log("Request: " + request.Data.GetString(), true);
            request.Accept();
        }

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            _world.worldManager.Engine.Logger.Log("Nat Introduction Request. LocalEndpoint: " + localEndPoint?.ToString() + ", Remote Endpoint: " + remoteEndPoint?.ToString() + ", token: " + token,true);
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            netClient.Connect(targetEndPoint, token);
            _world.worldManager.Engine.Logger.Log("NatIntroductionSuccess");
        }

        public readonly Stopwatch hartBeeter = new();

        private async Task SendHartBeet()
        {
            await _world.worldManager.Engine.NetApiManager.UpdateSession(_sessionToken);
        }

        public override void Netupdate()
        {
            base.Netupdate();
            netClient.PollEvents();
            netClient.NatPunchModule.PollEvents();
            if (hartBeeter.ElapsedMilliseconds >= 45000)
            {
                hartBeeter.Restart();
                SendHartBeet().ConfigureAwait(false);
            }
        }

        public override void Dispose()
        {
            _world.worldManager.Engine.NetApiManager.RemoveSession(_sessionToken).ConfigureAwait(false);
            base.Dispose();
        }
    }
}
