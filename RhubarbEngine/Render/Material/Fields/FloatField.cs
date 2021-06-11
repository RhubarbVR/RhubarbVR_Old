using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace RhubarbEngine.Render.Material.Fields
{
    public class FloatField: GenericField<float>
    {

        public override void createDeviceResource(ResourceFactory fact)
        {
            if(resource != null)
            {
                return;
            }
            resource = fact.CreateBuffer(new BufferDescription(4, BufferUsage.Dynamic));
        }
    }
}
