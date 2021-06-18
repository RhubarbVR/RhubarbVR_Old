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
using RhubarbEngine.Input;

namespace RhubarbEngine.Components.Users
{
    [Category(new string[] { "Users" })]
    public class Head : Component
    {

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            Matrix4x4 val = engine.renderManager.vrContext.Headpos;
            entity.setLocalTrans(val);
        }

        public override void buildSyncObjs(bool newRefIds)
        {
        }

        public Head(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public Head()
        {
        }
    }
}