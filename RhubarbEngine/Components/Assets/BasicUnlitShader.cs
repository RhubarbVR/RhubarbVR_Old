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
    public class BasicUnlitShader : AssetProvider<RShader>,IAsset
    {

        public override void onLoaded()
        {
            logger.Log("Loadded Shader");
            RShader shader = new RShader();
            shader.addUniform("Texture", Render.Shader.ShaderValueType.Val_texture2D, Render.Shader.ShaderType.MainFrag);
            //shader.addUniform("rambow", Render.Shader.ShaderValueType.Val_color, Render.Shader.ShaderType.MainFrag);
            shader.mainFragCode.userCode = @"

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = texture(sampler2D(Texture, Sampler), uv);
}
"; ;
            shader.LoadShader(engine.renderManager.gd, logger);
            load(shader);
        }

        public override void buildSyncObjs(bool newRefIds)
        {

        }
        public BasicUnlitShader(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public BasicUnlitShader()
        {
        }
    }
}
