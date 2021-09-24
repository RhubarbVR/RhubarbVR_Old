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
	public class Vec3Field : GenericField<Vector3f>
	{

		public override void CreateDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription(96, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
		unsafe public override void UpdateBuffer(GraphicsDevice gb)
		{
			gb.UpdateBuffer((DeviceBuffer)resource, 0, new Val_vec3(field.Value));
		}
	}
}
