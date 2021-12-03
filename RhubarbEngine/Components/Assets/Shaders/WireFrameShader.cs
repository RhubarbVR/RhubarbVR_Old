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
            shader.AddUniform("color", Render.Shader.ShaderValueType.Val_color, Render.Shader.ShaderType.MainFrag);
            shader.mainFragCode.userCode = @"
layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    fsout_Color0 = color;
}
";
            shader.mainVertCode.UserCode = @"
layout (location = 0) out vec2 fsin_UV;

void main()
{
    gl_Position = Proj * View * World * vec4(vsin_Position, 1);
    fsin_UV = vsin_UV;
}
";
            shader.mainShader.primitiveTopology = PrimitiveTopology.LineList;
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
