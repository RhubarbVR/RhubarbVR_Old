using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.NetManager;

namespace RhubarbEngine.Managers
{
    public class NetManager : IManager
    {

        public Engine engine;

        public Server server;

        public List<Client> clients;

        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            engine.logger.Log("Starting Server");
            server = new Server(this);
            server.initialize();
            clients = new List<Client>();
            return this;
        }

        public void addClient(string _ip = "localhost", int _port = 9050, string _key = "gay")
        {
            engine.logger.Log("Starting Client");
            Client client = new Client(this);
            client.initialize(_ip, _port, _key);
            clients.Add(client);
        }

        public void Update()
        {
            server.update();
            foreach(Client client in clients){
                client.update();
            }
        }

        public void cleanup()
        {
            server.cleanup();
            foreach (Client client in clients)
            {
                client.cleanup();
            }
        }
    }
}
