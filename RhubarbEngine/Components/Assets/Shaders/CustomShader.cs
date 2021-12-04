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
	public class CustomShader : AssetProvider<RShader>, IAsset
	{
        public class ShaderUniform : Worker
        {
            public Sync<Render.Shader.ShaderType> Shader;

            public Sync<Render.Shader.ShaderValueType> Type;

            public Sync<string> Name;

            public override void BuildSyncObjs(bool newRefIds)
            {
                Shader = new Sync<Render.Shader.ShaderType>(this, newRefIds);
                Type = new Sync<Render.Shader.ShaderValueType>(this, newRefIds);
                Name = new Sync<string>(this, newRefIds);
            }
            public ShaderUniform(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public ShaderUniform()
            {
            }
        }

        public class SyncBlendAttachmentDescription : Worker
        {
            public Sync<bool> BlendEnabled;
            
            public Sync<BlendFactor> SourceColorFactor;
            
            public Sync<BlendFactor> DestinationColorFactor;
            
            public Sync<BlendFunction> ColorFunction;
            
            public Sync<BlendFactor> SourceAlphaFactor;
            
            public Sync<BlendFactor> DestinationAlphaFactor;

            public Sync<BlendFunction> AlphaFunction;

            public override void BuildSyncObjs(bool newRefIds)
            {
                BlendEnabled = new Sync<bool>(this, newRefIds);
                SourceColorFactor = new Sync<BlendFactor>(this, newRefIds);
                DestinationColorFactor = new Sync<BlendFactor>(this, newRefIds);
                ColorFunction = new Sync<BlendFunction>(this, newRefIds);
                SourceAlphaFactor = new Sync<BlendFactor>(this, newRefIds);
                DestinationAlphaFactor = new Sync<BlendFactor>(this, newRefIds);
                AlphaFunction = new Sync<BlendFunction>(this, newRefIds);
            }
            public SyncBlendAttachmentDescription(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public SyncBlendAttachmentDescription()
            {
            }
        }

        public class SyncBlendStateDescription : Worker
        {
            public Sync<Colorf> BlendFactor;

            public SyncObjList<SyncBlendAttachmentDescription> AttachmentStates;

            public Sync<bool> AlphaToCoverageEnabled;

            public override void BuildSyncObjs(bool newRefIds)
            {
                BlendFactor = new Sync<Colorf>(this, newRefIds);
                AttachmentStates = new SyncObjList<SyncBlendAttachmentDescription>(this, newRefIds);
                AlphaToCoverageEnabled = new Sync<bool>(this, newRefIds);
            }
            public SyncBlendStateDescription(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public SyncBlendStateDescription()
            {
            }
        }

        public class SyncStencilBehaviorDescription:Worker
        {

            public Sync<StencilOperation> Fail;

            public Sync<StencilOperation> Pass;

            public Sync<StencilOperation> DepthFail;

            public Sync<ComparisonKind> Comparison;

            public override void BuildSyncObjs(bool newRefIds)
            {
                Fail = new Sync<StencilOperation>(this, newRefIds);
                Pass = new Sync<StencilOperation>(this, newRefIds);
                DepthFail = new Sync<StencilOperation>(this, newRefIds);
                Comparison = new Sync<ComparisonKind>(this, newRefIds);
            }
            public SyncStencilBehaviorDescription(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public SyncStencilBehaviorDescription()
            {
            }
        }

        public class SyncDepthStencilStateDescription : Worker
        {

            public Sync<bool> DepthTestEnabled;
         
            public Sync<bool> DepthWriteEnabled;
            
            public Sync<ComparisonKind> DepthComparison;

            public Sync<bool> StencilTestEnabled;

            public SyncStencilBehaviorDescription StencilFront;

            public SyncStencilBehaviorDescription StencilBack;
            
            public Sync<byte> StencilReadMask;
            
            public Sync<byte> StencilWriteMask;
            
            public Sync<uint> StencilReference;

            public override void BuildSyncObjs(bool newRefIds)
            {
                DepthTestEnabled = new Sync<bool>(this, newRefIds);
                DepthWriteEnabled = new Sync<bool>(this, newRefIds);
                DepthComparison = new Sync<ComparisonKind>(this, newRefIds);
                StencilTestEnabled = new Sync<bool>(this, newRefIds);
                StencilFront = new SyncStencilBehaviorDescription(this, newRefIds);
                StencilBack = new SyncStencilBehaviorDescription(this, newRefIds);
                StencilReadMask = new Sync<byte>(this, newRefIds);
                StencilWriteMask = new Sync<byte>(this, newRefIds);
                StencilReference = new Sync<uint>(this, newRefIds);
            }

            public SyncDepthStencilStateDescription(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public SyncDepthStencilStateDescription()
            {
            }
        }

        public class SyncRasterizerStateDescription : Worker
        {

            public Sync<FaceCullMode> CullMode;
            
            public Sync<PolygonFillMode> FillMode;
            
            public Sync<FrontFace> FrontFace;
            
            public Sync<bool> DepthClipEnabled;
            
            public Sync<bool> ScissorTestEnabled;

            public override void BuildSyncObjs(bool newRefIds)
            {
                CullMode = new Sync<FaceCullMode>(this, newRefIds);
                FillMode = new Sync<PolygonFillMode>(this, newRefIds);
                FrontFace = new Sync<FrontFace>(this, newRefIds);
                DepthClipEnabled = new Sync<bool>(this, newRefIds);
                ScissorTestEnabled = new Sync<bool>(this, newRefIds);
            }
            public SyncRasterizerStateDescription(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
            {

            }
            public SyncRasterizerStateDescription()
            {
            }
        }
        public class SyncShaderSettings : Worker
        {
            public SyncBlendStateDescription blendStateDescription;

            public SyncDepthStencilStateDescription depthStencilStateDescription;

            public SyncRasterizerStateDescription rasterizerStateDescription;

            public Sync<PrimitiveTopology> primitiveTopology;

            public override void BuildSyncObjs(bool newRefIds)
            {
                blendStateDescription = new SyncBlendStateDescription(this, newRefIds);
                depthStencilStateDescription = new SyncDepthStencilStateDescription(this, newRefIds);
                rasterizerStateDescription = new SyncRasterizerStateDescription(this, newRefIds);
                primitiveTopology = new Sync<PrimitiveTopology>(this, newRefIds);
            }

            public SyncShaderSettings(IWorldObject _parent, bool newRefIds = true) : base(_parent.World,_parent, newRefIds)
            {

            }
            public SyncShaderSettings()
            {
            }
        }

        public SyncObjList<ShaderUniform> shaderUniforms;

        [MultiLineSync]
        public Sync<string> mainVertShader;

        [MultiLineSync]
        public Sync<string> mainFragShader;

        [MultiLineSync]
        public Sync<string> shadowVertShader;
        
        [MultiLineSync]
        public Sync<string> shadowFragShader;

        public SyncShaderSettings mainShaderSettings;

        public SyncShaderSettings shadowShaderSettings;

        private readonly RShader _shader = new();

        public override void BuildSyncObjs(bool newRefIds)
        {
            shaderUniforms = new SyncObjList<ShaderUniform>(this, newRefIds);
            mainVertShader = new Sync<string>(this, newRefIds);
            mainFragShader = new Sync<string>(this, newRefIds);
            shadowVertShader = new Sync<string>(this, newRefIds);
            shadowFragShader = new Sync<string>(this, newRefIds);
            mainShaderSettings = new SyncShaderSettings(this, newRefIds);
            shadowShaderSettings = new SyncShaderSettings(this, newRefIds);
        }

        public void Compile()
        {
            Load(null);
            Logger.Log("Compileing shader");
            _shader.AddUniform("Texture", Render.Shader.ShaderValueType.Val_texture2D, Render.Shader.ShaderType.MainFrag);
            _shader.AddUniform("TintColor", Render.Shader.ShaderValueType.Val_color, Render.Shader.ShaderType.MainFrag);
            _shader.mainFragCode.userCode = @"

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = texture(sampler2D(Texture, Sampler), uv)*TintColor;
}
";
            _shader.LoadShader(Engine.RenderManager.Gd, Logger);
            Load(_shader);
        }

		public CustomShader(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CustomShader()
		{
		}
	}
}
