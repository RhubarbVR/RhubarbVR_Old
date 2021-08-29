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
    public class EnumSyncObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<IPrimitiveEditable> target;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<IPrimitiveEditable>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }


        public EnumSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public EnumSyncObserver()
        {
        }

    }

    [Category("ImGUI/Developer/SyncMemberObservers")]
    public class EnumSyncObserver<T> : EnumSyncObserver, IObserver where T : struct,System.Enum
    {

        public EnumSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public EnumSyncObserver()
        {
        }

        string[] ve = Enum.GetNames(typeof(T));

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            int c = Array.IndexOf(ve, Enum.GetName(typeof(T), (((Sync<T>)target.target).value)));
            ImGui.Combo((fieldName.value ?? "null") + $"##{referenceID.id}", ref c, ve, ve.Length);
            if (c != (int)(object)(((Sync<T>)target.target).value))
            {
                ((Sync<T>)target.target).value = Enum.GetValues<T>()[c];
            }
        }
    }
}
