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
           

        public Renderable(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Renderable()
        {
        }

        public void Dispose()
        {
            DestroyDeviceObjects();
        }
    }

}
