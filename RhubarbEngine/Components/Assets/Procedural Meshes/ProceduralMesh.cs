using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
    public abstract class ProceduralMesh : AssetProvider<RMesh>
    {
        public ProceduralMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ProceduralMesh()
        {
        }
    }
}
