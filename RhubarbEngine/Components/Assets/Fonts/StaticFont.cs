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
using System.Net.Http;
using SharpText.Core;
namespace RhubarbEngine.Components.Assets
{
	[Category(new string[] { "Assets/Fonts" })]
	public class StaticFont : AssetProvider<RFont>, IAsset
	{
        public enum Fonts
        {
            ArialCEBold,
            FifteenNarrow,
            Sacramento_Regular,
            Ubuntu_Bold,
            Ubuntu_BoldItalic,
            Ubuntu_Italic,
            Ubuntu_Light,
            Ubuntu_LightItalic,
            Ubuntu_Medium,
            Ubuntu_MediumItalic,
            Ubuntu_Regular,
        }

        public Sync<Fonts> Type;

        public Sync<float> FontSize;

        public override void OnLoaded()
        {
            Task.Run(UpdateFont);
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
            Type = new Sync<Fonts>(this, newRefIds);
            Type.Changed += ValueChange;
            FontSize = new Sync<float>(this, newRefIds)
            {
                Value = 25
            };
            FontSize.Changed += ValueChange;
        }
        private void ValueChange(IChangeable val)
        {
            Task.Run(UpdateFont);
        }

        public void UpdateFont()
        {
            try
            {
                Logger.Log("Downloaded");
                Load(new RFont(new Font(AppDomain.CurrentDomain.BaseDirectory+ "\\StaticAssets\\Fonts\\" + Type.Value.ToString()+".ttf", FontSize.Value)),true);
            }
            catch(Exception e)
            {
                Logger.Log($"Failed to Initialize font Error: " + e.ToString());
            }


        }

        public StaticFont(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public StaticFont()
		{
		}
	}
}
