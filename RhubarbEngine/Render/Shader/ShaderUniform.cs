﻿using System;
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

		public string GetCode(int location)
		{
            string code;
			switch (valueType)
			{
				case ShaderValueType.Val_texture2D:
					code = $"layout(set = 0, binding = {location}) uniform texture2D {fieldName}; \n";
					break;
				case ShaderValueType.Val_color:
					code = $"layout(set = 0, binding = {location}) uniform {valueType}{fieldName} {{ vec4 {fieldName}; }}; \n";
					break;
				default:
					var type = valueType.ToString().Replace("Val_", "");
					code = $"layout(set = 0, binding = {location}) uniform {valueType}{fieldName} {{ {type} {fieldName}; }}; \n";
					break;
			}
			return code;
		}

		public ResourceLayoutElementDescription GetResourceLayoutElementDescription()
		{
			ResourceKind resourceKind;
			ShaderStages shaderStage;
			if ((int)valueType <= 30)
			{
				resourceKind = ResourceKind.UniformBuffer;
			}
			else if ((int)valueType <= 33)
			{
				resourceKind = ResourceKind.TextureReadOnly;
			}
			else
			{
                Console.WriteLine("Shader Value Type not Found", true);
				throw new Exception("Shader Value Type not Found");
			}
			if ((int)shaderType % 2 == 0)
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
