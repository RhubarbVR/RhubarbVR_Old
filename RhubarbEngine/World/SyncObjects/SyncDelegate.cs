using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public class SyncDelegate: SyncDelegate<Action>
    {
        public SyncDelegate()
        {

        }

        public SyncDelegate(IWorldObject _parent, bool newrefid = true) : base(_parent, newrefid)
        {

        }
    }

    public class SyncDelegate<T> : SyncRef<IWorldObject> where T : class
    {
        private string _method;

        private Type _type;

        private T _DelegateTarget;

        public T Target { 
            get {
                if (base.target != null && base.target.IsRemoved)
                {
                    return null;
                }
                return _DelegateTarget;
            } set
            {
                if (value == Target)
                {
                    return;
                }
                Delegate @delegate = value as Delegate;
                if (@delegate.Target == null)
                {
                    _type = @delegate.Method.DeclaringType;
                    _method = @delegate.Method.Name;
                    base.target = null;
                }
                else
                {
                    IWorldObject worldObject = @delegate.Target as IWorldObject;
                    if (worldObject == null)
                    {
                        throw new Exception("Delegate doesn't belong to a WorldObject");
                    }
                    if (worldObject.World != world)
                    {
                        throw new Exception("Delegate owner belongs to a different world");
                    }
                    _type = @delegate.Method.DeclaringType;
                    _method = @delegate.Method.Name;
                    base.target = worldObject;
                }
                _DelegateTarget = value;
            }
        }
        public override void Change()
        {
            BuildDelegate();
        }

        public override void UpdateNetIngect(DataNodeGroup data)
        {
            data.setValue("Method", new DataNode<string>(_method));
            data.setValue("Type", new DataNode<string>(_type.Name));
        }

        public override void ReceiveDataIngect(DataNodeGroup data)
        {
            _method = ((DataNode<string>)data.getValue("Method")).Value;
            _type = Type.GetType(((DataNode<string>)data.getValue("Type")).Value);
            BuildDelegate();
        }

        public void BuildDelegate()
        {
            _DelegateTarget = (T)(object)Delegate.CreateDelegate(typeof(T), _type, _method);
        }

        public SyncDelegate()
        {

        }

        public SyncDelegate(IWorldObject _parent,bool newrefid = true) : base( _parent, newrefid)
        {

        }

    }
}
