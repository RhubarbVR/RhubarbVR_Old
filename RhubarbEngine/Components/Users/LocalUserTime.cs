using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using System.Numerics;

namespace RhubarbEngine.Components.Transform
{
    [Category(new string[] { "Users" })]
    public class LocalUserTime : Component
    {
        public Sync<DateTime> currentTime;

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            currentTime = world.worldManager.engine.platformInfo.Frame;
            // currentTime = DateTime.UtcNow;
        }

        public LocalUserTime(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public LocalUserTime()
        {
        }
    }
}