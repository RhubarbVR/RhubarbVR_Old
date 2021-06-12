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
    public class StaicMainShader : AssetProvider<RShader>,IAsset
    {

        public override void onLoaded()
        {
            logger.Log("Loadded Shader");
            RShader shader = new RShader();
            //shader.addUniform("Texture", Render.Shader.ShaderValueType.Val_texture2D, Render.Shader.ShaderType.MainFrag);
            shader.LoadShader(engine.renderManager.gd, logger);
            load(shader);
        }

        public override void buildSyncObjs(bool newRefIds)
        {

        }
        public StaicMainShader(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public StaicMainShader()
        {
        }
    }
}
