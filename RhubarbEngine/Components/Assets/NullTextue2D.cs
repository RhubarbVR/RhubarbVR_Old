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
using RhubarbEngine.Render.Material.Fields;
using RhubarbEngine.Render.Shader;
using System.Net;
using System.Web;
using System.Net.Http;

namespace RhubarbEngine.Components.Assets
{
    [Category(new string[] { "Assets" })]
    public class NullTextue2D : AssetProvider<RTexture2D>, IAsset
    {

        public override void onLoaded()
        {
            load(new RTexture2D(engine.renderManager.nulview));
        }

        public override void buildSyncObjs(bool newRefIds)
        {

        }

        public NullTextue2D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public NullTextue2D()
        {
        }
    }
}
