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

		public override void BuildSyncObjs(bool newRefIds)
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
                Value = 0.65f
            };
            activationAngle = new Sync<float>(this, newRefIds)
            {
                Value = 125f
            };
            deactivationDistance = new Sync<float>(this, newRefIds)
            {
                Value = 0.15f
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
			targetUser.Target = World.LocalUser;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			_targetPosition = Entity.GlobalPos();
			_targetRotation = Entity.GlobalRot();
		}

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			if (targetUser.Target == World.LocalUser && World.UserRoot != null)
			{
                var HeadPos = World.UserRoot.Head.Target.GlobalPos();
                var HeadRot = World.UserRoot.Head.Target.GlobalRot();
                if (rotateVerticalOnly.Value)
                {
                    var UserEntity = World.UserRoot.Entity;
                    HeadRot = UserEntity.GlobalRotToLocal(HeadRot);
                    var temp = HeadRot * Vector3f.AxisZ;
                    var forward = new Vector3f(temp.x,0,temp.z).Normalized;
                    HeadRot = Quaternionf.LookRotation(forward,Vector3f.AxisY);
                    HeadRot = UserEntity.LocalRotToGlobal(HeadRot);
                }
                var dist = HeadPos.Distance(Entity.GlobalPos());
                var disAngle = HeadRot.Angle(Entity.GlobalRot());
                if (dist >= activationDistance.Value || disAngle >= activationAngle.Value)
                {
                    _activated = true;
                }
                if (dist <= deactivationDistance.Value && disAngle <= deactivationAngle.Value)
                {
                    _activated = false;
                }
                if (_activated)
                {
                    _targetPosition = HeadPos;
                    _targetRotation = HeadRot;
                }
            }
            var pos = Vector3f.Lerp(Entity.GlobalPos(), _targetPosition, (float)(Engine.PlatformInfo.DeltaSeconds * positionSpeed.Value));
            var rot = Quaternionf.CreateFromEuler(0f, 0f, 0f);
            rot.SetToSlerp(Entity.GlobalRot(), _targetRotation, (float)Engine.PlatformInfo.DeltaSeconds * rotationSpeed.Value);
            Entity.SetGlobalTrans(Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(rot.ToSystemNumric()) * Matrix4x4.CreateTranslation(pos.ToSystemNumrics()));
        }

	}
}
