using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Input;
using Veldrid;
using RhubarbEngine.VirtualReality;

namespace RhubarbEngine.Managers
{
    public class InputManager : IManager
    {
        private Engine engine;

        public InputTracker mainWindows = new InputTracker();
        public IManager initialize(Engine _engine)
        {
            engine = _engine;
            return this;
        }

        public void Update()
        {
            if (mainWindows.GetKeyDown(Key.F8))
            {
                if (engine.outputType == OutputType.Screen)
                {
                    engine.renderManager.switchVRContext(OutputType.SteamVR);
                }
                else
                {
                    if (engine.outputType == OutputType.SteamVR)
                    {
                        engine.renderManager.switchVRContext(OutputType.Screen);
                    }
                }
            }
        }
    }
}
