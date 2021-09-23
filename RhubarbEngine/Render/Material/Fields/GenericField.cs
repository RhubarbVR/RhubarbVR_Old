using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World;
using Veldrid;
using System.Runtime.InteropServices;

namespace RhubarbEngine.Render.Material.Fields
{
	public class GenericField<T> : MaterialField, IWorldObject where T : unmanaged, IConvertible
	{
		public Sync<T> field;
		public override void BuildSyncObjs(bool newRefIds)
		{
			field = new Sync<T>(this, newRefIds);
			SetDefault();
			field.Changed += valueUpdate;
		}
		public override void setValue(Object val)
		{
			field.Value = (T)val;
		}

		public virtual void SetDefault()
		{
			field.Value = default;
		}

		private void valueUpdate(IChangeable e)
		{
			updateBuffer(Engine.RenderManager.gd);
		}

		unsafe public override void updateBuffer(GraphicsDevice gb)
		{
			IntPtr e = GCHandle.ToIntPtr(GCHandle.Alloc(field.Value));
			gb.UpdateBuffer((DeviceBuffer)resource, 0, e, (uint)sizeof(T));
		}

		public unsafe override void createDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription((uint)sizeof(T), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
	}
}
