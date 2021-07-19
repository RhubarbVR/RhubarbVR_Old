using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public abstract class FragShader : ShaderPart
    {
        public override string TopCode
        {
            get
            {
                return @"
#version 450
layout(set = 0, binding = 1) uniform sampler Sampler;
";
            }
        }

        public string userCode = @"

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = vec4(0.87,0.24,0.84,0.75);
}
";
        public override string UserCode
        {
            get
            {
                return userCode;
            }
            set
            {
                userCode = value;
            }
        }
    }
}