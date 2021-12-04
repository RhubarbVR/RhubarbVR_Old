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
        readonly bool _netsync = false;
        public WorkerSerializerObject(bool netsync = false)
        {
            _netsync = netsync;
        }

        public static DataNodeGroup CommonSerialize(IWorldObject @object)
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            return obj;
        }
        public static DataNodeGroup CommonRefSerialize(IWorldObject @object, NetPointer target)
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
            if (@object.IsPersistent || _netsync)
            {
                obj = new DataNodeGroup();
                foreach (var field in fields)
                {
                    if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || (_netsync && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))))
                    {
                        try
                        {
                            obj.SetValue(field.Name, ((IWorldObject)field.GetValue(@object)).Serialize(this));
                        }
                        catch
                        {
                            throw new Exception($"Failed To Serialize {@object.GetType()} , Field {field.Name} , Field Type {field.FieldType.GetFormattedName()}");
                        }
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

        public static IDataNode SetValueEnum<T>(T value)
        {
            var ttype = typeof(T);
            if (ttype.GetEnumUnderlyingType() == typeof(int))
            {
                return new DataNode<int>((int)(object)value);
            }
            else if (ttype.GetEnumUnderlyingType() == typeof(uint))
            {
                return new DataNode<uint>((uint)(object)value);
            }
            else if (ttype.GetEnumUnderlyingType() == typeof(byte))
            {
                return new DataNode<byte>((byte)(object)value);
            }
            else if (ttype.GetEnumUnderlyingType() == typeof(sbyte))
            {
                return new DataNode<sbyte>((sbyte)(object)value);
            }
            else if (ttype.GetEnumUnderlyingType() == typeof(long))
            {
                return new DataNode<long>((long)(object)value);
            }
            else if (ttype.GetEnumUnderlyingType() == typeof(ulong))
            {
                return new DataNode<ulong>((ulong)(object)value);
            }
            throw new Exception("Unknone enum type");
        }


        public static DataNodeGroup CommonValueSerialize<T>(IWorldObject @object, T value) where T : IConvertible
        {
            var obj = new DataNodeGroup();
            var Refid = new DataNode<NetPointer>(@object.ReferenceID);
            obj.SetValue("referenceID", Refid);
            var Value = typeof(T).IsEnum ? SetValueEnum(value) : (IDataNode)new DataNode<T>(value);
            obj.SetValue("Value", Value);
            return obj;
        }
    }
}
