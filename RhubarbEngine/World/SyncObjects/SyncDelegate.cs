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

    public class SyncDelegate<T> : SyncRef<IWorldObject> where T : System.Delegate
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
                Delegate @delegate = value;
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

        public override void Bind()
        {
            base.Bind();
            BuildDelegate();
        }

        public override void UpdateNetIngect(DataNodeGroup data)
        {
            data.setValue("Method", new DataNode<string>(_method));
            if (_type != null)
            {
                data.setValue("Type", new DataNode<string>(_type.FullName));
            }
            else
            {
                data.setValue("Type", new DataNode<string>(""));
            }
        }

        public override void ReceiveDataIngect(DataNodeGroup data)
        {
            _method = ((DataNode<string>)data.getValue("Method")).Value;
            string hello = ((DataNode<string>)data.getValue("Type")).Value;
            if (hello == "")
            {
                _type = null;
            }
            else
            {
                _type = Type.GetType(hello);
            }
            BuildDelegate();
        }

        public void BuildDelegate()
        {
            if (_type == null || _method == "" || _method == null|| base.target == null) return;
            try
            {
                Delegate _delegate = Delegate.CreateDelegate(typeof(T), base.target, _method, false, true);
                _DelegateTarget = _delegate as T;
            }
            catch(Exception e)
            {
                logger.Log($"Failed To load Delegate Type {_type}  Method {_method} Error" + e.ToString());
                _type = null;
                _method = "";
                base.target = null;
                _DelegateTarget = null;
            }
        }

        public SyncDelegate()
        {

        }

        public SyncDelegate(IWorldObject _parent,bool newrefid = true) : base( _parent, newrefid)
        {

        }

    }
}
