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
using g3;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{
    public class SyncListBaseObserver : UIWidget, IObserver
    {
        public virtual bool removeable => true;

        public Sync<string> fieldName;

        public SyncRef<ISyncList> target;

        public SyncRefList<WorkerObserver> children;

        public SyncRef<Entity> childrenHolder;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<ISyncList>(this, newRefIds);
            target.Changed += Target_Changed;
            fieldName = new Sync<string>(this, newRefIds);
            children = new SyncRefList<WorkerObserver>(this, newRefIds);
            childrenHolder = new SyncRef<Entity>(this, newRefIds);
        }

        public override void Dispose()
        {
            base.Dispose();
            childrenHolder.target?.Dispose();
        }

        private void Target_Changed(IChangeable obj)
        {
            if (entity.manager != world.localUser) return;
            foreach (var item in children)
            {
                item.target?.Dispose();
            }
            children.Clear();
            if (target.target == null) return;
            if (childrenHolder.target == null)
            {
                childrenHolder.target = entity.addChild(fieldName.value + "Holder");
            }
            int index = 0;
            foreach (var item in target.target)
            {
                if (typeof(Worker).IsAssignableFrom(item.GetType()))
                {
                    var obs = childrenHolder.target.attachComponent<WorkerObserver>();
                    obs.fieldName.value = index.ToString();
                    obs.target.target = ((Worker)item);
                    children.Add().target = obs;
                }
                index++;
            }
        }

        public SyncListBaseObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public SyncListBaseObserver()
        {
        }

        public virtual void ChildRender(int index,ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            if(children[index].target != null)
            {
                children[index].target.ImguiRender(imGuiRenderer, canvas);
                if (removeable)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("X##" + referenceID.id.ToString()))
                    {
                        target.target?.Remove(index);
                    }
                }
            }
        }

        public virtual void RenderChildren(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            for (int i = 0; i < children.Count(); i++)
            {
                ChildRender(i, imGuiRenderer,canvas);
            }
        }
    }

}
