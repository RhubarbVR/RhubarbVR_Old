using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using g3;

namespace RhubarbEngine.Components.ImGUI
{


    [Category("ImGUI/Developer")]
    public class WorkerObserver : UIWidget, IObserver
    {
        public Sync<string> fieldName;

        [NoSave]
        [NoShow]
        [NoSync]
        private Worker lastWorker;

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
            var e = new Thread(BuildView,1024);
            e.Priority = ThreadPriority.BelowNormal;
            e.Start();
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
            try
            {
                ClearOld();
                if (target.target == null) return;
                if (lastWorker != null)
                {
                    lastWorker.onDispose -= Target_onDispose;
                }
                target.target.onDispose += Target_onDispose;
                lastWorker = target.target;
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
                else if ((typeof(Render.Material.Fields.MaterialField).IsAssignableFrom(type)))
                {
                    var comp = entity.attachComponent<MaterialFieldObserver>();
                    comp.target.target = (Render.Material.Fields.MaterialField)target.target;
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
            catch { }
        }

        private void Target_onDispose(Worker obj)
        {
            this.Dispose();
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

        [NoSave]
        [NoShow]
        [NoSync]
        Entity e;

        private void BuildWorker()
        {
            Type type = target.target.GetType();
            //This is a temp fix
            if (e == null)
            {
                e = entity.addChild(type.Name + "Children");
                e.persistence.value = false;
            }
            //I should remove on change update before initialized or add a on initialized check inside this function
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(Worker).IsAssignableFrom(field.FieldType) && (field.GetCustomAttributes(typeof(NoShowAttribute),false).Length <= 0))
                {
                    var obs = e.attachComponent<WorkerObserver>();
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
            else if (gType == typeof(bool))
            {
                var obs = entity.attachComponent<BoolSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<bool>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(int))
            {
                var obs = entity.attachComponent<IntSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<int>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(float))
            {
                var obs = entity.attachComponent<FloatSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<float>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(double))
            {
                var obs = entity.attachComponent<DoubleSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<double>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Colorf))
            {
                var obs = entity.attachComponent <ColorfSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Colorf>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector2f))
            {
                var obs = entity.attachComponent<Vector2fSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector2f>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector3f))
            {
                var obs = entity.attachComponent<Vector3fSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector3f>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector4f))
            {
                var obs = entity.attachComponent<Vector4fSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector4f>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Quaternionf))
            {
                var obs = entity.attachComponent<QuaternionfSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Quaternionf>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector2d))
            {
                var obs = entity.attachComponent<Vector2dSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector2d>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector3d))
            {
                var obs = entity.attachComponent<Vector3dSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector3d>)target.target);
                root.target = obs;
            }
            else if (gType == typeof(Vector4d))
            {
                var obs = entity.attachComponent<Vector4dSyncObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((Sync<Vector4d>)target.target);
                root.target = obs;
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
                BuildSyncObjListMember(false);
            }
        }

        private void BuildSyncObjListMember(bool withAdd)
        {
            if (withAdd)
            {
                SyncListObserver obs = entity.attachComponent<SyncListObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((ISyncList)target.target);
                root.target = obs;
            }
            else
            {
                NoAddSyncListObserver obs = entity.attachComponent<NoAddSyncListObserver>();
                obs.fieldName.value = fieldName.value;
                obs.target.target = ((ISyncList)target.target);
                root.target = obs;
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
            else if ((typeof(SyncRefList<>).IsAssignableFrom(typeg)) || (typeof(SyncValueList<>).IsAssignableFrom(typeg)) || (typeof(SyncAssetRefList<>).IsAssignableFrom(typeg)) || (typeof(SyncObjList<>).IsAssignableFrom(typeg)))
            {
                BuildSyncObjListMember(true);
            }
            else if ((typeof(SyncDelegate<>).IsAssignableFrom(typeg)) || (typeg == typeof(SyncDelegate)))
            {

            }
            else if ((typeof(AssetRef<>).IsAssignableFrom(typeg)))
            {
                Type a = typeof(World.Asset.AssetProvider<>).MakeGenericType(type.GetGenericArguments()[0]);
                BuildSyncRef(a);
            }
            else if ((typeof(Driver<>).IsAssignableFrom(typeg)))
            {
                Type a = typeof(DriveMember<>).MakeGenericType(type.GetGenericArguments()[0]);
                BuildSyncRef(a);
            }
            else if ((typeof(SyncRef<>).IsAssignableFrom(typeg)))
            {
                BuildSyncRef(type.GetGenericArguments()[0]);
            }
            else if ((typeof(SyncUserList).IsAssignableFrom(typeg)))
            {
                BuildSyncObjListMember(false);
            }
            else
            {
                Console.WriteLine("Unknown Sync Type" + typeg.FullName);
            }
        }

        private void BuildSyncRef(Type type)
        {
            Type a = typeof(SyncRefObserver<>).MakeGenericType(type);
            SyncRefObserver obs = (SyncRefObserver)entity.attachComponent(a);
            obs.fieldName.value = fieldName.value;
            obs.target.target = ((ISyncRef)target.target);
            root.target = obs;
        }

        public WorkerObserver(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public WorkerObserver()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            if (PassThrough)
            {
                root.target?.ImguiRender(imGuiRenderer,canvas);
            }
            else
            {

            }
        }
    }
}