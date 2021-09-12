using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
	public class SyncDelegate : SyncDelegate<Action>
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

		private T _delegateTarget;

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public T Target
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {
			get
			{
				if (base.Target != null && base.Target.IsRemoved)
				{
					return null;
				}
				return _delegateTarget;
			}
			set
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
					base.Target = null;
				}
				else
				{
                    if (@delegate.Target is not IWorldObject worldObject)
                    {
                        throw new Exception("Delegate doesn't belong to a WorldObject");
                    }
                    if (worldObject.World != world)
					{
						throw new Exception("Delegate owner belongs to a different world");
					}
					_type = @delegate.Method.DeclaringType;
					_method = @delegate.Method.Name;
					base.Target = worldObject;
				}
				_delegateTarget = value;
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
			data.SetValue("Method", new DataNode<string>(_method));
			if (_type != null)
			{
				data.SetValue("Type", new DataNode<string>(_type.FullName));
			}
			else
			{
				data.SetValue("Type", new DataNode<string>(""));
			}
		}

		public override void ReceiveDataIngect(DataNodeGroup data)
		{
			_method = ((DataNode<string>)data.GetValue("Method")).Value;
			var hello = ((DataNode<string>)data.GetValue("Type")).Value;
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
			if (_type == null || _method == "" || _method == null || base.Target == null)
            {
                return;
            }

            try
			{
				var _delegate = Delegate.CreateDelegate(typeof(T), base.Target, _method, false, true);
				_delegateTarget = _delegate as T;
			}
			catch (Exception e)
			{
				logger.Log($"Failed To load Delegate Type {_type}  Method {_method} Error" + e.ToString());
				_type = null;
				_method = "";
				base.Target = null;
				_delegateTarget = null;
			}
		}

		public SyncDelegate()
		{

		}

		public SyncDelegate(IWorldObject _parent, bool newrefid = true) : base(_parent, newrefid)
		{

		}

	}
}
