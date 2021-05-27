using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public abstract class ShaderPart
    {
        public Dictionary<string, Uniform> uniforms = new Dictionary<string, Uniform>();

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
            return "Trains";
        }
    }
}
