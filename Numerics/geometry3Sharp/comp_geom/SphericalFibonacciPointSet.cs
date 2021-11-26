using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RNumerics
{
	/// <summary>
	/// A Spherical Fibonacci Point Set is a set of points that are roughly evenly distributed on
	/// a sphere. Basically the points lie on a spiral, see pdf below.
	/// The i-th SF point of an N-point set can be calculated directly.
	/// For a given (normalized) point P, finding the nearest SF point (ie mapping back to i)
	/// can be done in constant time.
	/// 
	/// math from http://lgdv.cs.fau.de/uploads/publications/spherical_fibonacci_mapping_opt.pdf
	/// </summary>
	public class SphericalFibonacciPointSet
	{
		public int N = 64;

		public SphericalFibonacciPointSet(int n = 64)
		{
			N = n;
		}


		public int Count { get { return N; } }


		/// <summary>
		/// Compute i'th spherical point
		/// </summary>
		public Vector3d Point(int i)
		{
			Util.gDevAssert(i < N);
			var div = (double)i / _pHI;
			var phi = MathUtil.TwoPI * (div - Math.Floor(div));
            double cos_phi = Math.Cos(phi), sin_phi = Math.Sin(phi);

            var z = 1.0 - (((2.0 * i) + 1.0) / (double)N);
			var theta = Math.Acos(z);
			var sin_theta = Math.Sin(theta);

			return new Vector3d(cos_phi * sin_theta, sin_phi * sin_theta, z);
		}
		public Vector3d this[int i]
		{
			get { return Point(i); }
		}


		/// <summary>
		/// Find index of nearest point-set point for input arbitrary point
		/// </summary>
		public int NearestPoint(Vector3d p, bool bIsNormalized = false)
		{
			if (bIsNormalized)
            {
                return InverseSF(ref p);
            }

            p.Normalize();
			return InverseSF(ref p);
		}




        static readonly double _pHI = (Math.Sqrt(5.0) + 1.0) / 2.0;

        static double Madfrac(double a, double b)
		{
            //#define madfrac(A,B) mad((A),(B),-floor((A)*(B)))
            return (a * b) + -Math.Floor(a * b);
		}

		/// <summary>
		/// This computes mapping from p to i. Note that the code in the original PDF is HLSL shader code.
		/// I have ported here to comparable C# functions. *However* the PDF also explains some assumptions
		/// made about what certain operators return in different cases (particularly NaN handling).
		/// I have not yet tested these cases to make sure C# behavior is the same (not sure when they happen).
		/// </summary>
		int InverseSF(ref Vector3d p)
		{
			var phi = Math.Min(Math.Atan2(p.y, p.x), Math.PI);
			var cosTheta = p.z;
			var k = Math.Max(2.0, Math.Floor(
                Math.Log(N * Math.PI * Math.Sqrt(5.0) * (1.0 - (cosTheta * cosTheta))) / Math.Log(_pHI * _pHI)));
			var Fk = Math.Pow(_pHI, k) / Math.Sqrt(5.0);

			//double F0 = round(Fk), F1 = round(Fk * PHI);
			double F0 = Math.Round(Fk), F1 = Math.Round(Fk * _pHI);

			var B = new Matrix2d(
                (2 * Math.PI * Madfrac(F0 + 1, _pHI - 1)) - (2 * Math.PI * (_pHI - 1)),
                (2 * Math.PI * Madfrac(F1 + 1, _pHI - 1)) - (2 * Math.PI * (_pHI - 1)),
				-2 * F0 / N, -2 * F1 / N);
			var invB = B.Inverse();

            //Vector2d c = floor(mul(invB, double2(phi, cosTheta - (1 - 1.0/N))));
            var c = new Vector2d(phi, cosTheta - (1 - (1.0 / N)));
			c = invB * c;
			c.x = Math.Floor(c.x);
			c.y = Math.Floor(c.y);

			double d = double.PositiveInfinity, j = 0;
			for (uint s = 0; s < 4; ++s)
			{
				var cosTheta_second = new Vector2d(s % 2, s / 2) + c;
                cosTheta = B.Row(1).Dot(cosTheta_second) + (1 - (1.0 / N));
                cosTheta = (MathUtil.Clamp(cosTheta, -1.0, +1.0) * 2.0) - cosTheta;
                var i = Math.Floor((N * 0.5) - (cosTheta * N * 0.5));
				phi = 2.0 * Math.PI * Madfrac(i, _pHI - 1);
                cosTheta = 1.0 - (((2.0 * i) + 1.0) * (1.0 / N)); // rcp(n);
                var sinTheta = Math.Sqrt(1.0 - (cosTheta * cosTheta));
				var q = new Vector3d(
					Math.Cos(phi) * sinTheta,
					Math.Sin(phi) * sinTheta,
					cosTheta);
				var squaredDistance = Vector3d.Dot(q - p, q - p);
				if (squaredDistance < d)
				{
					d = squaredDistance;
					j = i;
				}
			}

			// [TODO] should we be clamping this??
			return (int)j;
		}





	}
}
