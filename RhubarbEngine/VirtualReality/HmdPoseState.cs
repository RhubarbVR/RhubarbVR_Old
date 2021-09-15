using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Veldrid;

namespace RhubarbEngine.VirtualReality
{
	public readonly struct HmdPoseState
	{
		public readonly Matrix4x4 LeftEyeProjection;
		public readonly Matrix4x4 RightEyeProjection;
		public readonly Vector3 LeftEyePosition;
		public readonly Vector3 RightEyePosition;
		public readonly Quaternion LeftEyeRotation;
		public readonly Quaternion RightEyeRotation;

		public HmdPoseState(
			Matrix4x4 leftEyeProjection,
			Matrix4x4 rightEyeProjection,
			Vector3 leftEyePosition,
			Vector3 rightEyePosition,
			Quaternion leftEyeRotation,
			Quaternion rightEyeRotation)
		{
			LeftEyeProjection = leftEyeProjection;
			RightEyeProjection = rightEyeProjection;
			LeftEyePosition = leftEyePosition;
			RightEyePosition = rightEyePosition;
			LeftEyeRotation = leftEyeRotation;
			RightEyeRotation = rightEyeRotation;
		}

		public Vector3 GetEyePosition(VREye eye)
		{
            return eye switch
            {
                VREye.Left => LeftEyePosition,
                VREye.Right => RightEyePosition,
                _ => throw new VeldridException($"Invalid {nameof(VREye)}: {eye}."),
            };
        }

		public Quaternion GetEyeRotation(VREye eye)
		{
            return eye switch
            {
                VREye.Left => LeftEyeRotation,
                VREye.Right => RightEyeRotation,
                _ => throw new VeldridException($"Invalid {nameof(VREye)}: {eye}."),
            };
        }

		public Matrix4x4 CreateView(VREye eye, Matrix4x4 worldpos, Vector3 forward, Vector3 up)
		{
			var E = GetEyeRotation(eye);
			var eyPos = GetEyePosition(eye);
			var eyematrix = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion(E) * Matrix4x4.CreateTranslation(eyPos);
			Matrix4x4.Decompose(eyematrix * worldpos, out _, out var eyeQuat, out var eyePos);
			var forwardTransformed = Vector3.Transform(forward, eyeQuat);
			var upTransformed = Vector3.Transform(up, eyeQuat);
			return Matrix4x4.CreateLookAt(eyePos, eyePos + forwardTransformed, upTransformed);
		}
	}
}
