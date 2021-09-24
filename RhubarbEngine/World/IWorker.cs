using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Managers;
using RhubarbEngine.Render.Material.Fields;

namespace RhubarbEngine.World
{
    public interface IWorker : IChangeable, IWorldObject, IDisposable
    {

        public IUnitLogs Logger { get; }

        public IEngine Engine { get; }

        public IInputManager Input { get; }

        public double DeltaSeconds { get; }

        public event Action<IWorker> OnDispose;

        public void Initialize(World _world, IWorldObject _parent, bool newRefID = true, bool childlisten = true);

        public void CommonUpdate();

        public void InturnalSyncObjs(bool newRefIds);


        public void BuildSyncObjs(bool newRefIds);
        public void OnLoaded();

        public void OnUpdate();

        public void OnChangeInternal(IChangeable newValue);
        public void OnChanged();
        public void Removed();

        public void OnUserJoined(User user);

    }

    public class Worker : IWorker
    {
        private readonly List<IDisposable> _disposables = new();
        public void AddDisposable(IDisposable add)
        {
            try
            {
                _disposables.Add(add);
            }
            catch { }
        }

        public void RemoveDisposable(IDisposable add)
        {
            try
            {
                _disposables.Remove(add);
            }
            catch { }
        }
        public event Action<IWorker> OnDispose;

        public event Action<IChangeable> Changed;
        [NoShow]
        [NoSync]
        [NoSave]
        public World World { get; protected set; }
        [NoShow]
        [NoSync]
        [NoSave]
        public IWorldObject parent;

        public NetPointer ReferenceID { get; protected set; }

        public IUnitLogs Logger
        {
            get
            {
                return World.worldManager.engine.Logger;
            }
        }

        public IEngine Engine
        {
            get
            {
                return World.worldManager.engine;
            }
        }

        public IInputManager Input
        {
            get
            {
                return Engine.InputManager;
            }
        }

        public double DeltaSeconds
        {
            get
            {
                return World.worldManager.engine.PlatformInfo.DeltaSeconds;
            }
        }

        public bool Persistent = true;

        NetPointer IWorldObject.ReferenceID
        {
            get
            {
                return ReferenceID;
            }
        }

        [NoShow]
        [NoSync]
        [NoSave]
        World IWorldObject.World
        {
            get
            {
                return World;
            }
        }

        [NoShow]
        [NoSync]
        [NoSave]
        IWorldObject IWorldObject.Parent
        {
            get
            {
                return parent;
            }
        }

        bool IWorldObject.IsLocalObject
        {
            get
            {
                return false;
            }
        }

        bool IWorldObject.IsPersistent
        {
            get
            {
                return Persistent;
            }
        }

        public bool IsRemoved { get; private set; } = false;

        public void Destroy()
        {
            Dispose();
        }
        public Worker()
        {

        }
        public Worker(World _world, IWorldObject _parent, bool newRefID = true, bool childlisten = true)
        {
            Initialize(_world, _parent, newRefID, childlisten);
        }

        public virtual void Initialize(World _world, IWorldObject _parent, bool newRefID = true, bool childlisten = true)
        {
            World = _world;
            parent = _parent;
            parent.AddDisposable(this);
            InturnalSyncObjs(newRefID);
            BuildSyncObjs(newRefID);
            if (childlisten)
            {
                var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                foreach (var field in fields)
                {
                    if (typeof(IChangeable).IsAssignableFrom(field.FieldType) && ((IChangeable)field.GetValue(this)) != null)
                    {
                        ((IChangeable)field.GetValue(this)).Changed += OnChangeInternal;

                    }
                }
            }
            if (newRefID)
            {
                ReferenceID = _world.BuildRefID();
                _world.AddWorldObj(this);
            }
        }


        public virtual void InturnalSyncObjs(bool newRefIds)
        {

        }


        public virtual void BuildSyncObjs(bool newRefIds)
        {

        }

        public virtual void OnLoaded()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public void OnChangeInternal(IChangeable newValue)
        {
            Changed?.Invoke(this);
            OnChanged();
        }
        public virtual void OnChanged()
        {

        }
        public virtual void Removed()
        {

        }

        public virtual void CommonUpdate()
        {

        }

        public virtual void OnUserJoined(User user)
        {

        }


        public virtual void Dispose()
        {
            Removed();
            World.RemoveWorldObj(this);
            OnDispose?.Invoke(this);
            foreach (var dep in _disposables)
            {
                dep.Dispose();
            }
            IsRemoved = true;
        }


        public virtual DataNodeGroup Serialize(WorkerSerializerObject serializerObject)
        {
            return serializerObject.CommonWorkerSerialize(this);
        }

        public virtual void DeSerialize(DataNodeGroup data, List<Action> onload = default, bool NewRefIDs = false, Dictionary<ulong, ulong> newRefID = default, Dictionary<ulong, List<RefIDResign>> latterResign = default)
        {

            if (data == null)
            {
                World.worldManager.engine.Logger.Log("Node did not exsets When loading Node: " + GetType().FullName);
                return;
            }
            if (NewRefIDs)
            {
                if (newRefID == null)
                {
                    Console.WriteLine("Problem With " + GetType().FullName);
                }
                newRefID.Add(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID(), ReferenceID.getID());
                if (latterResign.ContainsKey(((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()))
                {
                    foreach (var func in latterResign[((DataNode<NetPointer>)data.GetValue("referenceID")).Value.getID()])
                    {
                        func(ReferenceID.getID());
                    }
                }
            }
            else
            {
                ReferenceID = ((DataNode<NetPointer>)data.GetValue("referenceID")).Value;
                if (ReferenceID.id == new NetPointer(0).id)
                {
                    Logger.Log(GetType().FullName + " RefID null");
                }
                else
                {
                    World.AddWorldObj(this);
                }
            }

            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || (!NewRefIDs && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0))))
                {
                    if (((IWorldObject)field.GetValue(this)) == null)
                    {
                        throw new Exception("Sync not initialized on " + GetType().FullName + " Field: " + field.Name);
                    }
                    ((IWorldObject)field.GetValue(this)).DeSerialize((DataNodeGroup)data.GetValue(field.Name), onload, NewRefIDs, newRefID, latterResign);
                }
            }
            if (typeof(IRenderObject).IsAssignableFrom(GetType()))
            {
                onload.Add(OnLoaded);
            }
            else
            {
                onload.Insert(0, OnLoaded);
            }
        }

    }
}
