using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public abstract class ShaderPart
    {
        public static string TopCode = @"
#version 450";

        public Dictionary<string, Uniform> uniforms = new Dictionary<string, Uniform>();

        public virtual string UserCode { get { return @"
layout(set = 0, binding = 1) uniform texture2D Input;
layout(set = 0, binding = 2) uniform sampler InputSampler;

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    vec2 uv = fsin_UV;
    uv.y = 1 - uv.y;

    fsout_Color0 = texture(sampler2D(Input, InputSampler), uv);
}
"; } }
        public string getUniformString()
        {
            string val = "";
            foreach (Uniform item in uniforms.Values)
            {
                val += item.getUniformString();
            }
            return val;
        }

        public virtual string getCode()
        {
            return TopCode + "\n" + getUniformString() + "\n" + UserCode;
        }
    }
}
