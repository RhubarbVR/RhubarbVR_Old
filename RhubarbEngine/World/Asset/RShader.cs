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
using g3;
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
        public MainFragShader mainFragCode = new MainFragShader();

        public ShadowFragShader shadowFragCode = new ShadowFragShader();

        public MainVertShader mainVertCode = new MainVertShader();

        public ShadowVertShader shadowVertCode = new ShadowVertShader();

        public Veldrid.Shader mainVertShader;

        public Veldrid.Shader mainFragShader;

        public Veldrid.Shader shadowVertShader;

        public Veldrid.Shader shadowFragShader;

        public ResourceLayout mainresourceLayout;

        public ResourceLayout shadowresourceLayout;

        public bool shaderLoaded;

        public List<ShaderUniform> Fields = new List<ShaderUniform>();

        public List<IDisposable> disposable = new List<IDisposable>();
        public void Dispose()
        {
            shaderLoaded = false;
            foreach (IDisposable dep in disposable)
            {
                dep.Dispose();
            }
            mainresourceLayout = null;
            shadowresourceLayout = null;
        }
        public void BuildCode()
        {
            int loc = 2;
            string mvert = "";
            string mfrag = "";
            string svert = "";
            string sfrag = "";
            foreach (ShaderUniform field in Fields)
            {
                switch (field.shaderType)
                {
                    case ShaderType.MainVert:
                        mvert += field.getCode(loc);
                        break;
                    case ShaderType.MainFrag:
                        mfrag += field.getCode(loc);
                        break;
                    case ShaderType.ShadowVert:
                        svert += field.getCode(loc);
                        break;
                    case ShaderType.ShadowFrag:
                        sfrag += field.getCode(loc);
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


        public void createResourceLayouts(ResourceFactory factory)
        {
            if (mainresourceLayout != null || shadowresourceLayout != null)
            {
                throw new Exception("Resource Layout already Created");
            }
            ResourceLayoutDescription main = new ResourceLayoutDescription();
            ResourceLayoutDescription shadow = new ResourceLayoutDescription();

            List<ResourceLayoutElementDescription> mainElements = new List<ResourceLayoutElementDescription>();
            List<ResourceLayoutElementDescription> shadowElements = new List<ResourceLayoutElementDescription>();

            ResourceLayoutElementDescription uboelement = new ResourceLayoutElementDescription("WVP", ResourceKind.UniformBuffer, ShaderStages.Vertex);
            mainElements.Add(uboelement);
            shadowElements.Add(uboelement);

            ResourceLayoutElementDescription Sampler = new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment);
            mainElements.Add(Sampler);
            shadowElements.Add(Sampler);

            foreach (ShaderUniform field in Fields)
            {
                if ((int)field.shaderType <= 2)
                {
                    mainElements.Add(field.getResourceLayoutElementDescription());
                }
                else
                {
                    shadowElements.Add(field.getResourceLayoutElementDescription());
                }
            }

            main.Elements = mainElements.ToArray();
            mainresourceLayout = factory.CreateResourceLayout(main);
            disposable.Add(mainresourceLayout);
            shadow.Elements = shadowElements.ToArray();
            shadowresourceLayout = factory.CreateResourceLayout(shadow);
            disposable.Add(shadowresourceLayout);
        }

        public ShaderUniform addUniform(string name, ShaderValueType vType, ShaderType stype)
        {
            ShaderUniform e = new ShaderUniform(name, vType, stype);
            Fields.Add(e);
            return e;
        }

        public void removeUniform(string name, ShaderValueType vType, ShaderType stype)
        {
            ShaderUniform e = new ShaderUniform(name, vType, stype);
            Fields.Remove(e);
        }
        public void LoadShader(GraphicsDevice gd, UnitLogs log)
        {
            try
            {
                Dispose();

                BuildCode();

                string mainFragShader_Code = mainFragCode.getCode();
                string shadowFragShader_Code = shadowFragCode.getCode();
                string mainVertShader_Code = mainVertCode.getCode();
                string shadowVertShader_Code = shadowVertCode.getCode();

                log.Log("Comnpileing ShaderCode MainFragSgader: \n" + mainFragShader_Code + " ShadowFragSgader: \n" + shadowFragShader_Code + " MainVertexSgader: \n" + mainVertShader_Code + " ShadowVertexSgader: \n" + shadowVertShader_Code);

                Shader[] mainshaders = gd.ResourceFactory.CreateFromSpirv(
                    createShaderDescription(ShaderStages.Vertex, mainVertShader_Code),
                  createShaderDescription(ShaderStages.Fragment, mainFragShader_Code)
                   );

                Shader[] shadowshaders = gd.ResourceFactory.CreateFromSpirv(
                      createShaderDescription(ShaderStages.Vertex, shadowVertShader_Code),
                      createShaderDescription(ShaderStages.Fragment, shadowFragShader_Code));

                mainFragShader = mainshaders[0];
                shadowFragShader = shadowshaders[0];
                mainVertShader = mainshaders[1];
                shadowVertShader = shadowshaders[1];
                disposable.Add(mainFragShader);
                disposable.Add(shadowFragShader);
                disposable.Add(mainVertShader);
                disposable.Add(shadowVertShader);

                log.Log("creating shader ResourceLayouts");
                createResourceLayouts(gd.ResourceFactory);

                log.Log("Loadded Shader");
                shaderLoaded = true;
            }
            catch (Exception e)
            {
                shaderLoaded = false;
                log.Log("Failed to load shader" + e.ToString(), true);
            }
        }
        public ShaderDescription createShaderDescription(ShaderStages stage,string _shaderCode)
        {
            return new ShaderDescription(stage, Encoding.ASCII.GetBytes(_shaderCode), "main");
        }


        public RShader()
        {
        }
    }
}
