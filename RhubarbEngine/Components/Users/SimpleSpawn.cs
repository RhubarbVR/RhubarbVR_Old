using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using g3;
using System.Numerics;
using RhubarbEngine.Input;
using RhubarbEngine.Components.Assets.Procedural_Meshes;

namespace RhubarbEngine.Components.Users
{
    [Category(new string[] { "Users" })]
    public class SimpleSpawn : Component
    {
        public static string ToHexString(ulong ouid)
        {
            string temp = BitConverter.ToString(BitConverter.GetBytes(ouid).Reverse().ToArray()).Replace("-", "");

            while (temp.Substring(0, 1) == "0")
            {
                temp = temp.Substring(1);
            }

            return temp;
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if(!world.userLoaded)
            {
                return;
            }
            if(world.localUser.userroot.target == null)
            {
                Entity rootent = world.RootEntity.addChild();
                rootent.name.value = $"{world.localUser.username} (ID:{ToHexString(world.localUser.referenceID.id)})";
                rootent.persistence.value = false;
                UserRoot userRoot = rootent.attachComponent<UserRoot>();
                userRoot.user.target = world.localUser;
                world.localUser.userroot.target = userRoot;
                Entity head = rootent.addChild("Head");
                head.attachComponent<Head>().userroot.target = userRoot;
                userRoot.Head.target = head;
                Entity left = rootent.addChild("Left hand");
                Entity right = rootent.addChild("Right hand");
                userRoot.LeftHand.target = left;
                userRoot.RightHand.target = right;
                Hand leftcomp = left.attachComponent<Hand>();
                leftcomp.userroot.target = userRoot;
                leftcomp.creality.value = Input.Creality.Left;
                Hand rightcomp = right.attachComponent<Hand>();
                rightcomp.creality.value = Input.Creality.Right;
                rightcomp.userroot.target = userRoot;
                Entity obj = world.worldManager.AddMesh<ArrowMesh>(left);
                Entity obj2 = world.worldManager.AddMesh<ArrowMesh>(right);
                Entity obj3 = world.worldManager.AddMesh<ArrowMesh>(head);

                obj.scale.value = new Vector3f(0.2f);
                obj2.scale.value = new Vector3f(0.2f);
                obj.rotation.value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
                obj2.rotation.value = Quaternionf.CreateFromYawPitchRoll(0.0f, -90.0f, 0.0f);
                logger.Log("SpawnedUser");
            }
        }

        public override void buildSyncObjs(bool newRefIds)
        {
        }

        public SimpleSpawn(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public SimpleSpawn()
        {
        }
    }
}