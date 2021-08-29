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


    [Category("ImGUI/Developer/SyncMemberObservers")]
    public class PrimitiveSyncObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<IPrimitiveEditable> target;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<IPrimitiveEditable>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
        }


        public PrimitiveSyncObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public PrimitiveSyncObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            string val = target.target?.primitiveString??"null";
            if(ImGui.InputText((fieldName.value ?? "null") + $"##{referenceID.id}", ref val, (uint)val.Length + 255,ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if(target.target != null)
                    target.target.primitiveString = val;
            }
            
        }
    }
}
