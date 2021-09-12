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

namespace RhubarbEngine.Components.Transform
{
	[Category(new string[] { "Transform" })]
	public class UserInterfacePositioner : Component
	{
		public SyncRef<User> targetUser;

		public Sync<bool> rotateVerticalOnly;

		public Sync<float> positionSpeed;

		public Sync<float> rotationSpeed;

		public Sync<float> activationDistance;

		public Sync<float> activationAngle;

		public Sync<float> deactivationDistance;

		public Sync<float> deactivationAngle;

		private bool _activated;

		private Vector3f _targetPosition = Vector3f.Zero;

		private Quaternionf _targetRotation = Quaternionf.Identity;

		public override void buildSyncObjs(bool newRefIds)
		{
			targetUser = new SyncRef<User>(this, newRefIds);
			rotateVerticalOnly = new Sync<bool>(this, newRefIds);
            positionSpeed = new Sync<float>(this, newRefIds)
            {
                Value = 3f
            };
            rotationSpeed = new Sync<float>(this, newRefIds)
            {
                Value = 3f
            };
            activationDistance = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };
            activationAngle = new Sync<float>(this, newRefIds)
            {
                Value = 80f
            };
            deactivationDistance = new Sync<float>(this, newRefIds)
            {
                Value = 0.1f
            };
            deactivationAngle = new Sync<float>(this, newRefIds)
            {
                Value = 10f
            };
            rotateVerticalOnly = new Sync<bool>(this, newRefIds)
            {
                Value = true
            };
            _activated = true;
		}

		public override void OnAttach()
		{
			targetUser.Target = world.LocalUser;
		}

		public override void onLoaded()
		{
			base.onLoaded();
			_targetPosition = entity.GlobalPos();
			_targetRotation = entity.GlobalRot();
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (targetUser.Target == world.LocalUser && world.UserRoot != null)
			{
				var a = world.UserRoot.Head.Target?.GlobalPos() ?? Vector3f.Zero;
				var temp = world.UserRoot.Head.Target.rotation.Value * Vector3f.AxisZ;
				var HeadFacingDirection = new Vector3f(temp.x, 0, temp.z).Normalized;
				var mat = world.UserRoot.entity.GlobalTrans();
				var e = new Vector3f((mat.M11 * HeadFacingDirection.x) + (mat.M12 * HeadFacingDirection.y) + (mat.M13 * HeadFacingDirection.z), (mat.M21 * HeadFacingDirection.x) + (mat.M22 * HeadFacingDirection.y) + (mat.M23 * HeadFacingDirection.z), (mat.M31 * HeadFacingDirection.x) + (mat.M32 * HeadFacingDirection.y) + (mat.M33 * HeadFacingDirection.z));
				var headrot = Quaternionf.LookRotation(e, world.UserRoot.entity.Up);
				var a2 = (rotateVerticalOnly.Value ? headrot : world.UserRoot.Head.Target?.GlobalRot() ?? Quaternionf.Zero);
				var b = entity.GlobalPos();
				var num = a.Distance(b);
				var b2 = entity.GlobalRot();
				var num2 = a2.Angle(b2);
				if (num >= activationDistance.Value || num2 >= activationAngle.Value)
				{
					_activated = true;
				}
				if (num <= deactivationDistance.Value && num2 <= deactivationAngle.Value)
				{
					_activated = false;
				}
				if (_activated)
				{
					_targetPosition = a;
					_targetRotation = a2;
				}

				var from = entity.GlobalPos();
				var pos = Vector3f.Lerp(from, _targetPosition, (float)(engine.platformInfo.deltaSeconds * positionSpeed.Value));
				var ae = entity.GlobalRot();
				var rot = Quaternionf.CreateFromEuler(0f, 0f, 0f);
				rot.SetToSlerp(ae, _targetRotation, (float)engine.platformInfo.deltaSeconds * rotationSpeed.Value);
				entity.SetGlobalTrans((Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(rot.ToSystemNumric()) * Matrix4x4.CreateTranslation(pos.ToSystemNumrics())));
			}
		}

	}
}
