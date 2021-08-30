using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using System.Numerics;
using g3;

namespace RhubarbEngine.World
{
    public static class WorldObjectHelper
    {
        public static void openWindow(this IWorldObject worldObject)
        {
            var worker = worldObject.getClosedWorker();
            if (worker != null) {
                World createWorld = worldObject.World.worldManager.focusedWorld ?? worldObject.World;
                Entity User = createWorld.userRoot.entity;
                Entity par = User.parent.target;
                var (cube, win, comp) = Helpers.MeshHelper.attachWindow<Components.ImGUI.WorkerObserver>(par);
                var headPos = createWorld.userRoot.Headpos;
                var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
                cube.setGlobalTrans(move * headPos);
                comp.target.target = worker;
            }
        }

        public static Worker getClosedWorker(this IWorldObject worldObject,bool allowSyncVals = false)
        {
            try
            {
                if (allowSyncVals)
                {
                    return (Worker)worldObject;
                }
                else
                {
                    if (typeof(ISyncMember).IsAssignableFrom(worldObject.GetType()))
                    {
                        return worldObject.Parent?.getClosedWorker(allowSyncVals);
                    }
                    else
                    {
                        return (Worker)worldObject;
                    }
                }
            }
            catch
            {
                return worldObject.Parent?.getClosedWorker(allowSyncVals);
            }
        }

        public static Entity getClosedEntity(this IWorldObject worldObject)
        {
            try
            {
                return (Entity)worldObject;
            }
            catch
            {
                return worldObject.Parent?.getClosedEntity();
            }
        }

        public static User getClosedUser(this IWorldObject worldObject)
        {
            try
            {
                return (User)worldObject;
            }
            catch
            {
                return worldObject.Parent?.getClosedUser();
            }
        }

        public static string getNameString(this IWorldObject worldObject)
        {
            return worldObject?.getClosedEntity()?.name.value ?? worldObject?.getClosedUser()?.username.value ?? worldObject?.GetType().Name??"null";
        }
    }

    public interface IWorldObject: IDisposable
    {
        NetPointer ReferenceID { get; }

        World World { get; }

        void addDisposable(IDisposable val);

        void removeDisposable(IDisposable val);

        IWorldObject Parent { get; }

        bool IsLocalObject { get; }

        bool IsPersistent { get; }

        bool IsRemoved { get; }
        DataNodeGroup serialize(bool netsync = false);

        void deSerialize( DataNodeGroup data , List<Action> onload = default(List<Action>), bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, List<RefIDResign>> latterResign = default(Dictionary<ulong, List<RefIDResign>>));

    }
}

