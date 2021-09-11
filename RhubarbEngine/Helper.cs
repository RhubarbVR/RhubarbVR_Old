using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;
using System.Numerics;
namespace RhubarbEngine
{
	public static class Helper
	{

		public static float DistanceFromPoint(this BoundingBox boundingBox, Vector3 fomLocalPos)
		{
			var dx = Math.Max(boundingBox.Min.X - fomLocalPos.X, fomLocalPos.X - boundingBox.Max.X);
			var dy = Math.Max(boundingBox.Min.Y - fomLocalPos.Y, fomLocalPos.Y - boundingBox.Max.Y);
			var dz = Math.Max(boundingBox.Min.Z - fomLocalPos.Z, fomLocalPos.Z - boundingBox.Max.Z);
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static string GetFormattedName(this Type type)
		{
			if (type.IsGenericType)
			{
				string genericArguments = type.GetGenericArguments()
									.Select(x => x.Name)
									.Aggregate((x1, x2) => $"{x1}, {x2}");
				return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
					 + $"<{genericArguments}>";
			}
			return type.Name;
		}

		public static string ToHexString(this ulong ouid)
		{
			string temp = BitConverter.ToString(BitConverter.GetBytes(ouid).Reverse().ToArray()).Replace("-", "");

			while (temp.Substring(0, 1) == "0")
			{
				temp = temp.Substring(1);
			}

			return temp;
		}
	}
}
