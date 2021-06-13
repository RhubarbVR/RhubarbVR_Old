using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RhubarbEngine.Render.Shader;
using Veldrid;
namespace RhubarbEngine.Render.Material.Fields
{
    public abstract class MaterialField:Worker
    {
        public Sync<ShaderValueType> valueType;

        public Sync<ShaderType> shaderType;

        public Sync<string> fieldName;

        public BindableResource resource;

        public bool isNull;

        public override void onLoaded()
        {
            base.onLoaded();
            createDeviceResource(engine.renderManager.gd.ResourceFactory);
        }

        public virtual void setValue(Object val)
        {

        }

        public override void inturnalSyncObjs(bool newRefIds)
        {
            valueType = new Sync<ShaderValueType>(this, newRefIds);
            shaderType = new Sync<ShaderType>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }

        public virtual void updateBuffer(GraphicsDevice gb)
        {

        }

        public virtual void createDeviceResource(ResourceFactory fact)
        {
        }
    }
}
