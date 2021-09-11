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
using g3;
using System.Numerics;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class PersonalSpace : Component
	{
		public SyncRef<Entity> FollowUser;
		public SyncRef<Entity> Keyboard;


		public override void buildSyncObjs(bool newRefIds)
		{
			FollowUser = new SyncRef<Entity>(this, newRefIds);
			Keyboard = new SyncRef<Entity>(this, newRefIds);
		}

		public void OpenKeyboard()
		{
			//    if (engine.outputType == VirtualReality.OutputType.Screen)
			//    {
			//        if (Keyboard.target != null)
			//        {
			//            Keyboard.target.enabled.value = false;
			//        }
			//        return;
			//    }
			if (Keyboard.target == null)
			{
				var keyboard = Keyboard.target = FollowUser.target.addChild("Keyboard");
				var e = keyboard;

				CurvedPlaneMesh bmesh = e.attachComponent<CurvedPlaneMesh>();
				bmesh.BottomRadius.value = engine.settingsObject.UISettings.KeyBoardCurve;
				bmesh.TopRadius.value = engine.settingsObject.UISettings.KeyBoardCurve + 10f;
				bmesh.Height.value = 0.4f;
				bmesh.Width.value = 0.5f;
				MeshInputPlane bmeshcol = e.attachComponent<MeshInputPlane>();
				Vector2f sizePix = new Vector2f(600, 600);
				bmeshcol.mesh.target = bmesh;
				bmeshcol.FocusedOverride.value = true;

				bmeshcol.pixelSize.value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				//e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
				e.rotation.value = Quaternionf.CreateFromEuler(90f, -90f, -90f) * Quaternionf.CreateFromEuler(0f, -40f, 0f);
				e.position.value = new Vector3f(0, -0.5, -0.6);
				RMaterial mit = e.attachComponent<RMaterial>();
				MeshRender meshRender = e.attachComponent<MeshRender>();
				ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
				imGUICanvas.noKeyboard.value = true;
				imGUICanvas.scale.value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				bmeshcol.pixelSize.value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				ImGUIKeyboard imGUIText = e.attachComponent<ImGUIKeyboard>();
				imGUICanvas.imputPlane.target = bmeshcol;
				imGUICanvas.element.target = imGUIText;
				mit.Shader.target = world.staticAssets.basicUnlitShader;
				meshRender.Materials.Add().target = mit;
				meshRender.Mesh.target = bmesh;
				imGUICanvas.noCloseing.value = true;
				imGUICanvas.noBackground.value = true;
				Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
				field.field.target = imGUICanvas;
			}
			else
			{
				Keyboard.target.enabled.value = true;
			}
		}

		public void CloseKeyboard()
		{
			if (Keyboard.target != null)
			{
				Keyboard.target.enabled.value = false;
			}
		}

		public override void OnAttach()
		{
			base.onLoaded();
			var d = world.RootEntity.addChild("User Follower");
			FollowUser.target = d;

			var e = d.addChild("Main Panel");
			d.attachComponent<UserInterfacePositioner>();
			e.attachComponent<TaskBarManager>();

			Entity rootent = world.RootEntity.addChild();
			rootent.name.value = $"PersonalSpace User";
			rootent.persistence.value = false;
			UserRoot userRoot = rootent.attachComponent<UserRoot>();
			userRoot.user.target = world.localUser;
			world.localUser.userroot.target = userRoot;
			Entity head = rootent.addChild("Head");
			head.attachComponent<Head>();
			head.addChild("Laser").attachComponent<InteractionLaser>().source.value = InteractionSource.HeadLaser;
			userRoot.Head.target = head;
			Entity left = rootent.addChild("Left hand");
			Entity right = rootent.addChild("Right hand");


			userRoot.LeftHand.target = left;
			userRoot.RightHand.target = right;
			Hand leftcomp = left.attachComponent<Hand>();
			leftcomp.userroot.target = userRoot;
			leftcomp.creality.value = Input.Creality.Left;
			Hand rightcomp = right.attachComponent<Hand>();
			rightcomp.creality.value = Input.Creality.Right;
			rightcomp.userroot.target = userRoot;

			var LeftLaser = left.addChild("Left Laser");
			LeftLaser.attachComponent<InteractionLaser>().source.value = InteractionSource.LeftLaser;
			LeftLaser.attachComponent<LaserVisual>().source.value = InteractionSource.LeftLaser;
			var RightLaser = right.addChild("Right Laser");
			RightLaser.attachComponent<InteractionLaser>().source.value = InteractionSource.RightLaser;
			RightLaser.attachComponent<LaserVisual>().source.value = InteractionSource.RightLaser;

			Entity leftContext = rootent.addChild("Left Context");
			Entity rightContext = rootent.addChild("Right Context");

			var leftC = leftContext.attachComponent<ContextMenu>();
			leftC.side.value = Input.Creality.Left;

			var rightC = rightContext.attachComponent<ContextMenu>();
			rightC.side.value = Input.Creality.Right;

			logger.Log("Spawned User PersonalSpace");
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (input.isKeyboardinuse)
				return;
			if ((input.mainWindows.GetKeyDown(Veldrid.Key.Tab) && (input.mainWindows.GetKey(Veldrid.Key.AltLeft) || input.mainWindows.GetKey(Veldrid.Key.AltRight))) || input.SecondaryPress(Input.Creality.None))
			{
				SwitchWorld();
			}
		}

		public void SwitchWorld()
		{
			var mang = engine.worldManager;

			var pos = mang.worlds.IndexOf(mang.focusedWorld) + 1;
			if (pos == mang.worlds.Count)
			{
				JoinNextIfBackground(0);
			}
			else
			{
				JoinNextIfBackground(pos);
			}
		}

		public void JoinNextIfBackground(int i, int count = 0)
		{
			if (count >= 255)
			{
				return;
			}
			var mang = engine.worldManager;
			if (mang.worlds[i].Focus == World.World.FocusLevel.Background)
			{
				mang.worlds[i].Focus = World.World.FocusLevel.Focused;
			}
			else
			{
				if (i + 1 == mang.worlds.Count)
				{
					JoinNextIfBackground(0, count + 1);
				}
				else
				{
					JoinNextIfBackground(i + 1, count + 1);
				}
			}

		}

		public PersonalSpace(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public PersonalSpace()
		{
		}
	}
}
