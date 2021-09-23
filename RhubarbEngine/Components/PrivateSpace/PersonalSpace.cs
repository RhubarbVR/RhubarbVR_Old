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
using RNumerics;
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


		public override void BuildSyncObjs(bool newRefIds)
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
			if (Keyboard.Target == null)
			{
				var keyboard = Keyboard.Target = FollowUser.Target.AddChild("Keyboard");
				var e = keyboard;

				var bmesh = e.AttachComponent<CurvedPlaneMesh>();
				bmesh.BottomRadius.Value = Engine.SettingsObject.UISettings.KeyBoardCurve;
				bmesh.TopRadius.Value = Engine.SettingsObject.UISettings.KeyBoardCurve + 10f;
				bmesh.Height.Value = 0.4f;
				bmesh.Width.Value = 0.5f;
				var bmeshcol = e.AttachComponent<MeshInputPlane>();
				var sizePix = new Vector2f(600, 600);
				bmeshcol.mesh.Target = bmesh;
				bmeshcol.FocusedOverride.Value = true;

				bmeshcol.pixelSize.Value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				//e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
				e.rotation.Value = Quaternionf.CreateFromEuler(90f, -90f, -90f) * Quaternionf.CreateFromEuler(0f, -40f, 0f);
				e.position.Value = new Vector3f(0, -0.5, -0.6);
				var mit = e.AttachComponent<RMaterial>();
				var meshRender = e.AttachComponent<MeshRender>();
				var imGUICanvas = e.AttachComponent<ImGUICanvas>();
				imGUICanvas.noKeyboard.Value = true;
				imGUICanvas.scale.Value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				bmeshcol.pixelSize.Value = new Vector2u((uint)sizePix.x, (uint)sizePix.y);
				var imGUIText = e.AttachComponent<ImGUIKeyboard>();
				imGUICanvas.imputPlane.Target = bmeshcol;
				imGUICanvas.element.Target = imGUIText;
				mit.Shader.Target = World.staticAssets.BasicUnlitShader;
				meshRender.Materials.Add().Target = mit;
				meshRender.Mesh.Target = bmesh;
				imGUICanvas.noCloseing.Value = true;
				imGUICanvas.noBackground.Value = true;
				var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
				field.field.Target = imGUICanvas;
			}
			else
			{
				Keyboard.Target.enabled.Value = true;
			}
		}

		public void CloseKeyboard()
		{
			if (Keyboard.Target != null)
			{
				Keyboard.Target.enabled.Value = false;
			}
		}

		public override void OnAttach()
		{
			base.OnLoaded();
			var d = World.RootEntity.AddChild("User Follower");
			FollowUser.Target = d;

			var e = d.AddChild("Main Panel");
			d.AttachComponent<UserInterfacePositioner>();
			e.AttachComponent<TaskBarManager>();

			var rootent = World.RootEntity.AddChild();
			rootent.name.Value = $"PersonalSpace User";
			rootent.persistence.Value = false;
			var userRoot = rootent.AttachComponent<UserRoot>();
			userRoot.user.Target = World.LocalUser;
			World.LocalUser.userroot.Target = userRoot;
			var head = rootent.AddChild("Head");
			head.AttachComponent<Head>();
			head.AddChild("Laser").AttachComponent<InteractionLaser>().source.Value = InteractionSource.HeadLaser;
			userRoot.Head.Target = head;
			var left = rootent.AddChild("Left hand");
			var right = rootent.AddChild("Right hand");


			userRoot.LeftHand.Target = left;
			userRoot.RightHand.Target = right;
			var leftcomp = left.AttachComponent<Hand>();
			leftcomp.userroot.Target = userRoot;
			leftcomp.creality.Value = RhubarbEngine.Input.Creality.Left;
			var rightcomp = right.AttachComponent<Hand>();
			rightcomp.creality.Value = RhubarbEngine.Input.Creality.Right;
			rightcomp.userroot.Target = userRoot;

			var LeftLaser = left.AddChild("Left Laser");
			LeftLaser.AttachComponent<InteractionLaser>().source.Value = InteractionSource.LeftLaser;
			LeftLaser.AttachComponent<LaserVisual>().source.Value = InteractionSource.LeftLaser;
			var RightLaser = right.AddChild("Right Laser");
			RightLaser.AttachComponent<InteractionLaser>().source.Value = InteractionSource.RightLaser;
			RightLaser.AttachComponent<LaserVisual>().source.Value = InteractionSource.RightLaser;

			var leftContext = rootent.AddChild("Left Context");
			var rightContext = rootent.AddChild("Right Context");

			var leftC = leftContext.AttachComponent<ContextMenu>();
			leftC.side.Value = RhubarbEngine.Input.Creality.Left;

			var rightC = rightContext.AttachComponent<ContextMenu>();
			rightC.side.Value = RhubarbEngine.Input.Creality.Right;

			Logger.Log("Spawned User PersonalSpace");
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (Input.IsKeyboardinuse)
            {
                return;
            }

            if ((Input.MainWindows.GetKeyDown(Veldrid.Key.Tab) && (Input.MainWindows.GetKey(Veldrid.Key.AltLeft) || Input.MainWindows.GetKey(Veldrid.Key.AltRight))) || Input.SecondaryPress(RhubarbEngine.Input.Creality.None))
			{
                SwitchWorld();
			}
		}

		public void SwitchWorld()
		{
			var mang = Engine.WorldManager;

			var pos = mang.worlds.IndexOf(mang.FocusedWorld) + 1;
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
			var mang = Engine.WorldManager;
			if (mang.worlds[i].Focus == RhubarbEngine.World.World.FocusLevel.Background)
			{
				mang.worlds[i].Focus = RhubarbEngine.World.World.FocusLevel.Focused;
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
