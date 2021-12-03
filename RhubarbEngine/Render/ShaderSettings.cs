using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
using Veldrid;

namespace RhubarbEngine.Render
{
    public class UpdateingBlendAttachmentDescription
    {
        /// <summary>
        /// Controls whether blending is enabled for the color attachment.
        /// </summary>
        public bool BlendEnabled;
        /// <summary>
        /// Controls the source color's influence on the blend result.
        /// </summary>
        public BlendFactor SourceColorFactor;
        /// <summary>
        /// Controls the destination color's influence on the blend result.
        /// </summary>
        public BlendFactor DestinationColorFactor;
        /// <summary>
        /// Controls the function used to combine the source and destination color factors.
        /// </summary>
        public BlendFunction ColorFunction;
        /// <summary>
        /// Controls the source alpha's influence on the blend result.
        /// </summary>
        public BlendFactor SourceAlphaFactor;
        /// <summary>
        /// Controls the destination alpha's influence on the blend result.
        /// </summary>
        public BlendFactor DestinationAlphaFactor;
        /// <summary>
        /// Controls the function used to combine the source and destination alpha factors.
        /// </summary>
        public BlendFunction AlphaFunction;

        public UpdateingBlendAttachmentDescription()
        {
            BlendEnabled = true;
            SourceColorFactor = BlendFactor.SourceAlpha;
            DestinationColorFactor = BlendFactor.InverseSourceAlpha;
            ColorFunction = BlendFunction.Add;
            SourceAlphaFactor = BlendFactor.SourceAlpha;
            DestinationAlphaFactor = BlendFactor.InverseSourceAlpha;
            AlphaFunction = BlendFunction.Add;
        }

        public BlendAttachmentDescription GetBlindattachment()
        {
            return new BlendAttachmentDescription 
            {
                BlendEnabled = BlendEnabled,
                SourceColorFactor = SourceColorFactor,
                DestinationColorFactor = DestinationColorFactor,
                ColorFunction = ColorFunction,
                SourceAlphaFactor = SourceAlphaFactor,
                DestinationAlphaFactor = DestinationAlphaFactor,
                AlphaFunction = AlphaFunction,  
            };
        }
    }

    public class UpdateingBlendStateDescription
    {

        /// <summary>
        /// A constant blend color used in <see cref="BlendFactor.BlendFactor"/> and <see cref="BlendFactor.InverseBlendFactor"/>,
        /// or otherwise ignored.
        /// </summary>
        public Colorf BlendFactor;
        /// <summary>
        /// An array of <see cref="BlendAttachmentDescription"/> describing how blending is performed for each color target
        /// used in the <see cref="Pipeline"/>.
        /// </summary>
        public List<UpdateingBlendAttachmentDescription> AttachmentStates = new();
        /// <summary>
        /// Enables alpha-to-coverage, which causes a fragment's alpha value to be used when determining multi-sample coverage.
        /// </summary>
        public bool AlphaToCoverageEnabled;


        public BlendStateDescription GetBlindState()
        {
            return new BlendStateDescription
            {
                BlendFactor = new RgbaFloat(BlendFactor.ToRGBA().ToSystemNumrics()),
                AttachmentStates = (from e in AttachmentStates select e.GetBlindattachment()).ToArray(),
                AlphaToCoverageEnabled = AlphaToCoverageEnabled,
            };
        }

        public UpdateingBlendStateDescription()
        {
            AttachmentStates.Add(new UpdateingBlendAttachmentDescription());
        }
    }

    public class UpdateingStencilBehaviorDescription
    {
        /// <summary>
        /// The operation performed on samples that fail the stencil test.
        /// </summary>
        public StencilOperation Fail;
        /// <summary>
        /// The operation performed on samples that pass the stencil test.
        /// </summary>
        public StencilOperation Pass;
        /// <summary>
        /// The operation performed on samples that pass the stencil test but fail the depth test.
        /// </summary>
        public StencilOperation DepthFail;
        /// <summary>
        /// The comparison operator used in the stencil test.
        /// </summary>
        public ComparisonKind Comparison;

        public StencilBehaviorDescription GetStencil()
        {
            return new StencilBehaviorDescription
            {
                Fail = Fail,
                Pass = Pass,
                DepthFail = DepthFail,
                Comparison = Comparison
            };
        }
    }

    public class UpdateingDepthStencilStateDescription
    {
        /// <summary>
        /// Controls whether depth testing is enabled.
        /// </summary>
        public bool DepthTestEnabled;
        /// <summary>
        /// Controls whether new depth values are written to the depth buffer.
        /// </summary>
        public bool DepthWriteEnabled;
        /// <summary>
        /// The <see cref="ComparisonKind"/> used when considering new depth values.
        /// </summary>
        public ComparisonKind DepthComparison;

        /// <summary>
        /// Controls whether the stencil test is enabled.
        /// </summary>
        public bool StencilTestEnabled;
        /// <summary>
        /// Controls how stencil tests are handled for pixels whose surface faces towards the camera.
        /// </summary>
        public UpdateingStencilBehaviorDescription StencilFront = new();
        /// <summary>
        /// Controls how stencil tests are handled for pixels whose surface faces away from the camera.
        /// </summary>
        public UpdateingStencilBehaviorDescription StencilBack = new();
        /// <summary>
        /// Controls the portion of the stencil buffer used for reading.
        /// </summary>
        public byte StencilReadMask;
        /// <summary>
        /// Controls the portion of the stencil buffer used for writing.
        /// </summary>
        public byte StencilWriteMask;
        /// <summary>
        /// The reference value to use when doing a stencil test.
        /// </summary>
        public uint StencilReference;

        public DepthStencilStateDescription GetDepthState()
        {
            return new DepthStencilStateDescription
            {
                DepthTestEnabled = DepthTestEnabled,
                DepthWriteEnabled = DepthWriteEnabled,
                DepthComparison = DepthComparison,
                StencilTestEnabled = StencilTestEnabled,
                StencilFront = StencilFront.GetStencil(),
                StencilBack = StencilBack.GetStencil(),
                StencilReadMask = StencilReadMask,
                StencilWriteMask = StencilWriteMask,
                StencilReference = StencilReference
            };
        }

        public UpdateingDepthStencilStateDescription()
        {
            DepthTestEnabled = true;
            DepthWriteEnabled = true;
            DepthComparison = ComparisonKind.LessEqual;
        }
    }

    public class UpdatingRasterizerStateDescription
    {
        /// <summary>
        /// Controls which face will be culled.
        /// </summary>
        public FaceCullMode CullMode;
        /// <summary>
        /// Controls how the rasterizer fills polygons.
        /// </summary>
        public PolygonFillMode FillMode;
        /// <summary>
        /// Controls the winding order used to determine the front face of primitives.
        /// </summary>
        public FrontFace FrontFace;
        /// <summary>
        /// Controls whether depth clipping is enabled.
        /// </summary>
        public bool DepthClipEnabled;
        /// <summary>
        /// Controls whether the scissor test is enabled.
        /// </summary>
        public bool ScissorTestEnabled;

        public RasterizerStateDescription GetResterizerState()
        {
            return new RasterizerStateDescription
            {
                CullMode = CullMode,
                FillMode = FillMode,
                FrontFace = FrontFace,
                DepthClipEnabled = DepthClipEnabled,
                ScissorTestEnabled = ScissorTestEnabled
            };
        }
        public UpdatingRasterizerStateDescription()
        {
            CullMode = FaceCullMode.None;
            FillMode = PolygonFillMode.Solid;
            FrontFace = FrontFace.Clockwise;
            DepthClipEnabled = true;
            ScissorTestEnabled = false;
        }
    }

    public class ShaderSettings
    {
        public UpdateingBlendStateDescription blendStateDescription = new();

        public UpdateingDepthStencilStateDescription depthStencilStateDescription = new();

        public UpdatingRasterizerStateDescription rasterizerStateDescription = new();

        public BlendStateDescription _blendStateDescription;

        public DepthStencilStateDescription _depthStencilStateDescription;

        public RasterizerStateDescription _rasterizerStateDescription;

        public PrimitiveTopology primitiveTopology; 

        public void Compile()
        {
            _blendStateDescription = blendStateDescription.GetBlindState();
            _depthStencilStateDescription = depthStencilStateDescription.GetDepthState();
            _rasterizerStateDescription = rasterizerStateDescription.GetResterizerState();
        }
    }
}
