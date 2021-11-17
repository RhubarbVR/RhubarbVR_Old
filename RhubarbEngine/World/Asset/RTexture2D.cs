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
using Veldrid.Utilities;

namespace RhubarbEngine.World.Asset
{
	public class RTexture2D : IAsset
	{

		public List<IDisposable> disposables = new();

		public TextureView view;

        public void Dispose()
		{
			foreach (var dep in disposables)
			{
				dep.Dispose();
			}
		}

		public void AddDisposable(IDisposable val)
		{
			disposables.Add(val);
		}

		public RTexture2D(TextureView _view)
		{
			view = _view;
		}
	}
}
