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
using RhubarbEngine.World.Asset;
using g3;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Components.Assets
{
    [Category(new string[] { "Assets" })]
    public class RMaterial : AssetProvider<RMaterial>,IAsset
    {
        public SyncAssetRefList<RShader> Shaders;

        public List<ResourceLayoutElementDescription> resorses;
        public override void onLoaded()
        {

        }

        public void LoadChange(RShader shader)
        {

        }

        public override void buildSyncObjs(bool newRefIds)
        {
            Shaders = new SyncAssetRefList<RShader>(this, newRefIds);
            Shaders.loadChange += LoadChange;
        }
        public RMaterial(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public RMaterial()
        {
        }
    }
}
