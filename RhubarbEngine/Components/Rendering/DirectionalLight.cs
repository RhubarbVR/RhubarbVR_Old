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
using RhubarbEngine.Render;
using RhubarbEngine.World.Asset;
using RNumerics;
using Veldrid;
using System.Numerics;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;

namespace RhubarbEngine.Components.Rendering
{
	[Category(new string[] { "Rendering" })]
	public class DirectionalLight : Component, IRenderObject
	{
        public RenderFrequency RenderFrac
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Threaded
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void BuildSyncObjs(bool newRefIds)
		{

		}
		public void Render()
		{
			throw new NotImplementedException();
		}

		public DirectionalLight(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public DirectionalLight()
		{
		}
	}
}
