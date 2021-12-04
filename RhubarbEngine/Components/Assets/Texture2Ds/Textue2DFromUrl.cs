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
    public class Textue2DFromUrl : AssetProvider<RTexture2D>, IAsset
    {
        public Sync<string> Url;

        public override void OnLoaded()
        {
            UpdateImg().ConfigureAwait(false);
        }

        public override void BuildSyncObjs(bool newRefIds)
        {
            Url = new Sync<string>(this, newRefIds)
            {
                Value = "https://cataas.com/cat/says/Base%20Url%20For%20RhubarbVR"
            };
            Url.Changed += UrlChanged;
        }
        private void UrlChanged(IChangeable val)
        {
            UpdateImg().ConfigureAwait(false);
        }

        public async Task UpdateImg()
        {
            Logger.Log("Loading img URL:" + Url.Value);
            using var client = new HttpClient();
            Logger.Log("Client");
            using var response = await client.GetAsync(Url.Value);
            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();

            try
            {
                Logger.Log("Downloaded");
                var _texture = new ImageSharpTexture(streamToReadFrom, true, true).CreateDeviceTexture(Engine.RenderManager.Gd, Engine.RenderManager.Gd.ResourceFactory);
                Load(new RTexture2D(Engine.RenderManager.Gd.ResourceFactory.CreateTextureView(_texture)), true);
            }
            catch
            {
                Logger.Log($"Failed to Initialize image");
                Load(null);
            }
        }

        public Textue2DFromUrl(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public Textue2DFromUrl()
        {
        }
    }
}
