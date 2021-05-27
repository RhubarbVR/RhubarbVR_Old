using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public class Uniform
    {
        public ShaderValueType type;

        public int location;

        public bool locationSet;

        public bool constructor;

        public string constructorValue;

        public string variableName;

        public string getUniformString()
        {
            string val = "";
            if (locationSet)
            {
                val = val + $"layout(location = {location}) uniform ";
            }
            string Type = type.ToString().Replace("Val_", "");
            val = val + Type + variableName;
            if (constructor)
            {
                val = val + $"new {Type}({constructorValue})" + ";\n";
            }
            else
            {
                val = val + ";\n";
            }
            return val;
        }

        public Uniform(string _variableName, ShaderValueType _type, string _constructorValue,int _location) : this(_variableName, _type, _constructorValue)
        {
            location = _location;
            locationSet = true;
        }

        public Uniform(string _variableName, ShaderValueType _type, int _location) : this(_variableName, _type)
        {
            location = _location;
            locationSet = true;
        }

        public Uniform(string _variableName, ShaderValueType _type,string _constructorValue) : this(_variableName, _type)
        {
            constructorValue = _constructorValue;
            constructor = true;
        }

        public Uniform(string _variableName,ShaderValueType _type): this(_variableName)
        {
            type = _type;
        }

        public Uniform(string _variableName)
        {
            variableName = _variableName;
        }
    }
}
