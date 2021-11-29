using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using RNumerics;
using System.Numerics;
using ImGuiNET;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class WorkerProperties : UIWidget, IPropertiesElement
	{
		public Sync<string> fieldName;

		[NoSave]
		[NoShow]
		[NoSync]
		private IWorker _lastWorker;

		public SyncRef<IWorker> target;

		public SyncRef<IPropertiesElement> root;

		public SyncRefList<IPropertiesElement> children;

		public Sync<bool> removeChildrenOnDispose;

        private bool PassThrough
        {
            get
            {
                return children.Count() <= 0;
            }
        }

        public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<IWorker>(this, newRefIds);
			target.Changed += Target_Changed;
			root = new SyncRef<IPropertiesElement>(this, newRefIds);
			children = new SyncRefList<IPropertiesElement>(this, newRefIds);
			fieldName = new Sync<string>(this, newRefIds);
            removeChildrenOnDispose = new Sync<bool>(this, newRefIds, true);
        }

		private void Target_Changed(IChangeable obj)
		{
			if (Entity.Manager != World.LocalUser)
            {
                return;
            }

            var e = new Thread(BuildView, 1024)
            {
                Priority = ThreadPriority.BelowNormal
            };
            e.Start();
		}

		private void ClearOld()
		{
			root.Target?.Dispose();
			foreach (var item in children)
			{
				item.Target?.Dispose();
			}
			children.Clear();
		}

		private void BuildView()
		{
			try
			{
				ClearOld();
				if (target.Target == null)
                {
                    return;
                }

                if (_lastWorker != null)
				{
					_lastWorker.OnDispose -= Target_onDispose;
				}
				target.Target.OnDispose += Target_onDispose;
				_lastWorker = target.Target;
				var type = target.Target.GetType();
				if (typeof(Entity).IsAssignableFrom(type))
				{
					var comp = Entity.AttachComponent<EntityProperties>();
					comp.target.Target = (Entity)target.Target;
					root.Target = comp;
				}
				else if (typeof(Component).IsAssignableFrom(type))
				{
					var comp = Entity.AttachComponent<ComponentProperties>();
					comp.target.Target = (Component)target.Target;
					root.Target = comp;
				}
				else if (typeof(Render.Material.Fields.MaterialField).IsAssignableFrom(type))
				{
					var comp = Entity.AttachComponent<MaterialFieldProperties>();
					comp.target.Target = (Render.Material.Fields.MaterialField)target.Target;
					root.Target = comp;
				}
				else if (typeof(ISyncMember).IsAssignableFrom(type))
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

		private void Target_onDispose(IWorker obj)
		{
			Dispose();
		}

		public override void Dispose()
		{
            if (removeChildrenOnDispose.Value)
            {
                root.Target?.Dispose();
                foreach (var item in children)
                {
                    item.Target?.Dispose();
                }
            }
            base.Dispose();
		}

		[NoSave]
		[NoShow]
		[NoSync]
		Entity _e;

		private void BuildWorker()
		{
			var type = target.Target.GetType();
			//This is a temp fix
			if (_e == null)
			{
				_e = Entity.AddChild(type.Name + "Children");
				_e.persistence.Value = false;
			}
			//I should remove on change update before initialized or add a on initialized check inside this function
			var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
                if (!(!typeof(IWorker).IsAssignableFrom(field.FieldType) || field.GetCustomAttributes(typeof(NoShowAttribute), false).Length > 0))
				{
					var obs = _e.AttachComponent<WorkerProperties>();
					obs.fieldName.Value = field.Name;
					obs.target.Target = (IWorker)field.GetValue(target.Target);
					children.Add().Target = obs;
				}
			}
		}

		private void BuildBasicSyncMember(Type type)
		{
			var gType = type.GetGenericArguments()[0];
			if (gType.IsEnum)
			{
				if (gType.GetCustomAttribute<FlagsAttribute>() != null)
				{
					var a = typeof(FlagsEnumSyncObserver<>).MakeGenericType(gType);
					var obs = (FlagsEnumSyncObserver)Entity.AddChild(fieldName.Value).AttachComponent(a);
					obs.fieldName.Value = fieldName.Value;
					obs.target.Target = (IPrimitiveEditable)target.Target;
					root.Target = obs;
				}
				else
				{
					var a = typeof(EnumSyncObserver<>).MakeGenericType(gType);
					var obs = (EnumSyncObserver)Entity.AddChild(fieldName.Value).AttachComponent(a);
					obs.fieldName.Value = fieldName.Value;
					obs.target.Target = (IPrimitiveEditable)target.Target;
					root.Target = obs;
				}
			}
			else if (gType == typeof(bool))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<BoolSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<bool>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(int))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<IntSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<int>)target.Target;
				root.Target = obs;
			}
            else if (gType == typeof(uint))
            {
                var obs = Entity.AddChild(fieldName.Value).AttachComponent<UIntSyncObserver>();
                obs.fieldName.Value = fieldName.Value;
                obs.target.Target = (Sync<uint>)target.Target;
                root.Target = obs;
            }
            else if (gType == typeof(float))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<FloatSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<float>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(double))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<DoubleSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<double>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Colorf))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<ColorfSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Colorf>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector2f))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector2fSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector2f>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector3f))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector3fSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector3f>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector4f))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector4fSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector4f>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Quaternionf))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<QuaternionfSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Quaternionf>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector2d))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector2dSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector2d>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector3d))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector3dSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector3d>)target.Target;
				root.Target = obs;
			}
			else if (gType == typeof(Vector4d))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector4dSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (Sync<Vector4d>)target.Target;
				root.Target = obs;
			}
            else if (gType == typeof(Vector2u))
            {
                var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector2uSyncObserver>();
                obs.fieldName.Value = fieldName.Value;
                obs.target.Target = (Sync<Vector2u>)target.Target;
                root.Target = obs;
            }
            else if (gType == typeof(Vector3u))
            {
                var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector3uSyncObserver>();
                obs.fieldName.Value = fieldName.Value;
                obs.target.Target = (Sync<Vector3u>)target.Target;
                root.Target = obs;
            }
            else if (gType == typeof(Vector4u))
            {
                var obs = Entity.AddChild(fieldName.Value).AttachComponent<Vector4uSyncObserver>();
                obs.fieldName.Value = fieldName.Value;
                obs.target.Target = (Sync<Vector4u>)target.Target;
                root.Target = obs;
            }
            else
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<PrimitiveSyncObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (IPrimitiveEditable)target.Target;
				root.Target = obs;
			}
		}

		private void BuildSyncAbstractObjListMember(Type type)
		{
			var gType = type.GetGenericArguments()[0];
			if (gType == typeof(Component))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<SyncComponentListObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (ISyncList)target.Target;
				root.Target = obs;
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
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<SyncListObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (ISyncList)target.Target;
				root.Target = obs;
			}
			else
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<NoAddSyncListObserver>();
				obs.fieldName.Value = fieldName.Value;
				obs.target.Target = (ISyncList)target.Target;
				root.Target = obs;
			}
		}

		private void BuildSyncMember(Type type)
		{
			var typeg = type;
			if (type.IsGenericType)
			{
				typeg = type.GetGenericTypeDefinition();
			}
			if (typeof(Sync<>).IsAssignableFrom(typeg))
			{
				BuildBasicSyncMember(type);
			}
			else if (typeof(SyncAbstractObjList<>).IsAssignableFrom(typeg))
			{
				BuildSyncAbstractObjListMember(type);
			}
			else if (!(!typeof(SyncRefList<>).IsAssignableFrom(typeg) && !typeof(SyncValueList<>).IsAssignableFrom(typeg) && !typeof(SyncAssetRefList<>).IsAssignableFrom(typeg) && !typeof(SyncObjList<>).IsAssignableFrom(typeg)))
			{
				BuildSyncObjListMember(true);
			}
			else if (!(!typeof(SyncDelegate<>).IsAssignableFrom(typeg) && typeg != typeof(SyncDelegate)))
			{

			}
			else if (typeof(AssetRef<>).IsAssignableFrom(typeg))
			{
				var a = typeof(World.Asset.AssetProvider<>).MakeGenericType(type.GetGenericArguments()[0]);
				BuildSyncRef(a);
			}
			else if (typeof(Driver<>).IsAssignableFrom(typeg))
			{
				var a = typeof(IDriveMember<>).MakeGenericType(type.GetGenericArguments()[0]);
				BuildSyncRef(a);
			}
			else if (typeof(SyncRef<>).IsAssignableFrom(typeg))
			{
				BuildSyncRef(type.GetGenericArguments()[0]);
			}
			else if (typeof(SyncUserList).IsAssignableFrom(typeg))
			{
				BuildSyncObjListMember(false);
			}
			else if (typeof(UserStream).IsAssignableFrom(typeg))
            {
                BuildWorker();
            }
            else
			{
				Console.WriteLine("Unknown Sync Type" + typeg.FullName);
			}
		}

		private void BuildSyncRef(Type type)
		{
			var a = typeof(SyncRefObserver<>).MakeGenericType(type);
			var obs = (SyncRefObserver)Entity.AddChild(fieldName.Value).AttachComponent(a);
			obs.fieldName.Value = fieldName.Value;
			obs.target.Target = (ISyncRef)target.Target;
			root.Target = obs;
		}

		public WorkerProperties(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public WorkerProperties()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			if (PassThrough)
			{
				root.Target?.ImguiRender(imGuiRenderer, canvas);
			}
			else
			{
                var open = true;
                Vector2 max;
                Vector2 min;
                if (ImGui.CollapsingHeader($"{target.Target?.GetType().GetFormattedName() ?? "null"} ID:({target.Target?.ReferenceID.id.ToHexString() ?? "null"}) ##{ReferenceID.id}", ref open))
                {
                    max = ImGui.GetItemRectMax();
                    min = ImGui.GetItemRectMin();
                    Helper.ThreadSafeForEach(children, (item) => ((SyncRef<IPropertiesElement>)item).Target?.ImguiRender(imGuiRenderer, canvas));
                }
                else
                {
                    max = ImGui.GetItemRectMax();
                    min = ImGui.GetItemRectMin();
                }
                if (ImGui.IsMouseHoveringRect(min, max) && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    Interaction.GrabbableHolder source = null;
                    switch (canvas.imputPlane.Target?.Source ?? Interaction.InteractionSource.None)
                    {
                        case Interaction.InteractionSource.LeftLaser:
                            source = World.LeftLaserGrabbableHolder;
                            break;
                        case Interaction.InteractionSource.RightLaser:
                            source = World.RightLaserGrabbableHolder;
                            break;
                        case Interaction.InteractionSource.HeadLaser:
                            source = World.HeadLaserGrabbableHolder;
                            break;
                        default:
                            break;
                    }
                    if (source != null)
                    {
                        source.Referencer.Target = target.Target;
                    }
                }
            }
		}
	}
}