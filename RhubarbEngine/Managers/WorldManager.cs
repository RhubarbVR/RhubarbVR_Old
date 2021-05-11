using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.World;

namespace RhubarbEngine.Managers
{
    public class WorldManager : IManager
    {
        public Engine engine;

        public List<World.World> worlds = new List<World.World>();

        public World.World privateOverlay;

        public World.World localWorld;

        public World.World focusedWorld;



        public IManager initialize(Engine _engine)
        {
            engine = _engine;

            engine.logger.Log("Starting Private Overlay World");
            privateOverlay = new World.World(this, "Private Overlay", 1);
            privateOverlay.Focus = World.World.FocusLevel.PrivateOverlay;
            worlds.Add(privateOverlay);

            engine.logger.Log("Starting Local World");
            localWorld = new World.World(this,"LoaclWorld",16);
            localWorld.Focus = World.World.FocusLevel.Focused;
            worlds.Add(privateOverlay);

            return this;
        }

        public void Update(DateTime startTime, DateTime Frame)
        {
            foreach(World.World world in worlds)
            {
                world.Update(startTime, Frame);
            }
        }
    }
}
