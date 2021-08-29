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
    public class ComponentObserver : UIWidget, IObserver
    {

        public SyncRef<Component> target;

        public SyncRef<IObserver> root;

        public SyncRefList<IObserver> children;

        private bool PassThrough => children.Count() <= 0;

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
            BuildView();
        }

        private void ClearOld()
        {

        }

        private void BuildView()
        {
            ClearOld();
            BuildWorker();
        }

        private void BuildWorker()
        {

        }


        public ComponentObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ComponentObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {

        }
    }
}
