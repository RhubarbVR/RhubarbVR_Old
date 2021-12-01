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

namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Utility" })]
	public class TextureRatioDriver : Component
	{
        public AssetRef<RTexture2D> texture;

        public Driver<float> WidthRatio;

		public override void BuildSyncObjs(bool newRefIds)
		{
            texture = new AssetRef<RTexture2D>(this, newRefIds);
            texture.LoadChange += Texture_LoadChange;
            WidthRatio = new Driver<float>(this, newRefIds);
            WidthRatio.Changed += WidthRatio_Changed;
        }

        private void WidthRatio_Changed(IChangeable obj)
        {
            Texture_LoadChange(texture.Asset);
        }

        private void Texture_LoadChange(RTexture2D obj)
        {
            if(obj is not null)
            {
                if(obj.view is not null)
                {
                    WidthRatio.Drivevalue = obj.view.Target.Width / (float)obj.view.Target.Height;
                }
            }
        }

        public TextureRatioDriver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public TextureRatioDriver()
		{
		}
	}
}
