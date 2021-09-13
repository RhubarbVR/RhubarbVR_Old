using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RhubarbDataTypes;

using RhubarbEngine.World.DataStructure;

namespace RhubarbEngine.World
{
    public class WorkerSerializerObject
    {
        bool netsync = false;
        public WorkerSerializerObject(bool netsync = false)
        {
            this.netsync = netsync;
        }

        public DataNodeGroup CommonSerialize(IWorldObject @object)
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            return obj;
        }
        public DataNodeGroup CommonRefSerialize(IWorldObject @object, NetPointer target)
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            var Value = new DataNode<NetPointer>(target);
            obj.SetValue("targetRefID", Value);
            return obj;
        }

        public DataNodeGroup CommonWorkerSerialize(IWorldObject @object)
        {
            var fields = @object.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            DataNodeGroup obj = null;
            if (@object.IsPersistent || netsync)
            {
                obj = new DataNodeGroup();
                foreach (var field in fields)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || (netsync && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))))
                    {
                        //This is for debug purposes 
                        //if (!netsync)
                        //{
                        //    Console.WriteLine(field.FieldType.FullName + "Name: " + field.Name);
                        //}
                        obj.SetValue(field.Name, ((IWorldObject)field.GetValue(@object)).Serialize(this));
                    }
                }
                var Refid = new DataNode<NetPointer>(@object.ReferenceID);
                obj.SetValue("referenceID", Refid);
            }
            return obj;
        }

        public DataNodeGroup CommonListAbstactSerialize<T>(IWorldObject @object, IEnumerable<T> worldObjects) where T : IWorker
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            var list = new DataNodeList();
            foreach (var val in worldObjects)
            {
                var tip = val.Serialize(this);
                var listobj = new DataNodeGroup();
                if (tip != null)
                {
                    listobj.SetValue("Value", tip);
                }
                //Need To add Constant Type Strings for better compression 
                listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
                list.Add(listobj);
            }
            obj.SetValue("list", list);
            return obj;
        }

        public DataNodeGroup CommonListSerialize(IWorldObject @object, IEnumerable<IWorldObject> worldObjects)
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            var list = new DataNodeList();
            foreach (var val in worldObjects)
            {
                var tip = val.Serialize(this);
                if (tip != null)
                {
                    list.Add(tip);
                }
            }
            obj.SetValue("list", list);
            return obj;
        }

        public DataNodeGroup CommonValueSerialize<T>(IWorldObject @object,T value)where T:IConvertible
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            IDataNode Value;
            if (typeof(T).IsEnum)
            {
                Value = new DataNode<int>((int)(object)value);
            }
            else
            {
                Value = new DataNode<T>(value);
            }
            obj.SetValue("Value", Value);
            return obj;
        }
    }
}
