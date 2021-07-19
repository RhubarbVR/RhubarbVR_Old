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
    [Category(new string[] { "Assets" })]
    public class Textue2DFromUrl : AssetProvider<RTexture2D>, IAsset
    {
        public Sync<string> Url;

        public override void onLoaded()
        {
            UpdateImg();
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            Url = new Sync<string>(this, newRefIds);
            Url.value = "https://cataas.com/cat/says/Base%20Url";
            Url.Changed += UrlChanged;
        }
        private void UrlChanged(IChangeable val)
        {
            UpdateImg();
        }

        public async void UpdateImg()
        {
            logger.Log("Loading img URL:" + Url.value);
            using (HttpClient client = new HttpClient())
            {
                logger.Log("Client");
                using (HttpResponseMessage response = await client.GetAsync(Url.value))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    logger.Log("Downloaded");
                    var _texture = new ImageSharpTexture(streamToReadFrom, true, true).CreateDeviceTexture(engine.renderManager.gd, engine.renderManager.gd.ResourceFactory);
                    load(new RTexture2D(engine.renderManager.gd.ResourceFactory.CreateTextureView(_texture)));
                }
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
