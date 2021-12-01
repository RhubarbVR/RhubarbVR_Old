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
using RNumerics;
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
	[Category(new string[] { "Assets/Shaders" })]
	public class WireFrameShader : AssetProvider<RShader>, IAsset
	{

		public override void OnLoaded()
		{
			Logger.Log("Loadded Shader");
			var shader = new RShader();
            shader.AddUniform("fillcolor", Render.Shader.ShaderValueType.Val_color, Render.Shader.ShaderType.MainFrag);
            shader.AddUniform("wirecolor", Render.Shader.ShaderValueType.Val_color, Render.Shader.ShaderType.MainFrag);
            shader.AddUniform("wirewidth", Render.Shader.ShaderValueType.Val_float, Render.Shader.ShaderType.MainFrag);
            shader.mainFragCode.userCode = @"
layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec3 fsin_Pos;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    if(min(min(fsin_Pos.x, fsin_Pos.y), fsin_Pos.z) < wirewidth/10.0) {
        fsout_Color0 = wirecolor;
    } else {
        fsout_Color0 = fillcolor;
    }
}
";
            shader.mainVertCode.UserCode = @"
layout (location = 0) out vec2 fsin_UV;
layout (location = 1) out vec3 fsin_Pos;

void main()
{
    gl_Position = Proj * View * World * vec4(vsin_Position, 1);
    fsin_Pos = gl_Position.xyz;
    fsin_UV = vsin_UV;
}
";
            shader.LoadShader(Engine.RenderManager.Gd, Logger);
			Load(shader);
		}

		public override void BuildSyncObjs(bool newRefIds)
		{

		}
		public WireFrameShader(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public WireFrameShader()
		{
		}
	}
}
