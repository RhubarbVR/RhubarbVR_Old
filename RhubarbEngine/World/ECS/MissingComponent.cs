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
		public override void BuildSyncObjs(bool newRefIds)
		{
			type = new Sync<string>(this, newRefIds);
		}

		public override void OnLoaded()
		{
			type.Value = temptype;
		}
		public MissingComponent(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MissingComponent()
		{
		}

		public override DataNodeGroup Serialize(WorkerSerializerObject workerSerializerObject)
		{
			var obj = new DataNodeGroup();
			if (Persistent)
			{
				var Refid = new DataNode<NetPointer>(ReferenceID);
				obj.SetValue("referenceID", Refid);
				obj.SetValue("Data", tempdata);
                var typevalue = new DataNode<string>(type.Value);
                obj.SetValue("type", typevalue);
            }
            return obj;
		}

		public override void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
		{
			if (data == null)
			{
				World.worldManager.Engine.Logger.Log("Node did not exsets When loading Node");
				return;
			}
			if (NewRefIDs)
			{
                if (newRefID == null)
                {
                    Console.WriteLine("Problem With " + GetType().FullName);
                }
                newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID(), ReferenceID.getID());
                if (latterResign.ContainsKey(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()))
                {
                    foreach (var func in latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()])
                    {
                        func(ReferenceID.getID());
                    }
                }
            }
			else
			{
				ReferenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
				World.AddWorldObj(this);
			}
			if (((DataNode<string>)data.GetValue("type")) != null)
			{
				temptype = ((DataNode<string>)data.GetValue("type")).Value;
				tempdata = (DataNodeGroup)data.GetValue("Data");
			}
			else
			{
				tempdata = data;
			}
			OnLoaded();
		}

	}
}
