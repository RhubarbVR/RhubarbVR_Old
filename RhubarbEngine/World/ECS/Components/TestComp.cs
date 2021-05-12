using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.ECS.Components
{
    public class TestComp: Component
    {
        public Sync<string> val;
        public override void buildSyncObjs(bool newRefIds)
        {
            val = new Sync<string>(this, newRefIds);
        }


        public override void onLoaded()
        {
            if (val.value == null)
            {
                world.worldManager.engine.logger.Log("Set Value");
                val.value = "Trains is the value";
            }
            world.worldManager.engine.logger.Log("Value is " + val.value);
        }
    }
}
