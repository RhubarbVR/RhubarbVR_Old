using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using System.Numerics;
using RNumerics;
using System.Reflection;

namespace RhubarbEngine.World
{
    public static class WorldObjectHelper
    {
        public static void OpenWindow(this IWorldObject worldObject)
        {
            var worker = worldObject.GetClosedWorker();
            if (worker != null)
            {
                var createWorld = worldObject.World.worldManager.FocusedWorld ?? worldObject.World;
                var User = createWorld.UserRoot.Entity;
                var par = User.parent.Target;
                var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<Components.ImGUI.WorkerProperties>(par);
                var headPos = createWorld.UserRoot.Headpos;
                var move = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateTranslation(new Vector3(0, 2, 0.5f)) * Matrix4x4.CreateFromQuaternion(Quaternionf.CreateFromEuler(0f, -90f, 0f).ToSystemNumric());
                cube.SetGlobalTrans(move * headPos);
                comp.target.Target = worker;
            }
        }

        public static IWorker GetClosedWorker(this IWorldObject worldObject, bool allowSyncVals = false)
        {
            allowSyncVals = allowSyncVals || worldObject.GetType().IsAssignableTo(typeof(UserStream));
            try
            {
                return allowSyncVals
                    ? (IWorker)worldObject
                    : typeof(ISyncMember).IsAssignableFrom(worldObject.GetType())
                        ? (worldObject.Parent?.GetClosedWorker(allowSyncVals))
                        : (IWorker)worldObject;
            }
            catch
            {
                return worldObject?.Parent?.GetClosedWorker(allowSyncVals);
            }
        }

        public static Entity GetClosedEntity(this IWorldObject worldObject)
        {
            try
            {
                return (Entity)worldObject;
            }
            catch
            {
                return worldObject?.Parent?.GetClosedEntity();
            }
        }

        public static User GetClosedUser(this IWorldObject worldObject)
        {
            try
            {
                return (User)worldObject;
            }
            catch
            {
                return worldObject?.Parent?.GetClosedUser();
            }
        }

        public static Component GetClosedComponent(this IWorldObject worldObject)
        {
            try
            {
                return (Component)worldObject;
            }
            catch
            {
                return worldObject?.Parent?.GetClosedComponent();
            }
        }

        public static T GetClosedGeneric<T>(this IWorldObject worldObject) where T : class, IWorldObject
        {
            try
            {
                return (T)worldObject;
            }
            catch
            {
                return worldObject?.Parent?.GetClosedGeneric<T>();
            }
        }

        public static string GetNameString(this IWorldObject worldObject)
        {
            return worldObject?.GetClosedEntity()?.name.Value ?? worldObject?.GetClosedUser()?.username.Value ?? worldObject?.GetType().Name ?? "null";
        }


        public static string GetExtendedNameString(this IWorldObject worldObject)
        {
            var comp = worldObject.GetClosedComponent();
            if (comp is null || comp == worldObject)
            {
                return worldObject?.GetClosedEntity()?.name.Value ?? worldObject?.GetClosedUser()?.username.Value ?? worldObject?.GetType().Name ?? "null";
            }
            else
            {
                return $"{comp.GetType().GetFormattedName()} attached to " + (worldObject?.GetClosedEntity()?.name.Value ?? worldObject?.GetClosedUser()?.username.Value ?? worldObject?.GetType().Name ?? "null");
            }
        }

        public static string GetFieldName(this IWorldObject worldObject)
        {
            if(worldObject?.Parent is null)
            {
                return " NUll";
            }

            if (worldObject.Parent.GetType().IsAssignableTo(typeof(ISyncList)))
            {
                var index = ((ISyncList)worldObject.Parent).IndexOf(worldObject);
                if(index != -1)
                {
                    return " Index:" + index.ToString();
                }
            }
            if (worldObject.Parent.GetType().IsAssignableTo(typeof(Render.Material.Fields.MaterialField)))
            {
                return ((Render.Material.Fields.MaterialField)worldObject.Parent).fieldName.Value;
            }
            var fields = worldObject.Parent.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.GetValue(worldObject.Parent) == worldObject)
                {
                    return " "+field.Name;
                }
            }
            return "";
        }
    }

	public interface IWorldObject : IDisposable
	{
        public bool IsParentDisposed { get; }

		NetPointer ReferenceID { get; }

		World World { get; }

		void AddDisposable(IDisposable val);

		void RemoveDisposable(IDisposable val);

		IWorldObject Parent { get; }

		bool IsLocalObject { get; }

		bool IsPersistent { get; }

		bool IsRemoved { get; }
		DataNodeGroup Serialize(WorkerSerializerObject serializerObject);

		void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default);

	}
}

