using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using RhubarbEngine.Managers;

namespace RhubarbEngine.NetManager
{
    public class Server
    {

        public Managers.NetManager netManager;

        public EventBasedNetListener listener;

        public LiteNetLib.NetManager server;

        public void initialize(int _port = 9050, string _key = "gay")
        {
            listener = new EventBasedNetListener();
            server = new LiteNetLib.NetManager(listener);
            server.Start(_port);
            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10 /* max connections */)
                {

                    netManager.engine.logger.Log("connection");
                    request.AcceptIfKey(_key);
                }
                else
                {
                    request.Reject();
                }
            };
            listener.PeerConnectedEvent += peer =>
            {
                netManager.engine.logger.Log("We got connection");
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };
        }

        public void update()
        {
            server.PollEvents();
        }
        public void cleanup()
        {
            server.Stop();
        }

        public Server(Managers.NetManager _netManager)
        {
            netManager = _netManager;
        }

    }
}
