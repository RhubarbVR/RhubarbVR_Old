using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World.ECS
{
	[Category(new string[] { "Internal" })]
	public class MissingComponent : Component
	{
		public Sync<string> type;

		public string temptype;

		public DataNodeGroup tempdata;
		public override void buildSyncObjs(bool newRefIds)
		{
			type = new Sync<string>(this, newRefIds);
		}

		public override void onLoaded()
		{
			type.Value = temptype;
		}
		public MissingComponent(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MissingComponent()
		{
		}

		public virtual DataNodeGroup Serialize()
		{
			var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var obj = new DataNodeGroup();
			if (Persistent)
			{
				foreach (var field in fields)
				{
					if (typeof(IWorldObject).IsAssignableFrom(field.FieldType))
					{
						obj.SetValue(field.Name, ((IWorldObject)field.GetValue(this)).Serialize());
					}
				}
				var Refid = new DataNode<NetPointer>(referenceID);
				obj.SetValue("referenceID", Refid);
				obj.SetValue("Data", tempdata);
			}
			return obj;
		}

		public virtual void DeSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, RefIDResign> latterResign = default)
		{
			if (data == null)
			{
				world.worldManager.engine.logger.Log("Node did not exsets When loading Node");
				return;
			}
			if (NewRefIDs)
			{
				newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID(), referenceID.getID());
				latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()](referenceID.getID());
			}
			else
			{
				referenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				world.AddWorldObj(this);
			}
			if (((DataNode<string>)data.GetValue("type")) != null)
			{
				temptype = ((DataNode<string>)data.GetValue("type")).Value;
				tempdata = ((DataNodeGroup)data.GetValue("Data"));
			}
			else
			{
				tempdata = data;
			}
			onLoaded();
		}

	}
}
