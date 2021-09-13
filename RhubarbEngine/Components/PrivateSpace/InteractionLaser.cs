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
using RhubarbEngine.Components.Physics.Colliders;
using BulletSharp;
using BulletSharp.Math;
using RNumerics;
using Veldrid;
using RhubarbEngine.Helpers;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Components.PrivateSpace
{
	public class InteractionLaser : Component
	{
		public Sync<InteractionSource> source;

		public Sync<Vector3f> rayderection;

		public Sync<float> distances;

		public Driver<Quaternionf> rotation;

		public override void OnAttach()
		{
			base.OnAttach();
			if (!World.Userspace)
            {
                return;
            }

            rotation.SetDriveTarget(Entity.rotation);
		}


		public override void BuildSyncObjs(bool newRefIds)
		{
            source = new Sync<InteractionSource>(this, newRefIds)
            {
                Value = InteractionSource.HeadLaser
            };
            rayderection = new Sync<Vector3f>(this, newRefIds)
            {
                Value = Vector3f.AxisZ
            };
            distances = new Sync<float>(this, newRefIds)
            {
                Value = 25f
            };
            rotation = new Driver<Quaternionf>(this, newRefIds);
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (!World.Userspace)
            {
                return;
            }

            if (((Engine.outputType == VirtualReality.OutputType.Screen) && (source.Value != InteractionSource.HeadLaser)))
            {
                return;
            }

            if (((Engine.outputType != VirtualReality.OutputType.Screen) && (source.Value == InteractionSource.HeadLaser)))
            {
                return;
            }

            try
			{
				if (source.Value == InteractionSource.HeadLaser)
				{
					var mousepos = Engine.inputManager.mainWindows.MousePosition;
					var size = new System.Numerics.Vector2(Engine.windowManager.mainWindow.width, Engine.windowManager.mainWindow.height);
					var x = (2.0f * mousepos.X / size.X) - 1.0f;
					var y = (2.0f * mousepos.Y / size.Y) - 1.0f;
					var ar = size.X / size.Y;
					var tan = (float)Math.Tan(Engine.settingsObject.RenderSettings.DesktopRenderSettings.fov * Math.PI / 360);
					var vectforward = new Vector3f(-x * tan * ar, y * tan, 1);
					var vectup = new Vector3f(0, 1, 0);
					Entity.rotation.Value = Quaternionf.LookRotation(vectforward, vectup);
				}
				System.Numerics.Matrix4x4.Decompose(Entity.GlobalTrans(), out var vsg, out var vrg, out var global);
				var sourcse = new Vector3(global.X, global.Y, global.Z);
				var val = Entity.GlobalRot().AxisZ;
				var destination = new Vector3(-val.x, -val.y, -val.z);
				switch (source.Value)
				{
					case InteractionSource.LeftLaser:
						Input.LeftLaser.SendRayCast(sourcse, destination);
						break;
					case InteractionSource.RightLaser:
						Input.RightLaser.SendRayCast(sourcse, destination);
						break;
					case InteractionSource.HeadLaser:
						Input.RightLaser.SendRayCast(sourcse, destination);
						break;
					default:
						break;
				}
			}
			catch (Exception e)
			{
				Logger.Log("Error With Interaction :" + e.ToString(), true);
			}

		}

		public InteractionLaser(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public InteractionLaser()
		{
		}
	}
}
