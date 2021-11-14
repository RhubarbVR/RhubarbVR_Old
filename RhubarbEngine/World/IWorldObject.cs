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
				var (cube, _, comp) = Helpers.MeshHelper.AttachWindow<Components.ImGUI.WorkerObserver>(par);
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
				return worldObject.Parent?.GetClosedWorker(allowSyncVals);
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
				return worldObject.Parent?.GetClosedEntity();
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
				return worldObject.Parent?.GetClosedUser();
			}
		}

		public static string GetNameString(this IWorldObject worldObject)
		{
			return worldObject?.GetClosedEntity()?.name.Value ?? worldObject?.GetClosedUser()?.username.Value ?? worldObject?.GetType().Name ?? "null";
		}
	}

	public interface IWorldObject : IDisposable
	{
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

