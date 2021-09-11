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
			positionSpeed = new Sync<float>(this, newRefIds);
			positionSpeed.value = 3f;
			rotationSpeed = new Sync<float>(this, newRefIds);
			rotationSpeed.value = 3f;
			activationDistance = new Sync<float>(this, newRefIds);
			activationDistance.value = 0.5f;
			activationAngle = new Sync<float>(this, newRefIds);
			activationAngle.value = 80f;
			deactivationDistance = new Sync<float>(this, newRefIds);
			deactivationDistance.value = 0.1f;
			deactivationAngle = new Sync<float>(this, newRefIds);
			deactivationAngle.value = 10f;
			rotateVerticalOnly = new Sync<bool>(this, newRefIds);
			rotateVerticalOnly.value = true;
			_activated = true;
		}

		public override void OnAttach()
		{
			targetUser.target = world.localUser;
		}

		public override void onLoaded()
		{
			base.onLoaded();
			_targetPosition = entity.globalPos();
			_targetRotation = entity.globalRot();
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (targetUser.target == world.localUser && world.userRoot != null)
			{
				Vector3f a = world.userRoot.Head.target?.globalPos() ?? Vector3f.Zero;
				var temp = world.userRoot.Head.target.rotation.value * Vector3f.AxisZ;
				var HeadFacingDirection = new Vector3f(temp.x, 0, temp.z).Normalized;
				var mat = world.userRoot.entity.globalTrans();
				var e = new Vector3f(mat.M11 * HeadFacingDirection.x + mat.M12 * HeadFacingDirection.y + mat.M13 * HeadFacingDirection.z, mat.M21 * HeadFacingDirection.x + mat.M22 * HeadFacingDirection.y + mat.M23 * HeadFacingDirection.z, mat.M31 * HeadFacingDirection.x + mat.M32 * HeadFacingDirection.y + mat.M33 * HeadFacingDirection.z);
				var headrot = Quaternionf.LookRotation(e, world.userRoot.entity.up);
				Quaternionf a2 = (rotateVerticalOnly.value ? headrot : world.userRoot.Head.target?.globalRot() ?? Quaternionf.Zero);
				Vector3f b = entity.globalPos();
				float num = a.Distance(b);
				Quaternionf b2 = entity.globalRot();
				float num2 = a2.Angle(b2);
				if (num >= activationDistance.value || num2 >= activationAngle.value)
				{
					_activated = true;
				}
				if (num <= deactivationDistance.value && num2 <= deactivationAngle.value)
				{
					_activated = false;
				}
				if (_activated)
				{
					_targetPosition = a;
					_targetRotation = a2;
				}

				Vector3f from = entity.globalPos();
				var pos = Vector3f.Lerp(from, _targetPosition, (float)(engine.platformInfo.deltaSeconds * positionSpeed.value));
				Quaternionf ae = entity.globalRot();
				var rot = Quaternionf.CreateFromEuler(0f, 0f, 0f);
				rot.SetToSlerp(ae, _targetRotation, (float)engine.platformInfo.deltaSeconds * rotationSpeed.value);
				entity.setGlobalTrans((Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(rot.ToSystemNumric()) * Matrix4x4.CreateTranslation(pos.ToSystemNumrics())));
			}
		}

	}
}
