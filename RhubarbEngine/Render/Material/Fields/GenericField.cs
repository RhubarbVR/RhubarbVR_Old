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
    public class GenericField<T> : MaterialField where T : unmanaged
    {
        public Sync<T> field;
        public override void buildSyncObjs(bool newRefIds)
        {
            field = new Sync<T>(this, newRefIds);
        }

        unsafe public override void updateBuffer(GraphicsDevice gb)
        {
            IntPtr e = GCHandle.ToIntPtr(GCHandle.Alloc(field.value));
            gb.UpdateBuffer((DeviceBuffer)resource, 0, e , (uint)sizeof(T));
        }

        public unsafe override void createDeviceResource(ResourceFactory fact)
        {
            if (resource != null)
            {
                return;
            }
            resource = fact.CreateBuffer(new BufferDescription((uint)sizeof(T), BufferUsage.Dynamic));
        }
    }
}
