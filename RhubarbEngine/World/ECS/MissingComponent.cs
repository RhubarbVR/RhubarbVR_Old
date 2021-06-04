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
    [Category(new string[] { "International" })]
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
            type.value = temptype;
        }
        public MissingComponent(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public MissingComponent()
        {
        }

        public virtual DataNodeGroup serialize()
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            DataNodeGroup obj = new DataNodeGroup();
            if (Persistent)
            {
                foreach (var field in fields)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(field.FieldType))
                    {
                        obj.setValue(field.Name, ((IWorldObject)field.GetValue(this)).serialize());
                    }
                }
                DataNode<NetPointer> Refid = new DataNode<NetPointer>(referenceID);
                obj.setValue("referenceID", Refid);
                obj.setValue("Data", tempdata);
            }
            return obj;
        }

        public virtual void deSerialize(DataNodeGroup data, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default(Dictionary<ulong, ulong>), Dictionary<ulong, RefIDResign> latterResign = default(Dictionary<ulong, RefIDResign>))
        {
            if (data == null)
            {
                world.worldManager.engine.logger.Log("Node did not exsets When loading Node");
                return;
            }
            if (NewRefIDs)
            {
                newRefID.Add(((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID(), referenceID.getID());
                latterResign[((DataNode<NetPointer>)data.getValue("referenceID")).Value.getID()](referenceID.getID());
            }
            else
            {
                referenceID = ((DataNode<NetPointer>)data.getValue("referenceID")).Value;
                world.addWorldObj(this);
            }
            if (((DataNode<string>)data.getValue("type")) != null)
            {
                temptype = ((DataNode<string>)data.getValue("type")).Value;
                tempdata = ((DataNodeGroup)data.getValue("Data"));
            }
            else
            {
                tempdata = data;
            }
            onLoaded();
        }

    }
}
