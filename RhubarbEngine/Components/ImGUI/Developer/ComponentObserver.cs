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
using System.Threading;

namespace RhubarbEngine.Components.ImGUI
{


    [Category("ImGUI/Developer")]
    public class ComponentObserver : UIWidget, IObserver
    {

        public SyncRef<Component> target;

        public SyncRef<IObserver> root;

        public SyncRefList<IObserver> children;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<Component>(this, newRefIds);
            target.Changed += Target_Changed;
            root = new SyncRef<IObserver>(this, newRefIds);
            children = new SyncRefList<IObserver>(this, newRefIds);
        }

        private void Target_Changed(IChangeable obj)
        {
            if (entity.manager != world.localUser) return;
            var e = new Thread(BuildView, 1024);
            e.Priority = ThreadPriority.BelowNormal;
            e.Start();
        }

        private void ClearOld()
        {
            foreach (var item in children)
            {
                item.target?.Dispose();
            }
            children.Clear();
        }

        private void BuildView()
        {
            try
            {
                ClearOld();
                if (target.target == null) return;
                FieldInfo[] fields = target.target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
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
            catch { }
        }


        public ComponentObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ComponentObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            bool open = true;
            Vector2 max;
            Vector2 min;
            if (ImGui.CollapsingHeader($"{target.target?.GetType().Name ?? "null"} ID:({target.target?.referenceID.id.ToHexString() ?? "null"}) ##{referenceID.id}",ref open))
            {
                max = ImGui.GetItemRectMax();
                min = ImGui.GetItemRectMin();
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer,canvas);
                }
            }
            else
            {
                max = ImGui.GetItemRectMax();
                min = ImGui.GetItemRectMin();
            }
            if (ImGui.IsMouseHoveringRect(min, max)&&ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                Interaction.GrabbableHolder source = null;
                switch (canvas.imputPlane.target?.source ?? Interaction.InteractionSource.None)
                {
                    case Interaction.InteractionSource.LeftLaser:
                        source = world.LeftLaserGrabbableHolder;
                        break;
                    case Interaction.InteractionSource.RightLaser:
                        source = world.RightLaserGrabbableHolder;
                        break;
                    case Interaction.InteractionSource.HeadLaser:
                        source = world.HeadLaserGrabbableHolder;
                        break;
                    default:
                        break;
                }
                if (source != null)
                {
                    source.Referencer.target = target.target;
                }
            }
            if (!open)
            {
                target.target?.Dispose();
            }

        }
    }
}
