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
using RNumerics;
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

        private Texture _texture;
        private TextureView _view;

		public override void OnLoaded()
		{
			_texture = new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, color.Value), false).CreateDeviceTexture(Engine.renderManager.gd, Engine.renderManager.gd.ResourceFactory);
			_view = Engine.renderManager.gd.ResourceFactory.CreateTextureView(_texture);
			Load(new RTexture2D(_view));
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
            color = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.White
            };
            color.Changed += Color_Changed;
		}

		private void Color_Changed(IChangeable obj)
		{
			if (_texture == null)
            {
                return;
            }

            _texture.UpdateTexture(new ImageSharpTexture(ImageSharpExtensions.CreateTextureColor(2, 2, color.Value), false), Engine.renderManager.gd, Engine.renderManager.gd.ResourceFactory);
		}

		public SolidColorTextue2D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SolidColorTextue2D()
		{
		}
	}
}
