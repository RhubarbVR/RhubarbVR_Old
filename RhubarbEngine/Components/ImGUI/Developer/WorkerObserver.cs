using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{


    [Category("ImGUI/Developer")]
    public class WorkerObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        public SyncRef<Worker> target;

        public SyncRef<IObserver> root;

        public SyncRefList<IObserver> children;

        public Sync<bool> removeChildrenOnDispose;

        private bool PassThrough => children.Count() <= 0;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            target = new SyncRef<Worker>(this, newRefIds);
            target.Changed += Target_Changed;
            root = new SyncRef<IObserver>(this, newRefIds);
            children = new SyncRefList<IObserver>(this, newRefIds);
            fieldName = new Sync<string>(this, newRefIds);
            removeChildrenOnDispose = new Sync<bool>(this, newRefIds);
            removeChildrenOnDispose.value = true;
        }

        private void Target_Changed(IChangeable obj)
        {
            if (entity.manager != world.localUser) return;
            BuildView();
        }

        private void ClearOld()
        {
            root.target?.Dispose();
            foreach (var item in children)
            {
                item.target?.Dispose();
            }
            children.Clear();
        }

        private void BuildView()
        {
            ClearOld();
            if (target.target == null) return;
            Type type = target.target.GetType();
            if ((typeof(Entity).IsAssignableFrom(type)))
            {
                var comp = entity.attachComponent<EntityObserver>();
                comp.target.target = (Entity)target.target;
                root.target = comp;
            }
            else if ((typeof(Component).IsAssignableFrom(type)))
            {
                var comp = entity.attachComponent<ComponentObserver>();
                comp.target.target = (Component)target.target;
                root.target = comp;
            }
            else if ((typeof(ISyncMember).IsAssignableFrom(type)))
            {
                BuildSyncMember(type);
            }
            else
            {
                BuildWorker();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (removeChildrenOnDispose.value)
            {
                root.target?.Dispose();
                foreach (var item in children)
                {
                    item.target?.Dispose();
                }
            }
        }

        private void BuildWorker()
        {
            Type type = target.target.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute),false).Length <= 0))
                {
                    var obs = entity.attachComponent<WorkerObserver>();
                    obs.fieldName.value = field.Name;
                    obs.target.target = ((Worker)field.GetValue(target.target));
                    children.Add().target = obs;
                }
            }
        }

        private void BuildBasicSyncMember(Type type)
        {
            Type gType = type.GetGenericArguments()[0];
            if (gType.IsEnum)
            {
                if(gType.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    Type a = typeof(FlagsEnumSyncObserver<>).MakeGenericType(gType);
                    FlagsEnumSyncObserver obs = (FlagsEnumSyncObserver)entity.attachComponent(a);
                    obs.fieldName.value = fieldName.value;
                    obs.target.target = ((IPrimitiveEditable)target.target);
                    root.target = obs;
                }
                else 
                {
                    Type a = typeof(EnumSyncObserver<>).MakeGenericType(gType);
                    EnumSyncObserver obs = (EnumSyncObserver)entity.attachComponent(a);
                    obs.fieldName.value = fieldName.value;
                    obs.target.target = ((IPrimitiveEditable)target.target);
                    root.target = obs;
                }
            }
            else
            {
                var obs = entity.attachComponent<PrimitiveSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((IPrimitiveEditable)target.target);
                root.target = obs;
            }
        }

        private void BuildSyncAbstractObjListMember(Type type)
        {
            Type gType = type.GetGenericArguments()[0];
            if (gType == typeof(Component))
            {
                SyncComponentListObserver obs = entity.attachComponent<SyncComponentListObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((ISyncList)target.target);
                root.target = obs;
            }
            else
            {
                //Type a = typeof(EnumSyncObserver<>).MakeGenericType(gType);
                //EnumSyncObserver obs = (EnumSyncObserver)entity.attachComponent(a);
                //obs.fieldName.value = fieldName.value;
                //obs.target.target = ((IPrimitiveEditable)target.target);
                //root.target = obs;
            }
        }

        private void BuildSyncMember(Type type)
        {
            Type typeg = type;
            if (type.IsGenericType)
            {
                typeg = type.GetGenericTypeDefinition();
            }
            if ((typeof(Sync<>).IsAssignableFrom(typeg)))
            {
                BuildBasicSyncMember(type);
            }
            else if ((typeof(SyncAbstractObjList<>).IsAssignableFrom(typeg)))
            {
                BuildSyncAbstractObjListMember(type);
            }
            else if ((typeof(SyncAssetRefList<>).IsAssignableFrom(typeg)))
            {

            }
            else if ((typeof(SyncDelegate<>).IsAssignableFrom(typeg)))
            {

            }
            else if ((typeof(SyncObjList<>).IsAssignableFrom(typeg)))
            {

            }
            else if ((typeof(SyncRef<>).IsAssignableFrom(typeg)))
            {

            }
            else if ((typeof(SyncUserList).IsAssignableFrom(typeg)))
            {

            }
            else if ((typeof(SyncValueList<>).IsAssignableFrom(typeg)))
            {

            }
            else
            {
                Console.WriteLine("Unknown Sync Type" + typeg.FullName);
            }
        }

        public WorkerObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public WorkerObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            if (PassThrough)
            {
                root.target?.ImguiRender(imGuiRenderer);
            }
            else
            {

            }
        }
    }
}