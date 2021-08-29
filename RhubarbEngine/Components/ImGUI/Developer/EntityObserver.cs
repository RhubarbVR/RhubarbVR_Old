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
    

    [Category("ImGUI/Developer")]
    public class EntityObserver : UIWidget,IObserver
    {

        public SyncRef<Entity> target;

        public SyncRefList<IObserver> children;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<Entity>(this, newRefIds);
            target.Changed += Target_Changed;
            children = new SyncRefList<IObserver>(this, newRefIds);
        }

        private void Target_Changed(IChangeable obj)
        {
            BuildView();
        }
        private void ClearOld()
        {
            foreach (var item in children)
            {
                item.target?.Dispose();
            }
            children.Clear();
        }
        public EntityObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public EntityObserver()
        {
        }

        private void BuildView()
        {
            ClearOld();
            if (target.target == null) return;
            FieldInfo[] fields = typeof(Entity).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute), false).Length <= 0))
                {
                    var obs = entity.attachComponent<WorkerObserver>();
                    obs.fieldName.value = field.Name;
                    obs.target.target = ((Worker)field.GetValue(target.target));
                    children.Add().target = obs;
                }
            }
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            if (ImGui.BeginChild($"{target.target?.name.value??"null"}##{referenceID.id}",ImGui.GetWindowContentRegionMax(),false,ImGuiWindowFlags.NoCollapse| ImGuiWindowFlags.NoResize))
            {
                ImGui.Text($"{target.target?.name.value ?? "null"} ID:({target.target?.referenceID.id.ToString() ?? "null"})");
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer);
                }
                ImGui.EndChild();
            }
            
        }
    }
}
