using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Managers
{
    public class InputManager : IManager
    {
        private Engine engine;
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            return this;
        }

        public void Update()
        {

        }
    }
}
