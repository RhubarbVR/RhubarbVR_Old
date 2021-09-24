using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using RhubarbEngine.Render.Shader.Vals;
using RNumerics;

namespace RhubarbEngine.Render.Material.Fields
{
	public class Dvec3Field : GenericField<Vector3d>
	{

		public override void CreateDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription(192, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
		unsafe public override void UpdateBuffer(GraphicsDevice gb)
		{
			gb.UpdateBuffer((DeviceBuffer)resource, 0, new Val_dvec3(field.Value));
		}
	}
}
