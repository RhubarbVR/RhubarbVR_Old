using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseR;

namespace RhubarbEngine.World.ECS
{
    public class Entity: Worker
    {
        public Sync<Vector3> position;

        public Sync<Quaternion> rotation;

        public Sync<Vector3> scale;

        public Sync<string> name;

        public override void buildSyncObjs(bool newRefIds)
        {
            position = new Sync<Vector3>(this, newRefIds);
            scale = new Sync<Vector3>(this, newRefIds);
            scale.value = Vector3.One;
            rotation = new Sync<Quaternion>(this, newRefIds);
            name = new Sync<string>(this, newRefIds);
        }

        public Entity(IWorldObject _parent):base(_parent.World, _parent)
        {

        }
    }
}
