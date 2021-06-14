using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using RhubarbEngine.Render.Shader.Vals;
using g3;

namespace RhubarbEngine.Render.Material.Fields
{
    public class ColorField : GenericField<Colorf>
    {

        public override void createDeviceResource(ResourceFactory fact)
        {
            if (resource != null)
            {
                return;
            }
            resource = fact.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }
        unsafe public override void updateBuffer(GraphicsDevice gb)
        {
            gb.UpdateBuffer((DeviceBuffer)resource, 0, new Val_color(field.value));
        }
    }
}
