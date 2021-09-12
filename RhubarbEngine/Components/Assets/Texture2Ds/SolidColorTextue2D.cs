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
using RhubarbEngine.World.Asset;
using g3;
using System.Numerics;
using Veldrid;
using RhubarbEngine.Render;
using RhubarbEngine.Utilities;
using Veldrid.Utilities;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.IO;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Render.Material.Fields;
using RhubarbEngine.Render.Shader;
using System.Net;
using System.Web;
using System.Net.Http;

namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Texture2Ds" })]
	public class SolidColorTextue2D : AssetProvider<RTexture2D>, IAsset
	{
		public Sync<Colorf> color;

		private Texture texture;
		private TextureView view;

		public override void onLoaded()
		{
			texture = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, color.Value), false).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
			view = engine.renderManager.gd.ResourceFactory.CreateTextureView(texture);
			load(new RTexture2D(view));
		}

		public override void buildSyncObjs(bool newRefIds)
		{
			color = new Sync<Colorf>(this, newRefIds);
			color.Value = Colorf.White;
			color.Changed += Color_Changed;
		}

		private void Color_Changed(IChangeable obj)
		{
			if (texture == null)
				return;
			texture.UpdateTexture(new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, color.Value), false), engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
		}

		public SolidColorTextue2D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SolidColorTextue2D()
		{
		}
	}
}
