using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;

namespace RhubarbEngine.Render
{
    public abstract class RenderObject : Component, IDisposable
    {

        public override void LoadToWorld()
        {
            world.RenderObjects.Add(this);
        }
        public RenderObject(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public RenderObject()
        {
        }

        public void Dispose()
        {
            try
            {
                world.RenderObjects.Remove(this);
            }
            catch {

            }
        }
    }

}
