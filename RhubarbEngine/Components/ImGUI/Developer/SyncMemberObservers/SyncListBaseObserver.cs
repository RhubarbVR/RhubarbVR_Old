using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{
	public class SyncListBaseObserver : UIWidget, IPropertiesElement
	{
        public virtual bool Removeable
        {
            get
            {
                return true;
            }
        }

        public Sync<string> fieldName;

		public SyncRef<ISyncList> target;

		public SyncRefList<WorkerProperties> children;

		public SyncRef<Entity> childrenHolder;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<ISyncList>(this, newRefIds);
			target.Changed += Target_Changed;
			fieldName = new Sync<string>(this, newRefIds);
			children = new SyncRefList<WorkerProperties>(this, newRefIds);
			childrenHolder = new SyncRef<Entity>(this, newRefIds);
		}

		public override void Dispose()
		{
			childrenHolder.Target?.Dispose();
            base.Dispose();
        }


        public override void Destroy()
        {
            base.Destroy();
            Entity.Destroy();
        }

        private void Target_Changed(IChangeable obj)
		{
			if (Entity.Manager != World.LocalUser)
            {
                return;
            }

            Bind();

            foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
			if (target.Target == null)
            {
                return;
            }

            if (childrenHolder.Target == null)
			{
				childrenHolder.Target = Entity.AddChild(fieldName.Value + "Holder");
			}
			var index = 0;
			foreach (var item in target.Target)
			{
				if (typeof(IWorker).IsAssignableFrom(item.GetType()))
				{
					var obs = Entity.AddChild(fieldName.Value + $":{index}").AttachComponent<WorkerProperties>();
					obs.fieldName.Value = index.ToString();
					obs.target.Target = (IWorker)item;
					children.Add().Target = obs;
				}
				index++;
			}
        }
        [NoSave]
        [NoShow]
        [NoSync]
        private ISyncList _lastTarget;

        private bool _bound;

        private void Bind()
        {
            if (_bound)
            {
                UnBind();
            }
            _lastTarget = target.Target;
            if(target.Target is not null)
            {
                target.Target.ElementAdded += Target_ElementAdded;
                target.Target.ElementRemoved += Target_ElementRemoved;
                target.Target.ClearElements += Target_ClearElements;
            }
            _bound = true;
        }

        private void Target_ClearElements()
        {
            foreach (var item in children)
            {
                item.Target?.Dispose();
            }
            children.Clear();
        }

        private void Target_ElementRemoved(IWorker arg1, int arg2)
        {
            children[arg2].Target?.Dispose();
            children.Remove(arg2);
        }

        private void Target_ElementAdded(IWorker obj)
        {
            if (typeof(IWorker).IsAssignableFrom(obj.GetType()))
            {
                var obs = Entity.AddChild(fieldName.Value + $":{target.Target.Count() - 1}").AttachComponent<WorkerProperties>();
                obs.fieldName.Value = (target.Target.Count() - 1).ToString();
                obs.target.Target = obj;
                children.Add().Target = obs;
            }
        }

        private void UnBind()
        {
            if (_bound)
            {
                if(_lastTarget is not null)
                {
                    _lastTarget.ElementAdded -= Target_ElementAdded;
                    _lastTarget.ElementRemoved -= Target_ElementRemoved;
                    _lastTarget.ClearElements -= Target_ClearElements;
                }
                _bound = false;
            }
        }
        public SyncListBaseObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SyncListBaseObserver()
		{
		}

		public virtual void ChildRender(int index, ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (children[index].Target != null)
			{
				children[index].Target.ImguiRender(imGuiRenderer, canvas);
				if (Removeable)
				{
					ImGui.SameLine();
					if (ImGui.Button("X##" + ReferenceID.id.ToString()))
					{
						target.Target?.Remove(index);
					}
				}
			}
		}

		public virtual void RenderChildren(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			for (var i = 0; i < children.Count(); i++)
			{
				ChildRender(i, imGuiRenderer, canvas);
			}
		}
	}

}
