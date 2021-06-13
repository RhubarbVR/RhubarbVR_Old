using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

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
                case ShaderValueType.Val_color:
                    code = $"layout(set = 0, binding = {location}) uniform vec4 {fieldName};\n";
                    break;
                default:
                    string type = valueType.ToString().Replace("Val_", "");
                    code = $"layout(set = 0, binding = {location}) uniform {valueType} {{ {type} {fieldName}; }}; \n";
                    break;
            }
            return code;
        }

        public ResourceLayoutElementDescription getResourceLayoutElementDescription()
        {
            ResourceKind resourceKind;
            ShaderStages shaderStage;
            if ((int)valueType <= 35)
            {
                resourceKind = ResourceKind.UniformBuffer;
            }
            else if ((int)valueType <= 38)
            {
                resourceKind = ResourceKind.TextureReadOnly;
            }
            else
            {
                Logger.Log("Shader Value Type not Found", true);
                throw new Exception("Shader Value Type not Found");
            }
            if((int)shaderType%2 == 0)
            {
                shaderStage = ShaderStages.Fragment;
            }
            else
            {
                shaderStage = ShaderStages.Vertex;
            }
            return new ResourceLayoutElementDescription(fieldName, resourceKind, shaderStage);
        }

        public ShaderUniform(string name, ShaderValueType vType, ShaderType stype)
        {
            valueType = vType;
            shaderType = stype;
            fieldName = name;
        }
    }
}
