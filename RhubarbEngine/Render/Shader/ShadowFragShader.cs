using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public class ShadowFragShader: ShaderPart
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
    }
}
