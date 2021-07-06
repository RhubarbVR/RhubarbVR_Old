using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbCloudApi;
namespace RhubarbEngine.Managers
{
    public class NetApiManager : IManager
    {
        private Engine engine;

        public CloudInterface cloudInterface;

        public string token;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            engine.logger.Log("Starting Cloud Interface");
            cloudInterface = new CloudInterface(_engine.dataPath, token);
            return this;
        }

        public void Update()
        {

        }
    }
}
