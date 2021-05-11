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
    public class Client
    {
        public Managers.NetManager netManager;

        public EventBasedNetListener listener;

        public LiteNetLib.NetManager client;

        public void initialize(string _ip = "localhost", int _port = 9050, string _key = "")
        {
            listener = new EventBasedNetListener();
            client = new LiteNetLib.NetManager(listener);
            client.Start();
            client.Connect(_ip, _port, _key);
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                netManager.engine.logger.Log("We got: {0}" + dataReader.GetString(100));
                dataReader.Recycle();
            };
        }

        public void update()
        {
            client.PollEvents();
        }
        public void cleanup()
        {
            client.Stop();
        }

        public Client(Managers.NetManager _netManager)
        {
            netManager = _netManager;
        }
    }
}
