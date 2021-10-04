using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;

using Valve.Sockets;

namespace RhubarbEngine.World.Net
{
	public class SteamPeer : Peer
	{
		public SteamNetModule netModule;
        public NetworkingSockets client;
        public bool IsClientConnection;
        public uint clientConnected;
		public SteamPeer(SteamNetModule _netModule,uint client)
		{
			netModule = _netModule;
            clientConnected = client;
        }

        public SteamPeer(SteamNetModule _netModule, Address address)
        {
            netModule = _netModule;
            client = new NetworkingSockets();
            clientConnected = client.Connect(ref address);
            IsClientConnection = true;
            Send(netModule.ConectionReqweset(), ReliabilityLevel.Reliable);
        }
        public override void Send(byte[] val, ReliabilityLevel reliableOrdered)
		{
            if (IsClientConnection)
            {
                switch (reliableOrdered)
                {
                    case ReliabilityLevel.Unreliable:
                        client.SendMessageToConnection(clientConnected, val, SendFlags.Unreliable);
                        break;
                    case ReliabilityLevel.LatestOnly:
                        client.SendMessageToConnection(clientConnected, val, SendFlags.NoNagle);
                        break;
                    case ReliabilityLevel.Reliable:
                        client.SendMessageToConnection(clientConnected, val, SendFlags.Reliable);
                        break;
                    default:
                        client.SendMessageToConnection(clientConnected, val, SendFlags.NoDelay);
                        break;
                }
            }
            else
            {
                switch (reliableOrdered)
                {
                    case ReliabilityLevel.Unreliable:
                        netModule.server.SendMessageToConnection(clientConnected, val,SendFlags.Unreliable);
                        break;
                    case ReliabilityLevel.LatestOnly:
                        netModule.server.SendMessageToConnection(clientConnected, val, SendFlags.NoNagle);
                        break;
                    case ReliabilityLevel.Reliable:
                        netModule.server.SendMessageToConnection(clientConnected, val, SendFlags.Reliable);
                        break;
                    default:
                        netModule.server.SendMessageToConnection(clientConnected, val, SendFlags.NoDelay);
                        break;
                }
            }
		}
	}


    public class SteamNetModule : NetModule
    {
        public NetworkingSockets server = new();

        const int MAX_MESSAGES = 20;
        public NetworkingMessage[] netMessages = new NetworkingMessage[MAX_MESSAGES];
        public uint pollGroup;
        public override void Connect(string token)
		{
            var address = new Address();
            var colonIndex = token.IndexOf(':');
            var host = token.Substring(0, colonIndex);
            var port = token.Substring(colonIndex + 1);
            address.SetAddress(host, ushort.Parse(port));
            rhuPeers.Add(new SteamPeer(this, address));
            pollGroup = server.CreatePollGroup();
        }


        public override IReadOnlyList<Peer> Peers { get { return rhuPeers; } }

		public List<SteamPeer> rhuPeers = new();

		public SteamNetModule(World world) : base(world)
		{
			Console.WriteLine("Starting net");
            var address = new Address();
            address.SetAddress("::0", 5271);
            server.CreateListenSocket(ref address);
            
        }

        public SteamPeer GetPearFromClientID(uint id)
        {
            foreach (var item in rhuPeers)
            {
                if (!item.IsClientConnection)
                {
                    if(item.clientConnected == id)
                    {
                        return item;
                    }
                }
            }
            return null;
        }


        public unsafe override void Netupdate()
        {
            base.Netupdate();
            server.RunCallbacks();
            var netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, MAX_MESSAGES);

            if (netMessagesCount > 0)
            {
                for (var i = 0; i < netMessagesCount; i++)
                {
                    ref var netMessage = ref netMessages[i];
                    var peer = GetPearFromClientID(netMessage.connection);
                    if(peer is null)
                    {
                        peer = new SteamPeer(this, netMessage.connection);
                        rhuPeers.Add(peer);
                    }
                    _world.NetworkReceiveEvent(new Span<byte>(netMessage.data.ToPointer(), netMessage.length).ToArray(), peer);
                    netMessage.Destroy();
                }
            }
        }

        public override unsafe void SendData(DataNodeGroup node, NetData item)
		{
            var data = node.GetByteArray();
            SendToAll(data, item.reliabilityLevel);
		}

        public void SendToAll(byte[] data, ReliabilityLevel reliableOrdered)
        {
            foreach (var item in rhuPeers)
            {
                item.Send(data, reliableOrdered);
            }
        }

        public byte[] ConectionReqweset()
        {
            var req = new DataNodeGroup();
            return req.GetByteArray();
        }

        public override void Dispose()
        {
        }

    }
}
