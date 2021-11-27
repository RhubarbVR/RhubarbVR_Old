using MessagePack;

using System;
using System.Numerics;




namespace RNumerics
{
	// mostly ported from WildMagic5 Wm5Quaternion, from geometrictools.com
	[MessagePackObject]
	public struct Quaternionf : IComparable<Quaternionf>, IEquatable<Quaternionf>, IConvertible
	{
		[Key(0)]
		public float x;
		[Key(1)]
		public float y;
		[Key(2)]
		public float z;
		[Key(3)]
		public float w;
		public unsafe Quaternion ToSystemNumric()
		{
			fixed (Quaternionf* vector3f = &this)
			{
				return *(Quaternion*)vector3f;
			}
		}
		public static unsafe Quaternionf ToRhuNumrics(ref Quaternion value)
		{
			fixed (Quaternion* vector3f = &value)
			{
				return *(Quaternionf*)vector3f;
			}
		}
		public static explicit operator Quaternion(Quaternionf b) => b.ToSystemNumric();

		public static explicit operator Quaternionf(Quaternion b) => ToRhuNumrics(ref b);

		public Quaternionf(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Quaternionf(float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
		public Quaternionf(Quaternionf q2) { x = q2.x; y = q2.y; z = q2.z; w = q2.w; }

		public Quaternionf(Vector3f axis, float AngleDeg)
		{
			x = y = z = 0;
			w = 1;
			SetAxisAngleD(axis, AngleDeg);
		}
		public Quaternionf(Vector3f vFrom, Vector3f vTo)
		{
			x = y = z = 0;
			w = 1;
			SetFromTo(vFrom, vTo);
		}
		public Quaternionf(Quaternionf p, Quaternionf q, float t)
		{
			x = y = z = 0;
			w = 1;
			SetToSlerp(p, q, t);
		}
		public Quaternionf(Matrix3f mat)
		{
			x = y = z = 0;
			w = 1;
			SetFromRotationMatrix(mat);
		}
		[IgnoreMember]
		static public readonly Quaternionf Zero = new Quaternionf(0.0f, 0.0f, 0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Quaternionf Identity = new Quaternionf(0.0f, 0.0f, 0.0f, 1.0f);
		[IgnoreMember]
		public float this[int key]
		{
			get { if (key == 0) return x; else if (key == 1) return y; else if (key == 2) return z; else return w; }
			set { if (key == 0) x = value; else if (key == 1) y = value; else if (key == 2) z = value; else w = value; }

		}

		[IgnoreMember]
		public float LengthSquared
		{
			get { return x * x + y * y + z * z + w * w; }
		}
		[IgnoreMember]
		public float Length
		{
			get { return (float)Math.Sqrt(x * x + y * y + z * z + w * w); }
		}

		public float Normalize(float epsilon = 0)
		{
			float length = Length;
			if (length > epsilon)
			{
				float invLength = 1.0f / length;
				x *= invLength;
				y *= invLength;
				z *= invLength;
				w *= invLength;
			}
			else
			{
				length = 0;
				x = y = z = w = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Quaternionf Normalized
		{
			get { Quaternionf q = new Quaternionf(this); q.Normalize(); return q; }
		}
		public float Dot(Quaternionf q2)
		{
			return x * q2.x + y * q2.y + z * q2.z + w * q2.w;
		}




		public static Quaternionf operator *(Quaternionf a, Quaternionf b)
		{
			float w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
			float x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
			float y = a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z;
			float z = a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x;
			return new Quaternionf(x, y, z, w);
		}


		public static Quaternionf operator -(Quaternionf q1, Quaternionf q2)
		{
			return new Quaternionf(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);
		}

		public static Vector3f operator *(Quaternionf q, Vector3f v)
		{
			//return q.ToRotationMatrix() * v;
			// inline-expansion of above:
			float twoX = 2 * q.x;
			float twoY = 2 * q.y;
			float twoZ = 2 * q.z;
			float twoWX = twoX * q.w;
			float twoWY = twoY * q.w;
			float twoWZ = twoZ * q.w;
			float twoXX = twoX * q.x;
			float twoXY = twoY * q.x;
			float twoXZ = twoZ * q.x;
			float twoYY = twoY * q.y;
			float twoYZ = twoZ * q.y;
			float twoZZ = twoZ * q.z;
			return new Vector3f(
				v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
				v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
				v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY)));
			;
		}

		// so convenient
		public static Vector3d operator *(Quaternionf q, Vector3d v)
		{
			//return q.ToRotationMatrix() * v;
			// inline-expansion of above:
			double twoX = 2 * q.x;
			double twoY = 2 * q.y;
			double twoZ = 2 * q.z;
			double twoWX = twoX * q.w;
			double twoWY = twoY * q.w;
			double twoWZ = twoZ * q.w;
			double twoXX = twoX * q.x;
			double twoXY = twoY * q.x;
			double twoXZ = twoZ * q.x;
			double twoYY = twoY * q.y;
			double twoYZ = twoZ * q.y;
			double twoZZ = twoZ * q.z;
			return new Vector3d(
				v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
				v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
				v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY)));
			;
		}



		/// <summary> Inverse() * v </summary>
		public Vector3f InverseMultiply(ref Vector3f v)
		{
			float norm = LengthSquared;
			if (norm > 0)
			{
				float invNorm = 1.0f / norm;
				float qx = -x * invNorm, qy = -y * invNorm, qz = -z * invNorm, qw = w * invNorm;
				float twoX = 2 * qx;
				float twoY = 2 * qy;
				float twoZ = 2 * qz;
				float twoWX = twoX * qw;
				float twoWY = twoY * qw;
				float twoWZ = twoZ * qw;
				float twoXX = twoX * qx;
				float twoXY = twoY * qx;
				float twoXZ = twoZ * qx;
				float twoYY = twoY * qy;
				float twoYZ = twoZ * qy;
				float twoZZ = twoZ * qz;
				return new Vector3f(
					v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
					v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
					v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY)));
			}
			else
				return Vector3f.Zero;
		}


		/// <summary> Inverse() * v </summary>
		public Vector3d InverseMultiply(ref Vector3d v)
		{
			float norm = LengthSquared;
			if (norm > 0)
			{
				float invNorm = 1.0f / norm;
				float qx = -x * invNorm, qy = -y * invNorm, qz = -z * invNorm, qw = w * invNorm;
				double twoX = 2 * qx;
				double twoY = 2 * qy;
				double twoZ = 2 * qz;
				double twoWX = twoX * qw;
				double twoWY = twoY * qw;
				double twoWZ = twoZ * qw;
				double twoXX = twoX * qx;
				double twoXY = twoY * qx;
				double twoXZ = twoZ * qx;
				double twoYY = twoY * qy;
				double twoYZ = twoZ * qy;
				double twoZZ = twoZ * qz;
				return new Vector3d(
					v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
					v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
					v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY)));
				;
			}
			else
				return Vector3f.Zero;
		}



		// these multiply quaternion by (1,0,0), (0,1,0), (0,0,1), respectively.
		// faster than full multiply, because of all the zeros
		[IgnoreMember]
		public Vector3f AxisX
		{
			get
			{
				float twoY = 2 * y;
				float twoZ = 2 * z;
				float twoWY = twoY * w;
				float twoWZ = twoZ * w;
				float twoXY = twoY * x;
				float twoXZ = twoZ * x;
				float twoYY = twoY * y;
				float twoZZ = twoZ * z;
				return new Vector3f(1 - (twoYY + twoZZ), twoXY + twoWZ, twoXZ - twoWY);
			}
		}
		[IgnoreMember]
		public Vector3f AxisY
		{
			get
			{
				float twoX = 2 * x;
				float twoY = 2 * y;
				float twoZ = 2 * z;
				float twoWX = twoX * w;
				float twoWZ = twoZ * w;
				float twoXX = twoX * x;
				float twoXY = twoY * x;
				float twoYZ = twoZ * y;
				float twoZZ = twoZ * z;
				return new Vector3f(twoXY - twoWZ, 1 - (twoXX + twoZZ), twoYZ + twoWX);
			}
		}
		[IgnoreMember]
		public Vector3f AxisZ
		{
			get
			{
				float twoX = 2 * x;
				float twoY = 2 * y;
				float twoZ = 2 * z;
				float twoWX = twoX * w;
				float twoWY = twoY * w;
				float twoXX = twoX * x;
				float twoXZ = twoZ * x;
				float twoYY = twoY * y;
				float twoYZ = twoZ * y;
				return new Vector3f(twoXZ + twoWY, twoYZ - twoWX, 1 - (twoXX + twoYY));
			}
		}
        public Quaternionf Inverse()
		{
			float norm = LengthSquared;
			if (norm > 0)
			{
				float invNorm = 1.0f / norm;
				return new Quaternionf(
					-x * invNorm, -y * invNorm, -z * invNorm, w * invNorm);
			}
			else
				return Quaternionf.Zero;
		}
		public static Quaternionf Inverse(Quaternionf q)
		{
			return q.Inverse();
		}

		public float Angle(Quaternionf e)
		{
			return (float)Math.Acos(Math.Min(Math.Abs(Dot(e)), 1f)) * 2f * 57.29578f;
		}

		public Matrix3f ToRotationMatrix()
		{
			float twoX = 2 * x;
			float twoY = 2 * y;
			float twoZ = 2 * z;
			float twoWX = twoX * w;
			float twoWY = twoY * w;
			float twoWZ = twoZ * w;
			float twoXX = twoX * x;
			float twoXY = twoY * x;
			float twoXZ = twoZ * x;
			float twoYY = twoY * y;
			float twoYZ = twoZ * y;
			float twoZZ = twoZ * z;
			Matrix3f m = Matrix3f.Zero;
			m[0, 0] = 1 - (twoYY + twoZZ);
			m[0, 1] = twoXY - twoWZ;
			m[0, 2] = twoXZ + twoWY;
			m[1, 0] = twoXY + twoWZ;
			m[1, 1] = 1 - (twoXX + twoZZ);
			m[1, 2] = twoYZ - twoWX;
			m[2, 0] = twoXZ - twoWY;
			m[2, 1] = twoYZ + twoWX;
			m[2, 2] = 1 - (twoXX + twoYY);
			return m;
		}



		public void SetAxisAngleD(Vector3f axis, float AngleDeg)
		{
			double angle_rad = MathUtil.Deg2Rad * AngleDeg;
			double halfAngle = 0.5 * angle_rad;
			double sn = Math.Sin(halfAngle);
			w = (float)Math.Cos(halfAngle);
			x = (float)(sn * axis.x);
			y = (float)(sn * axis.y);
			z = (float)(sn * axis.z);
		}
		public static Quaternionf AxisAngleD(Vector3f axis, float angleDeg)
		{
			return new Quaternionf(axis, angleDeg);
		}
		public static Quaternionf AxisAngleR(Vector3f axis, float angleRad)
		{
			return new Quaternionf(axis, angleRad * MathUtil.Rad2Degf);
		}

		// this function can take non-normalized vectors vFrom and vTo (normalizes internally)
		public void SetFromTo(Vector3f vFrom, Vector3f vTo)
		{
			// [TODO] this page seems to have optimized version:
			//    http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

			// [RMS] not ideal to explicitly normalize here, but if we don't,
			//   output quaternion is not normalized and this causes problems,
			//   eg like drift if we do repeated SetFromTo()
			Vector3f from = vFrom.Normalized, to = vTo.Normalized;
			Vector3f bisector = (from + to).Normalized;
			w = from.Dot(bisector);
			if (w != 0)
			{
				Vector3f cross = from.Cross(bisector);
				x = cross.x;
				y = cross.y;
				z = cross.z;
			}
			else
			{
				float invLength;
				if (Math.Abs(from.x) >= Math.Abs(from.y))
				{
					// V1.x or V1.z is the largest magnitude component.
					invLength = (float)(1.0 / Math.Sqrt(from.x * from.x + from.z * from.z));
					x = -from.z * invLength;
					y = 0;
					z = +from.x * invLength;
				}
				else
				{
					// V1.y or V1.z is the largest magnitude component.
					invLength = (float)(1.0 / Math.Sqrt(from.y * from.y + from.z * from.z));
					x = 0;
					y = +from.z * invLength;
					z = -from.y * invLength;
				}
			}
			Normalize();   // aaahhh just to be safe...
		}
		public static Quaternionf FromTo(Vector3f vFrom, Vector3f vTo)
		{
			return new Quaternionf(vFrom, vTo);
		}
		public static Quaternionf FromToConstrained(Vector3f vFrom, Vector3f vTo, Vector3f vAround)
		{
			float fAngle = MathUtil.PlaneAngleSignedD(vFrom, vTo, vAround);
			return Quaternionf.AxisAngleD(vAround, fAngle);
		}

		public static Quaternionf Slerp(Quaternionf a, Quaternionf b, float lerp)
		{
			if (lerp <= 0f)
			{
				return a;
			}
			if (lerp >= 1f)
			{
				return b;
			}
			float num = a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
			if (num < 0f)
			{
				b = b.Inverse();
				num = 0f - num;
			}
			if (Math.Abs(num - 1f) < Math.Max(1E-06f * Math.Max(Math.Abs(num), Math.Abs(1f)), 5.605194E-45f))
			{
				return b;
			}
			float num2 = (float)Math.Acos(num);
			float num3 = (float)Math.Sqrt(1f - num * num);
			float num4 = (float)Math.Sin((1f - lerp) * num2) / num3;
			float num5 = (float)Math.Sin(lerp * num2) / num3;
			return new Quaternionf(a.x * num4 + b.x * num5, a.y * num4 + b.y * num5, a.z * num4 + b.z * num5, a.w * num4 + b.w * num5);
		}

		public void SetToSlerp(Quaternionf a, Quaternionf b, float lerp)
		{
			var e = Slerp(a, b, lerp);
			x = e.x;
			y = e.y;
			z = e.z;
			w = e.w;

		}


		public void SetFromRotationMatrix(Matrix3f rot)
		{
			// Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
			// article "Quaternion Calculus and Fast Animation".
			Index3i next = new Index3i(1, 2, 0);

			float trace = rot[0, 0] + rot[1, 1] + rot[2, 2];
			float root;

			if (trace > 0)
			{
				// |w| > 1/2, may as well choose w > 1/2
				root = (float)Math.Sqrt(trace + (float)1);  // 2w
				w = ((float)0.5) * root;
				root = ((float)0.5) / root;  // 1/(4w)
				x = (rot[2, 1] - rot[1, 2]) * root;
				y = (rot[0, 2] - rot[2, 0]) * root;
				z = (rot[1, 0] - rot[0, 1]) * root;
			}
			else
			{
				// |w| <= 1/2
				int i = 0;
				if (rot[1, 1] > rot[0, 0])
				{
					i = 1;
				}
				if (rot[2, 2] > rot[i, i])
				{
					i = 2;
				}
				int j = next[i];
				int k = next[j];

				root = (float)Math.Sqrt(rot[i, i] - rot[j, j] - rot[k, k] + (float)1);

				Vector3f quat = new Vector3f(x, y, z);
				quat[i] = ((float)0.5) * root;
				root = ((float)0.5) / root;
				w = (rot[k, j] - rot[j, k]) * root;
				quat[j] = (rot[j, i] + rot[i, j]) * root;
				quat[k] = (rot[k, i] + rot[i, k]) * root;
				x = quat.x;
				y = quat.y;
				z = quat.z;
			}

			Normalize();   // we prefer normalized quaternions...
		}




		public static bool operator ==(Quaternionf a, Quaternionf b)
		{
			return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
		}
		public static bool operator !=(Quaternionf a, Quaternionf b)
		{
			return (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);
		}
		public override bool Equals(object obj)
		{
			return this == (Quaternionf)obj;
		}
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				hash = (hash * 16777619) ^ w.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Quaternionf other)
		{
			if (x != other.x)
				return x < other.x ? -1 : 1;
			else if (y != other.y)
				return y < other.y ? -1 : 1;
			else if (z != other.z)
				return z < other.z ? -1 : 1;
			else if (w != other.w)
				return w < other.w ? -1 : 1;
			return 0;
		}
		public bool Equals(Quaternionf other)
		{
			return (x == other.x && y == other.y && z == other.z && w == other.w);
		}





		public bool EpsilonEqual(Quaternionf q2, float epsilon)
		{
			return (float)Math.Abs(x - q2.x) <= epsilon &&
				   (float)Math.Abs(y - q2.y) <= epsilon &&
				   (float)Math.Abs(z - q2.z) <= epsilon &&
				   (float)Math.Abs(w - q2.w) <= epsilon;
		}

        public static Quaternionf CreateFromYawPitchRoll(Vector3f val) 
        {
            return CreateFromYawPitchRoll(val.x, val.y, val.z);
        }

        /// <summary>
        /// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians, around the Y-axis.</param>
        /// <param name="pitch">The pitch angle, in radians, around the X-axis.</param>
        /// <param name="roll">The roll angle, in radians, around the Z-axis.</param>
        /// <returns></returns>
        public static Quaternionf CreateFromYawPitchRoll(float yaw, float pitch, float roll)
		{
			//  Roll first, about axis the object is facing, then
			//  pitch upward, then yaw to face into the new heading
			float sr, cr, sp, cp, sy, cy;

			float halfRoll = roll * 0.5f;
			sr = (float)Math.Sin(halfRoll);
			cr = (float)Math.Cos(halfRoll);

			float halfPitch = pitch * 0.5f;
			sp = (float)Math.Sin(halfPitch);
			cp = (float)Math.Cos(halfPitch);

			float halfYaw = yaw * 0.5f;
			sy = (float)Math.Sin(halfYaw);
			cy = (float)Math.Cos(halfYaw);

			Quaternionf result;

			result.x = cy * sp * cr + sy * cp * sr;
			result.y = sy * cp * cr - cy * sp * sr;
			result.z = cy * cp * sr - sy * sp * cr;
			result.w = cy * cp * cr + sy * sp * sr;

			return result;
		}


        public static Quaternionf FromToRotation(Vector3f from, Vector3f to)
        {
            var num = from.Dot(to);
            if (!(from == to))
            {
                var b = Vector3f.Zero;
                if (!(from == b))
                {
                    var b2 = Vector3f.Zero;
                    if (!(to == b2))
                    {
                        var float5 = from.Cross(to);
                        if (float5.SqrMagnitude <= 1E-08f && num < 0f)
                        {
                            b = new Vector3f(1f);
                            var float6 = b.Cross(from);
                            if (float6.SqrMagnitude <= 1E-08f)
                            {
                                b = new Vector3f(0f, 1f);
                                float6 = b.Cross(from);
                            }
                            return CreateFromYawPitchRoll(float6.Normalized);
                        }
                        return new Quaternionf(float5.x, float5.y, float5.z, MathF.Sqrt(from.SqrMagnitude * to.SqrMagnitude) + num).Normalized;
                    }
                }
            }
            return Identity;
        }



        public static Quaternionf LookRotation(Vector3f forward, Vector3f up)
		{
			Vector3f b = Vector3f.Zero;
			if (forward == b)
			{
				return Identity;
			}
			Vector3f b2 = forward.Normalized;
			Vector3f a = up.Cross(b2);
			b = Vector3f.Zero;
			if (a == b)
			{
				Quaternionf q = CreateFromEuler(-90f, 0f, 0f);
				a = q * b2;
			}
			else
			{
				a = a.Normalized;
			}
			Vector3f float5 = b2.Cross(a);
			float num = a.x + float5.y + b2.z;
			float num3;
			float num4;
			float num5;
			float num6;
			if (num > 0f)
			{
				float num2 = (float)Math.Sqrt(num + 1f);
				num3 = num2 * 0.5f;
				num2 = 0.5f / num2;
				num4 = (float5.z - b2.y) * num2;
				num5 = (b2.x - a.z) * num2;
				num6 = (a.y - float5.x) * num2;
			}
			else if (a.x >= float5.y && a.x >= b2.z)
			{
				float num7 = (float)Math.Sqrt(1f + a.x - float5.y - b2.z);
				float num8 = 0.5f / num7;
				num4 = 0.5f * num7;
				num5 = (a.y + float5.x) * num8;
				num6 = (a.z + b2.x) * num8;
				num3 = (float5.z - b2.y) * num8;
			}
			else if (float5.y > b2.z)
			{
				float num9 = (float)Math.Sqrt(1f + float5.y - a.x - b2.z);
				float num10 = 0.5f / num9;
				num4 = (float5.x + a.y) * num10;
				num5 = 0.5f * num9;
				num6 = (b2.y + float5.z) * num10;
				num3 = (b2.x - a.z) * num10;
			}
			else
			{
				float num11 = (float)Math.Sqrt(1f + b2.z - a.x - float5.y);
				float num12 = 0.5f / num11;
				num4 = (b2.x + a.z) * num12;
				num5 = (b2.y + float5.z) * num12;
				num6 = 0.5f * num11;
				num3 = (a.y - float5.x) * num12;
			}
			return new Quaternionf(num4, num5, num6, num3);
		}

		public static explicit operator Quaternionf(Vector3f v) => CreateFromEuler(v.x, v.y, v.z);

		public static Quaternionf CreateFromEuler(float yaw, float pitch, float roll)
		{

			return CreateFromYawPitchRoll((float)(Math.PI / 180) * yaw, (float)(Math.PI / 180) * pitch, (float)(Math.PI / 180) * roll);
		}
		public Vector3f getEuler()
		{
			float sqw = w * w;
			float sqx = x * x;
			float sqy = y * y;
			float sqz = z * z;
			float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			float test = x * w - y * z;
			Vector3f v = Vector3f.Zero;

			if (test > 0.4995f * unit)
			{ // singularity at north pole
				v.y = 2f * (float)Math.Atan2(y, x);
				v.x = (float)Math.PI / 2;
				v.z = 0;
				return NormalizeAngles(v * (float)(Math.PI / 180));
			}
			if (test < -0.4995f * unit)
			{ // singularity at south pole
				v.y = -2f * (float)Math.Atan2(y, x);
				v.x = -(float)Math.PI / 2;
				v.z = 0;
				return NormalizeAngles(v * (float)(Math.PI / 180));
			}
			Quaternionf q = new Quaternionf(w, z, x, y);
			v.y = (float)Math.Atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));     // Yaw
			v.x = (float)Math.Asin(2f * (q.x * q.z - q.w * q.y));                             // Pitch
			v.z = (float)Math.Atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));      // Roll
			return NormalizeAngles(v * (float)(Math.PI / 180));
		}
		static Vector3f NormalizeAngles(Vector3f angles)
		{
			angles.x = NormalizeAngle(angles.x);
			angles.y = NormalizeAngle(angles.y);
			angles.z = NormalizeAngle(angles.z);
			return angles;
		}
		static float NormalizeAngle(float angle)
		{
			while (angle > 360)
				angle -= 360;
			while (angle < 0)
				angle += 360;
			return angle;
		}
		public override string ToString()
		{
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
		}
		public string ToString(string fmt)
		{
			return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
	}
}
