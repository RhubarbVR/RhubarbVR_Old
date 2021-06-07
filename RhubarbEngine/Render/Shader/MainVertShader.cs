using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public class MainVertShader : ShaderPart
    {
        public virtual string TopCode
        {
            get
            {
                return @"
#version 450
";
            }
        }
        public string UserCode = @"
layout (set = 0, binding = 0) uniform WVP
{
    mat4 Proj;
    mat4 View;
    mat4 World;
};

layout(set = 0, binding = 1) uniform texture2D Input;
layout(set = 0, binding = 2) uniform sampler InputSampler;

layout (location = 0) in vec3 vsin_Position;
layout (location = 1) in vec2 vsin_UV;

layout (location = 0) out vec2 fsin_UV;

void main()
{
    gl_Position = Proj * View * World * vec4(vsin_Position, 1);
    fsin_UV = vsin_UV;
}
";
    }
}
