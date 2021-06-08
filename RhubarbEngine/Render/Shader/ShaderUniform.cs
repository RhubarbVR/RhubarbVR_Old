using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public class ShaderUniform
    {
        public ShaderValueType valueType;

        public ShaderType shaderType;

        public string fieldName = "";

        public string getCode(int location)
        {
            string code = "";
            switch (valueType)
            {
                case ShaderValueType.Val_Color:
                    code = $"layout(set = 1, binding = {location}) uniform vec4 {fieldName};\n";
                    break;
                default:
                    string type = valueType.ToString().Replace("Val_", "");
                    code = $"layout(set = 1, binding = {location}) uniform {type} {fieldName};\n";
                    break;
            }
            return code;
        }
        public ShaderUniform(string name, ShaderValueType vType, ShaderType stype)
        {
            valueType = vType;
            shaderType = stype;
            fieldName = name;
        }
    }
}
