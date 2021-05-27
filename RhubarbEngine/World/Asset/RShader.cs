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

        public bool shaderLoaded;

        public List<(string type, string name)> Fields = new List<(string type, string name)>();

        public List<IDisposable> disposable = new List<IDisposable>();
        public void Dispose()
        {
            shaderLoaded = false;
            foreach (IDisposable dep in disposable)
            {
                dep.Dispose();
            }
        }



        public void LoadShader(GraphicsDevice gd)
        {
            try
            {
                Dispose();
                string mainFragShader_Code = mainFragCode.getCode();
                string shadowFragShader_Code = shadowFragCode.getCode();
                string mainVertShader_Code = mainVertCode.getCode();
                string shadowVertShader_Code = shadowVertCode.getCode();




                mainFragShader = gd.ResourceFactory.CreateShader(createShaderDescription(ShaderStages.Fragment, mainFragShader_Code));
                shadowFragShader = gd.ResourceFactory.CreateShader(createShaderDescription(ShaderStages.Fragment, shadowFragShader_Code));
                mainVertShader = gd.ResourceFactory.CreateShader(createShaderDescription(ShaderStages.Vertex, mainVertShader_Code));
                shadowVertShader = gd.ResourceFactory.CreateShader(createShaderDescription(ShaderStages.Vertex, shadowVertShader_Code));
                disposable.Add(mainFragShader);
                disposable.Add(shadowFragShader);
                disposable.Add(mainVertShader);
                disposable.Add(shadowVertShader);

                shaderLoaded = true;
            }
            catch
            {
                shaderLoaded = false;
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
