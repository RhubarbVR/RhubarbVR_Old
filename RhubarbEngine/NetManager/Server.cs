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

        public int port { get; private set; } 

        public void initialize(int _port = 9050, string _key = "gay")
        {
            listener = new EventBasedNetListener();
            server = new LiteNetLib.NetManager(listener);

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
        private void startServer(int _port = 9050)
        {
            try
            {
                server.Start(_port);
                port = _port;
            }
            catch
            {
                if (_port <= 9090)
                {
                    startServer(_port + 1);
                }
                else
                {
                    throw;
                }
            }
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
