using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using Veldrid;
using System.Runtime.CompilerServices;
using RhubarbEngine.Render;
using System.Numerics;
using RhubarbEngine.Render.Shader;
using Veldrid.SPIRV;

namespace RhubarbEngine.World.Asset
{
	public class RShader : IAsset
	{
		public MainFragShader mainFragCode = new();

		public ShadowFragShader shadowFragCode = new();

		public MainVertShader mainVertCode = new();

		public ShadowVertShader shadowVertCode = new();

		public Veldrid.Shader mainVertShader;

		public Veldrid.Shader mainFragShader;

		public Veldrid.Shader shadowVertShader;

		public Veldrid.Shader shadowFragShader;

		public ResourceLayout mainresourceLayout;

		public ResourceLayout shadowresourceLayout;

		public bool shaderLoaded;

		public List<ShaderUniform> Fields = new();

		public List<IDisposable> disposable = new();
		public void Dispose()
		{
			shaderLoaded = false;
			foreach (var dep in disposable)
			{
				dep.Dispose();
			}
			mainresourceLayout = null;
			shadowresourceLayout = null;
		}
		public void BuildCode()
		{
			var loc = 2;
			var mvert = "";
			var mfrag = "";
			var svert = "";
			var sfrag = "";
			foreach (var field in Fields)
			{
				switch (field.shaderType)
				{
					case ShaderType.MainVert:
						mvert += field.GetCode(loc);
						break;
					case ShaderType.MainFrag:
						mfrag += field.GetCode(loc);
						break;
					case ShaderType.ShadowVert:
						svert += field.GetCode(loc);
						break;
					case ShaderType.ShadowFrag:
						sfrag += field.GetCode(loc);
						break;
					default:
						break;
				}
				loc++;
			}
			mainVertCode.InjectedCode = mvert;
			mainFragCode.InjectedCode = mfrag;
			shadowVertCode.InjectedCode = svert;
			shadowFragCode.InjectedCode = sfrag;
		}


		public void CreateResourceLayouts(ResourceFactory factory)
		{
			if (mainresourceLayout != null || shadowresourceLayout != null)
			{
				throw new Exception("Resource Layout already Created");
			}
			var main = new ResourceLayoutDescription();
			var shadow = new ResourceLayoutDescription();

			var mainElements = new List<ResourceLayoutElementDescription>();
			var shadowElements = new List<ResourceLayoutElementDescription>();

			var uboelement = new ResourceLayoutElementDescription("WVP", ResourceKind.UniformBuffer, ShaderStages.Vertex);
			mainElements.Add(uboelement);
			shadowElements.Add(uboelement);

			var Sampler = new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment);
			mainElements.Add(Sampler);
			shadowElements.Add(Sampler);

			foreach (var field in Fields)
			{
				if ((int)field.shaderType <= 2)
				{
					mainElements.Add(field.GetResourceLayoutElementDescription());
				}
				else
				{
					shadowElements.Add(field.GetResourceLayoutElementDescription());
				}
			}

			main.Elements = mainElements.ToArray();
			mainresourceLayout = factory.CreateResourceLayout(main);
			disposable.Add(mainresourceLayout);
			shadow.Elements = shadowElements.ToArray();
			shadowresourceLayout = factory.CreateResourceLayout(shadow);
			disposable.Add(shadowresourceLayout);
		}

		public ShaderUniform AddUniform(string name, ShaderValueType vType, ShaderType stype)
		{
			var e = new ShaderUniform(name, vType, stype);
			Fields.Add(e);
			return e;
		}

		public void RemoveUniform(string name, ShaderValueType vType, ShaderType stype)
		{
			var e = new ShaderUniform(name, vType, stype);
			Fields.Remove(e);
		}
		public void LoadShader(GraphicsDevice gd, IUnitLogs log)
		{
            if(gd is null)
            {
                return;
            }
			try
			{
				Dispose();

				BuildCode();

				var mainFragShader_Code = mainFragCode.getCode();
				var shadowFragShader_Code = shadowFragCode.getCode();
				var mainVertShader_Code = mainVertCode.getCode();
				var shadowVertShader_Code = shadowVertCode.getCode();

				//log.Log("Comnpileing ShaderCode MainFragSgader: \n" + mainFragShader_Code + " ShadowFragSgader: \n" + shadowFragShader_Code + " MainVertexSgader: \n" + mainVertShader_Code + " ShadowVertexSgader: \n" + shadowVertShader_Code);

				var mainshaders = gd.ResourceFactory.CreateFromSpirv(
                    CreateShaderDescription(ShaderStages.Vertex, mainVertShader_Code),
                  CreateShaderDescription(ShaderStages.Fragment, mainFragShader_Code)
				   );

				var shadowshaders = gd.ResourceFactory.CreateFromSpirv(
                      CreateShaderDescription(ShaderStages.Vertex, shadowVertShader_Code),
                      CreateShaderDescription(ShaderStages.Fragment, shadowFragShader_Code));

				mainFragShader = mainshaders[0];
				shadowFragShader = shadowshaders[0];
				mainVertShader = mainshaders[1];
				shadowVertShader = shadowshaders[1];
				disposable.Add(mainFragShader);
				disposable.Add(shadowFragShader);
				disposable.Add(mainVertShader);
				disposable.Add(shadowVertShader);

				log.Log("creating shader ResourceLayouts");
				CreateResourceLayouts(gd.ResourceFactory);

				log.Log("Loadded Shader");
				shaderLoaded = true;
			}
			catch (Exception e)
			{
				shaderLoaded = false;
				log.Log("Failed to load shader" + e.ToString(), true);
			}
		}
		public static ShaderDescription CreateShaderDescription(ShaderStages stage, string _shaderCode)
		{
			return new ShaderDescription(stage, Encoding.ASCII.GetBytes(_shaderCode), "main");
		}


		public RShader()
		{
		}
	}
}
