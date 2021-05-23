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

namespace RhubarbEngine.World.Asset
{
    public class RShader : IAsset
    {
        private string _shaderCode = "";

        public List<(string type, string name)> Fields = new List<(string type, string name)>();

        public void Dispose()
        {

        }

        public void setCode(string code)
        {

        }

        public Shader LoadShader(GraphicsDevice gd, IWorldObject owner)
        {
            Shader veldridShader = gd.ResourceFactory.CreateShader(createShaderDescription());
            owner.addDisposable(veldridShader);
            return veldridShader;
        }
        public ShaderDescription createShaderDescription()
        {
            ShaderStages stage = ShaderStages.None;
            return new ShaderDescription(stage, Encoding.ASCII.GetBytes(_shaderCode), "main");
        }


        public RShader()
        {
        }
    }
}
