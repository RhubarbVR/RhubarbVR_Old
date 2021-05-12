using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseR;

namespace RhubarbEngine.World.ECS
{
    public class Entity: Worker<Entity>
    {
        public Sync<Vector3> position;

        public Sync<Quaternion> rotation;

        public Sync<Vector3> scale;

        public Sync<string> name;

        public override void buildSyncObjs()
        {
            position = new Sync<Vector3>(this);
            scale = new Sync<Vector3>(this);
            scale.value = Vector3.One;
            rotation = new Sync<Quaternion>(this);
            name = new Sync<string>(this);
        }

        public Entity(IWorldObject _parent):base(_parent.World, _parent)
        {

        }
    }
}
