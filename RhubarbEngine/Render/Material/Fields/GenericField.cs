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
			field.Changed += ValueUpdate;
		}
		public override void SetValue(object val)
		{
			field.Value = (T)val;
		}

		public virtual void SetDefault()
		{
			field.Value = default;
		}

		private void ValueUpdate(IChangeable e)
		{
			UpdateBuffer(Engine.RenderManager.Gd);
		}

		unsafe public override void UpdateBuffer(GraphicsDevice gb)
		{
            if(gb is null)
            {
                return;
            }
			var e = GCHandle.ToIntPtr(GCHandle.Alloc(field.Value));
			gb.UpdateBuffer((DeviceBuffer)resource, 0, e, (uint)sizeof(T));
		}

		public unsafe override void CreateDeviceResource(ResourceFactory fact)
		{
			if (resource != null)
			{
				return;
			}
			resource = fact.CreateBuffer(new BufferDescription((uint)sizeof(T), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
		}
	}
}
