using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
using RhubarbEngine.Render;
using RhubarbEngine.Components;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using System.Numerics;
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.World.ECS
{
	public class Entity : Worker, IWorldObject
	{

        public override void Destroy()
        {
            if (!IsRoot)
            {
                base.Destroy();
            }
        }

        [NoShow]
        [NoSave]
        [NoSync]
        public User CreatingUser
        {
            get
            {
                return World.users[(int)ReferenceID.GetOwnerID()];
            }
        }

        [NoShow]
		[NoSave]
		[NoSync]
		public User Manager
		{
			get
			{
				var retur = _manager.Target ?? (parent.Target?.NullableManager);
                if (retur == null)
				{
					retur = World.HostUser;
				}
				return retur;
			}
			set { _manager.Target = value; }
		}
		[NoShow]
		[NoSave]
		[NoSync]
		public User NullableManager
		{
			get
			{
				var retur = _manager.Target ?? (parent.Target?.Manager);
                return retur;
			}
		}


		private Matrix4x4 _cashedGlobalTrans = Matrix4x4.CreateScale(Vector3.One);

		private Matrix4x4 _cashedLocalMatrix = Matrix4x4.CreateScale(Vector3.One);
		[NoShow]
		[NoSave]
		[NoSync]
		private Entity _internalParent;

		[NoSave]
		public SyncRef<User> _manager;

		public new SyncRef<Entity> parent;

		public Sync<RemderLayers> remderlayer;

		public void AddPhysicsDisableder(IPhysicsDisableder physicsDisableder)
		{
			try
			{
				physicsDisableders.Add(physicsDisableder);
				OnPhysicsDisableder?.Invoke(PhysicsDisabled);
			}
			catch
			{
			}
		}

		public Vector3f GlobalPointToLocal(Vector3f point, bool Child = true)
		{
			var newtrans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f) * Matrix4x4.CreateTranslation(point.ToSystemNumrics());
            var parentMatrix = Child ? _cashedGlobalTrans : parent.Target._cashedGlobalTrans;
            Matrix4x4.Invert(parentMatrix, out var invparentMatrix);
			var newlocal = newtrans * invparentMatrix;
			Matrix4x4.Decompose(newlocal, out _, out _, out var newtranslation);
			return (Vector3f)newtranslation;
		}
		public Vector3f GlobalScaleToLocal(Vector3f Scale, bool Child = true)
		{
			var newtrans = Matrix4x4.CreateScale((Vector3)Scale) * Matrix4x4.CreateFromYawPitchRoll(0f, 0f, 0f) * Matrix4x4.CreateTranslation(0, 0, 0);
			var parentMatrix = Child ? _cashedGlobalTrans : parent.Target._cashedGlobalTrans;
            Matrix4x4.Invert(parentMatrix, out var invparentMatrix);
			var newlocal = newtrans * invparentMatrix;
			Matrix4x4.Decompose(newlocal, out var newscale, out _, out _);
			return (Vector3f)newscale;
		}
		public Quaternionf GlobalRotToLocal(Quaternionf Rot, bool Child = true)
		{
			var newtrans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion((Quaternion)Rot) * Matrix4x4.CreateTranslation(0, 0, 0);
			var parentMatrix = Child ? _cashedGlobalTrans : parent.Target._cashedGlobalTrans;
            Matrix4x4.Invert(parentMatrix, out var invparentMatrix);
			var newlocal = newtrans * invparentMatrix;
			Matrix4x4.Decompose(newlocal, out _, out var newrotation, out _);
			return (Quaternionf)newrotation;
		}

        public Quaternionf LocalRotToGlobal(Quaternionf Rot)
        {
            var newtrans = Matrix4x4.CreateScale(1f) * Matrix4x4.CreateFromQuaternion((Quaternion)Rot) * Matrix4x4.CreateTranslation(0, 0, 0);
            Matrix4x4.Decompose(newtrans * _cashedGlobalTrans, out _, out var newrotation, out _);
            return (Quaternionf)newrotation;
        }

        public void RemovePhysicsDisableder(IPhysicsDisableder physicsDisableder)
		{
			try
			{
				physicsDisableders.Remove(physicsDisableder);
				OnPhysicsDisableder?.Invoke(PhysicsDisabled);
			}
			catch
			{
			}
		}

		public event Action<bool> OnPhysicsDisableder;
		[NoShow]
		[NoSave]
		[NoSync]
		public List<IPhysicsDisableder> physicsDisableders = new();
        [NoShow]
        [NoSave]
        [NoSync]
        public bool PhysicsDisabled
        {
            get
            {
                return physicsDisableders.Count > 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return World?.RootEntity?.ReferenceID.id == ReferenceID.id;
            }
        }

        [NoShow]
        [NoSave]
        [NoSync]
        IWorldObject IWorldObject.Parent
        {
            get
            {
                return _internalParent?._children ?? (IWorldObject)World;
            }
        }

        public Sync<string> name;

		public Sync<Vector3f> position;

		public Sync<Quaternionf> rotation;

		public Sync<Vector3f> scale;

		public Sync<bool> enabled;

		public Sync<bool> persistence;
		[NoShow]
		public SyncObjList<Entity> _children;

		public bool parentEnabled = true;

		public event Action EnabledChanged;

		public event Action<bool> OnClick;

		public event Action<GrabbableHolder, bool> OnGrip;

		public event Action<GrabbableHolder, bool> OnDrop;

		public event Action<bool> OnPrimary;

		public event Action<bool> OnSecondary;

		public event Action<bool> OnTriggerTouching;

		public void SetParent(Entity entity, bool preserverGlobal = true, bool resetPos = false)
		{
			var mach = _cashedGlobalTrans;
			parent.Target = entity;
			if (preserverGlobal)
			{
				SetGlobalTrans(mach);
			}
			else if (resetPos)
			{
				SetGlobalTrans(entity._cashedGlobalTrans);
			}
		}

		public void SendTriggerTouching(bool laser, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnTriggerTouching?.Invoke(laser);
		}

		public void SendClick(bool laser, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnClick?.Invoke(laser);
		}
		public void SendGrip(bool laser, GrabbableHolder holder, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnGrip?.Invoke(holder, laser);
		}

		public void SendDrop(bool laser, GrabbableHolder holder, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnDrop?.Invoke(holder, laser);
		}

		public void SendPrimary(bool laser, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnPrimary?.Invoke(laser);
		}
		public void SendSecondary(bool laser, bool click = true)
		{
			if (!click)
            {
                return;
            }

            OnSecondary?.Invoke(laser);
		}
        public bool IsEnabled
        {
            get
            {
                return parentEnabled && enabled.Value;
            }
        }

        private void LoadListObject()
		{
			foreach (var item in _components)
			{
				item.ListObject(IsEnabled);
			}
		}

		public void ParentEnabledChange(bool _parentEnabled)
		{
			if (!enabled.Value)
            {
                return;
            }

            if (_parentEnabled != parentEnabled)
			{
				parentEnabled = _parentEnabled;
				foreach (var item in _children)
				{
					item.ParentEnabledChange(_parentEnabled);
				}
			}
			LoadListObject();
			EnabledChanged?.Invoke();
		}

		public void OnPersistenceChange(IChangeable newValue)
		{
			base.Persistent = persistence.Value;
		}
		public override void InturnalSyncObjs(bool newRefIds)
		{
			World.AddWorldEntity(this);
		}

		public Vector3f GlobalPos()
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out _, out _, out var translation);
			return (Vector3f)translation;
		}
		public Quaternionf GlobalRot()
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out _, out var rotation, out _);
			return (Quaternionf)rotation;
		}
		public Vector3f GlobalScale()
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out var scale, out _, out _);
			return (Vector3f)scale;
		}

		public void SetGlobalPos(Vector3f pos)
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out var scale, out var rotation, out _);
			SetGlobalTrans(Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(pos.ToSystemNumrics()));
		}

		public void SetGlobalRot(Quaternionf rot)
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out var scale, out _, out var translation);
			SetGlobalTrans(Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rot.ToSystemNumric()) * Matrix4x4.CreateTranslation(translation));
		}

		public void SetGlobalScale(Vector3f pos)
		{
			Matrix4x4.Decompose(_cashedGlobalTrans, out _, out var rotation, out var translation);
			SetGlobalTrans(Matrix4x4.CreateScale(pos.ToSystemNumrics()) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation));
		}
		public Matrix4x4 GlobalTrans()
		{
			return _cashedGlobalTrans;
		}

		public Vector3f Up
		{
			get
			{
				var mat = _cashedGlobalTrans;
				var e = new Vector3f((mat.M11 * Vector3f.AxisY.x) + (mat.M12 * Vector3f.AxisY.y) + (mat.M13 * Vector3f.AxisY.z), (mat.M21 * Vector3f.AxisY.x) + (mat.M22 * Vector3f.AxisY.y) + (mat.M23 * Vector3f.AxisY.z), (mat.M31 * Vector3f.AxisY.x) + (mat.M32 * Vector3f.AxisY.y) + (mat.M33 * Vector3f.AxisY.z));
				return e;

			}
		}

		public Matrix4x4 LocalTrans()
		{
			return _cashedLocalMatrix;
		}

		public void SetGlobalTrans(Matrix4x4 newtrans, bool SendUpdate = true)
		{
			var parentMatrix = Matrix4x4.CreateScale(Vector3.One);
			if (_internalParent != null)
			{
				parentMatrix = _internalParent.GlobalTrans();
			}
			Matrix4x4.Invert(parentMatrix, out var invparentMatrix);
			var newlocal = newtrans * invparentMatrix;
			Matrix4x4.Decompose(newlocal, out var newscale, out var newrotation, out var newtranslation);
			position.SetValueNoOnChange((Vector3f)newtranslation);
			rotation.SetValueNoOnChange((Quaternionf)newrotation);
			scale.SetValueNoOnChange((Vector3f)newscale);
			_cashedGlobalTrans = newtrans;
			UpdateGlobalTrans(SendUpdate);
		}

		public event Action<Matrix4x4> GlobalTransformChange;
		public event Action<Matrix4x4> GlobalTransformChangePhysics;

		public void SetLocalTrans(Matrix4x4 newtrans)
		{
			Matrix4x4.Decompose(newtrans, out var newscale, out var newrotation, out var newtranslation);
			position.SetValueNoOnChange((Vector3f)newtranslation);
			rotation.SetValueNoOnChange((Quaternionf)newrotation);
			scale.SetValueNoOnChange((Vector3f)newscale);
			UpdateGlobalTrans();
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			position = new Sync<Vector3f>(this, newRefIds);
            scale = new Sync<Vector3f>(this, newRefIds)
            {
                Value = Vector3f.One
            };
            rotation = new Sync<Quaternionf>(this, newRefIds)
            {
                Value = Quaternionf.Identity
            };
            name = new Sync<string>(this, newRefIds);
			enabled = new Sync<bool>(this, newRefIds);
            persistence = new Sync<bool>(this, newRefIds)
            {
                Value = true
            };
            _children = new SyncObjList<Entity>(this, newRefIds);
			_components = new SyncAbstractObjList<Component>(this, newRefIds);
            _components.ElementListChange += Components_ElementListChange;
            _manager = new SyncRef<User>(this, newRefIds);
			enabled.Value = true;
			parent = new SyncRef<Entity>(this, newRefIds);
			parent.Changed += Parent_Changed;
            remderlayer = new Sync<RemderLayers>(this, newRefIds)
            {
                Value = RemderLayers.normal
            };
            position.Changed += OnTransChange;
			rotation.Changed += OnTransChange;
			scale.Changed += OnTransChange;
			enabled.Changed += OnEnableChange;
			persistence.Changed += OnPersistenceChange;
		}

        private bool _calculateVelocity = false;

        private void Components_ElementListChange()
        {
            var calc = false;
            foreach (var item in _components)
            {
                if (typeof(IVelocityReqwest).IsAssignableFrom(item.GetType()))
                {
                    calc = true;
                }
            }
            _calculateVelocity = calc;
        }

        private void Parent_Changed(IChangeable obj)
		{
			if (World.RootEntity == this)
            {
                return;
            }

            if (parent.Target == _internalParent)
            {
                return;
            }

            if (_internalParent == null)
			{
				_internalParent = parent.Target;
				UpdateGlobalTrans();
				return;
			}
			if (parent.Target == null)
			{
				parent.Target = World.RootEntity;
				return;
			}
			if (World != parent.Target.World)
			{
				Logger.Log("tried to set parent from another world");
				return;
			}
			if (!parent.Target.CheckIfParented(this))
			{
				parent.Target._children.AddInternal(this);
				_internalParent._children.RemoveInternal(this);
				_internalParent = parent.Target;
				ParentEnabledChange(_internalParent.IsEnabled);
				UpdateGlobalTrans();
			}
			else
			{
				parent.Target = _internalParent;
			}
		}
		public bool CheckIfParented(Entity entity)
		{
            return entity == this || (_internalParent?.CheckIfParented(entity) ?? false);
        }
		private void OnTransChange(IChangeable newValue)
		{
			UpdateGlobalTrans();
		}

		private void OnEnableChange(IChangeable newValue)
		{
			if (!enabled.Value && (World.RootEntity == this))
			{ enabled.Value = true; };
			foreach (var item in _children)
			{
				item.ParentEnabledChange(enabled.Value);
			}
			LoadListObject();
			EnabledChanged?.Invoke();
		}
		private void UpdateGlobalTrans(bool Sendupdate = true)
		{
            if (!IsRoot)
            {
                var parentMatrix = Matrix4x4.CreateScale(Vector3.One);
                if (_internalParent != null)
                {
                    parentMatrix = _internalParent.GlobalTrans();
                }
                var localMatrix = Matrix4x4.CreateScale((Vector3)scale.Value) * Matrix4x4.CreateFromQuaternion(rotation.Value.ToSystemNumric()) * Matrix4x4.CreateTranslation((Vector3)position.Value);
                _cashedGlobalTrans = localMatrix * parentMatrix;
                _cashedLocalMatrix = localMatrix;
                if (Sendupdate)
                {
                    GlobalTransformChangePhysics?.Invoke(_cashedGlobalTrans);
                }

                GlobalTransformChange?.Invoke(_cashedGlobalTrans);
                foreach (var entity in _children)
                {
                    entity.UpdateGlobalTrans();
                }
            }
            else
            {
                _cashedGlobalTrans = Matrix4x4.CreateScale(Vector3.One);
                _cashedLocalMatrix = Matrix4x4.CreateScale(Vector3.One);
            }
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public Entity AddChild(string name = "Entity")
		{
			var val = _children.Add(true);
			val.parent.Target = this;
			val.name.Value = name;
			return val;
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public T AttachComponent<T>() where T : Component
		{
			var newcomp = (T)Activator.CreateInstance(typeof(T));
			_components.Add(newcomp);
			try
			{
				newcomp.OnAttach();
			}
			catch (Exception e)
			{
                Logger.Log("Failed To run Attach On Component" + typeof(T).Name + " Error:" + e.ToString());
			}
			newcomp.OnLoaded();
			return newcomp;
		}

		[NoShow]
		[NoSync]
		[NoSave]
		public Component AttachComponent(Type type)
		{
			var newcomp = (Component)Activator.CreateInstance(type);
			_components.Add(newcomp);
			try
			{
				newcomp.OnAttach();
			}
			catch (Exception e)
			{
                Logger.Log("Failed To run Attach On Component" + type.Name + " Error:" + e.ToString());
			}
			newcomp.OnLoaded();
			return newcomp;
		}


		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateGlobalTrans();
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public T GetFirstComponent<T>() where T : Component
		{
			foreach (var item in _components)
			{
				if (item.GetType() == typeof(T))
				{
					return (T)item;
				}
			}
			return null;
		}
		[NoShow]
		[NoSync]
		[NoSave]
		public IEnumerable<T> GetAllComponents<T>() where T : Component
		{
			foreach (var item in _components)
			{
				if (typeof(T).IsAssignableFrom(item.GetType()))
				{
					yield return (T)item;
				}
			}
		}

		public void AddToRenderQueue(RenderQueue gu, Vector3 playpos, RemderLayers layer, RhubarbEngine.Utilities.BoundingFrustum frustum, Matrix4x4 view)
		{
			if (((int)layer & (int)remderlayer.Value) <= 0)
			{
				return;
			}
			foreach (object comp in _components)
			{
				try
				{
					gu.Add((Renderable)comp, playpos, ref frustum, view);
				}
				catch
				{ }
			}
		}

		public override void Dispose()
		{
			World.RemoveWorldEntity(this);

            Parallel.ForEach(_components, (comp) => {
                try
                {
                    comp.Dispose();
                }
                catch { }
            });

            base.Dispose();
        }

        private Vector3 _lastPos;
        
        public Vector3f Velocity { get; private set; }

        public void Update(DateTime startTime, DateTime Frame)
		{
			if (base.IsRemoved)
            {
                return;
            }

            if (_calculateVelocity)
            {
                Velocity = (_lastPos - _cashedGlobalTrans.Translation) * (float)Engine.PlatformInfo.DeltaSeconds;
                _lastPos = _cashedGlobalTrans.Translation;
            }

            foreach (var comp in _components)
			{
				if (comp.enabled.Value && !comp.IsRemoved)
				{
					comp.CommonUpdate(startTime, Frame);
				}
			}
		}


		public SyncAbstractObjList<Component> _components;

		public Entity(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds)
		{

		}
		public Entity()
		{
		}
	}
}
