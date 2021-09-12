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
	public class Vec2Field : GenericField<Vector2f>
	{

		public override void createDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
		unsafe public override void updateBuffer(GraphicsDevice gb)
		{
			gb.UpdateBuffer((DeviceBuffer)resource, 0, new Val_vec2(field.Value));
		}
	}
}
