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
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Interaction/Button")]
	public class ImGUIImageButton : UIWidget
	{

		public AssetRef<RTexture2D> texture;
		public Sync<Vector2f> size;
		public Sync<Vector2f> UV0;
		public Sync<Vector2f> UV1;
		public Sync<Colorf> tint;
		public Sync<Colorf> big;
		public Sync<int> padding;
		public Sync<bool> TintOnClick;
		public Sync<float> TintOnClickTime;

		public SyncDelegate action;
		private TextureView _view;

		public override void OnLoaded()
		{
			base.OnLoaded();
			LoadTextureView();
		}

		private void Test()
		{
			Logger.Log("This is a test of test in test");
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);

			texture = new AssetRef<RTexture2D>(this, newRefIds);
			texture.loadChange += AssetChange;
            size = new Sync<Vector2f>(this, newRefIds)
            {
                Value = new Vector2f(100, 100)
            };
            UV0 = new Sync<Vector2f>(this, newRefIds)
            {
                Value = new Vector2f(0, 0)
            };
            UV1 = new Sync<Vector2f>(this, newRefIds)
            {
                Value = new Vector2f(1, 1)
            };
            padding = new Sync<int>(this, newRefIds)
            {
                Value = 0
            };
            tint = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.White
            };
            big = new Sync<Colorf>(this, newRefIds)
            {
                Value = Colorf.White
            };
            action = new SyncDelegate(this, newRefIds)
            {
                Target = Test
            };
            TintOnClick = new Sync<bool>(this, newRefIds)
            {
                Value = true
            };
            TintOnClickTime = new Sync<float>(this, newRefIds)
            {
                Value = 0.1f
            };
        }
		public void AssetChange(RTexture2D newAsset)
		{
			LoadTextureView();
		}

		public void LoadTextureView()
		{
			if (texture.Target != null)
			{
				if (texture.Asset != null)
				{
					if (texture.Asset.view != null)
					{
						SetResource(texture.Asset.view);
					}
					else
					{
						SetResource(Engine.renderManager.nulview);
					}
				}
				else
				{
					SetResource(Engine.renderManager.nulview);
				}
			}
			else
			{
				SetResource(Engine.renderManager.nulview);
			}

		}
		private void SetResource(TextureView res)
		{
			if (_view != res)
			{
				_view = res;
			}
		}

		public ImGUIImageButton(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public ImGUIImageButton()
		{
		}

		private DateTime _lastClick;

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			var tintval = tint.Value;
			if (TintOnClick.Value & (DateTime.UtcNow - _lastClick) < new TimeSpan(0, 0, 0, 0, (int)(TintOnClickTime.Value * 1000)))
			{
				tintval *= 0.8f;
			}
			if (ImGui.ImageButton(imGuiRenderer.GetOrCreateImGuiBinding(Engine.renderManager.gd.ResourceFactory, _view), new Vector2(size.Value.x, size.Value.y), new Vector2(UV0.Value.x, UV0.Value.y), new Vector2(UV1.Value.x, UV1.Value.y), padding.Value, big.Value.ToRGBA().ToSystem(), tintval.ToRGBA().ToSystem()))
			{
				if (TintOnClick.Value)
				{
					_lastClick = DateTime.UtcNow;
				}
				action.Target?.Invoke();
			}
		}
	}
}
