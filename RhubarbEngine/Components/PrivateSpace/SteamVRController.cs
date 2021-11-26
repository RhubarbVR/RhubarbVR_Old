using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using RNumerics;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Input;
using System.Numerics;
using RhubarbEngine.Components.Physics;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.VirtualReality.OpenVR;
using Valve.VR;
using System.Runtime.InteropServices;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class SteamVRController : AssetProvider<RMesh>
	{
		public Sync<Creality> creality;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			creality = new Sync<Creality>(this,  newRefIds);
			creality.Changed += Changed;
		}

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
			if(creality.Value == Creality.Left)
			{
				Entity.SetLocalTrans(Input.LeftController.Posistion);
			}
			else if (creality.Value == Creality.Right)
			{
				Entity.SetLocalTrans(Input.RightController.Posistion);
			}
		}

        public override void OnLoaded()
        {
            base.OnLoaded();
			if (Engine.Rendering)
			{
				LoadMesh();
				Engine.RenderManager.VrContextUpdated += LoadMesh;
			}
		}

        private void Changed(IChangeable obj)
		{
			LoadMesh();
		}


		private void LoadMesh()
		{
			if (Engine.Rendering)
			{
				var context = Engine.RenderManager.VrContext;
				if (context.GetType() == typeof(OpenVRContext))
				{
					var contextCast = context as OpenVRContext;
					uint index = 0;
					if (creality.Value == contextCast.controllerOne.Creality)
					{
						index = contextCast.controllerOne.deviceindex;
					}
                    else
					{
						index = contextCast.controllerTwo.deviceindex;
					}
					ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
					var capacity = contextCast.VRSystem.GetStringTrackedDeviceProperty((uint)index, ETrackedDeviceProperty.Prop_RenderModelName_String, null, 0, ref error);
					var buffer = new System.Text.StringBuilder((int)capacity);
					contextCast.VRSystem.GetStringTrackedDeviceProperty((uint)index, ETrackedDeviceProperty.Prop_RenderModelName_String, buffer, capacity, ref error);

					var s = buffer.ToString();
					var pRenderModel = System.IntPtr.Zero;
					OpenVR.RenderModels.LoadRenderModel_Async(s, ref pRenderModel);
					//var renderModel = (RenderModel_t)Marshal.PtrToStructure(pRenderModel, typeof(RenderModel_t));
					//LoadVrMesh(renderModel);
				}
			}
		}

		private void LoadVrMesh(RenderModel_t renderModel)
		{
			
		}

		public override void OnAttach()
        {
            base.OnAttach();
			var shader = World.staticAssets.BasicUnlitShader;
			var mit = Entity.AttachComponent<RMaterial>();
			var meshRender = Entity.AttachComponent<MeshRender>();
			var textue2DFromUrl = Entity.AttachComponent<Textue2DFromUrl>();
			mit.Shader.Target = shader;
			meshRender.Materials.Add().Target = mit;
			meshRender.Mesh.Target = this;
			var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.Target = textue2DFromUrl;
		}

        public override void Dispose()
        {
			if (Engine.Rendering)
			{
				Engine.RenderManager.VrContextUpdated -= LoadMesh;
			}
			base.Dispose();
        }

        public SteamVRController(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SteamVRController()
		{
		}
	}
}
