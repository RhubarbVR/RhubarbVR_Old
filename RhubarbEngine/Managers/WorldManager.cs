using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;

namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        private Engine engine;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            return this;
        }
    }
}
