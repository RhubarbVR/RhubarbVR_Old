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
	public class FontFromUrl : AssetProvider<RFont>, IAsset
	{
        public Sync<string> Url;

        public Sync<float> FontSize;

        public override void OnLoaded()
        {
            Load(null);
            UpdateFont().ConfigureAwait(false);
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
            Url = new Sync<string>(this, newRefIds)
            {
                Value = "https://cdn.discordapp.com/attachments/675870070261547035/911469622635659284/Sacramento-Regular.ttf"
            };
            Url.Changed += UrlChanged;
            FontSize = new Sync<float>(this, newRefIds)
            {
                Value = 25
            };
            FontSize.Changed += UrlChanged;
        }
        private void UrlChanged(IChangeable val)
        {
            UpdateFont().ConfigureAwait(false);
        }

        public async Task UpdateFont()
        {
            Logger.Log("Loading font URL:" + Url.Value);
            using var client = new HttpClient();
            using var response = await client.GetAsync(Url.Value);
            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();

            try
            {
                Logger.Log("Downloaded");
                Load(new RFont(new Font(streamToReadFrom, FontSize.Value)),true);
            }
            catch(Exception e)
            {
                Logger.Log($"Failed to Initialize font Error: " + e.ToString());
            }


        }

        public FontFromUrl(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public FontFromUrl()
		{
		}
	}
}
