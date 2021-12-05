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

        public override void OnSave()
        {
            ClearOld();
            base.OnSave();
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
				if (typeof(Entity).IsAssignableFrom(type) && !GetType().IsAssignableTo(typeof(EntityProperties)))
				{
					var comp = Entity.AttachComponent<EntityProperties>();
					comp.target.Target = (Entity)target.Target;
					root.Target = comp;
				}
				else if (typeof(Component).IsAssignableFrom(type)&& !GetType().IsAssignableTo(typeof(ComponentProperties)))
				{
					var comp = Entity.AttachComponent<ComponentProperties>();
					comp.target.Target = (Component)target.Target;
					root.Target = comp;
				}
				else if (typeof(Render.Material.Fields.MaterialField).IsAssignableFrom(type) && !GetType().IsAssignableTo(typeof(MaterialFieldProperties)))
				{
					var comp = Entity.AttachComponent<MaterialFieldProperties>();
					comp.target.Target = (Render.Material.Fields.MaterialField)target.Target;
					root.Target = comp;
				}
				else if (typeof(ISyncMember).IsAssignableFrom(type))
				{
					BuildSyncMember(type,target.Target,fieldName.Value);
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


		private void BuildWorker()
		{
			var type = target.Target.GetType();
			//I should remove on change update before initialized or add a on initialized check inside this function
			var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
                if (!(!typeof(IWorker).IsAssignableFrom(field.FieldType) || field.GetCustomAttributes(typeof(NoShowAttribute), false).Length > 0))
				{
                    if (typeof(Entity).IsAssignableFrom(field.FieldType))
                    {
                        var comp = Entity.AddChild(field.Name).AttachComponent<EntityProperties>();
                        comp.target.Target = (Entity)field.GetValue(target.Target);
                        children.Add().Target = comp;
                    }
                    else if (typeof(Component).IsAssignableFrom(field.FieldType))
                    {
                        var comp = Entity.AddChild(field.Name).AttachComponent<ComponentProperties>();
                        comp.target.Target = (Component)field.GetValue(target.Target);
                        children.Add().Target = comp;
                    }
                    else if (typeof(Render.Material.Fields.MaterialField).IsAssignableFrom(field.FieldType))
                    {
                        var comp = Entity.AddChild(field.Name).AttachComponent<MaterialFieldProperties>();
                        comp.target.Target = (Render.Material.Fields.MaterialField)field.GetValue(target.Target);
                        children.Add().Target = comp;
                    }
                    else if (typeof(ISyncMember).IsAssignableFrom(field.FieldType))
                    {
                        BuildSyncMember(field.FieldType, (IWorker)field.GetValue(target.Target), field.Name, field.GetCustomAttribute<MultiLineSyncAttribute>() != null);
                    }
                    else
                    {
                        var obs = Entity.AddChild(field.Name).AttachComponent<WorkerProperties>();
                        obs.fieldName.Value = field.Name;
                        obs.target.Target = (IWorker)field.GetValue(target.Target);
                        children.Add().Target = obs;
                    }
				}
			}
		}

		private void BuildBasicSyncMember(Type type, IWorker worker,string namefield, bool multiLine)
		{
			var gType = type.GetGenericArguments()[0];
            if (multiLine)
            {
                var obs = Entity.AddChild(namefield).AttachComponent<MultiLineSyncObserver>();
                obs.fieldName.Value = namefield;
                obs.target.Target = (IPrimitiveEditable)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
                return;
            }
			if (gType.IsEnum)
			{
				if (gType.GetCustomAttribute<FlagsAttribute>() != null)
				{
					var a = typeof(FlagsEnumSyncObserver<>).MakeGenericType(gType);
					var obs = (FlagsEnumSyncObserver)Entity.AddChild(namefield).AttachComponent(a);
					obs.fieldName.Value = namefield;
					obs.target.Target = (IPrimitiveEditable)worker;
                    if (worker == target.Target)
                    {
                        root.Target = obs;
                    }
                    else
                    {
                        children.Add().Target = obs;
                    }
                }
				else
				{
					var a = typeof(EnumSyncObserver<>).MakeGenericType(gType);
					var obs = (EnumSyncObserver)Entity.AddChild(namefield).AttachComponent(a);
					obs.fieldName.Value = namefield;
					obs.target.Target = (IPrimitiveEditable)worker;
                    if (worker == target.Target)
                    {
                        root.Target = obs;
                    }
                    else
                    {
                        children.Add().Target = obs;
                    }
                }
			}
			else if (gType == typeof(bool))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<BoolSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<bool>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(int))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<IntSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<int>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else if (gType == typeof(uint))
            {
                var obs = Entity.AddChild(namefield).AttachComponent<UIntSyncObserver>();
                obs.fieldName.Value = namefield;
                obs.target.Target = (Sync<uint>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else if (gType == typeof(float))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<FloatSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<float>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(double))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<DoubleSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<double>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Colorf))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<ColorfSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Colorf>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector2f))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector2fSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector2f>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector3f))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector3fSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector3f>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector4f))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector4fSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector4f>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Quaternionf))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<QuaternionfSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Quaternionf>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector2d))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector2dSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector2d>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector3d))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector3dSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector3d>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else if (gType == typeof(Vector4d))
			{
				var obs = Entity.AddChild(namefield).AttachComponent<Vector4dSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (Sync<Vector4d>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else if (gType == typeof(Vector2u))
            {
                var obs = Entity.AddChild(namefield).AttachComponent<Vector2uSyncObserver>();
                obs.fieldName.Value = namefield;
                obs.target.Target = (Sync<Vector2u>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else if (gType == typeof(Vector3u))
            {
                var obs = Entity.AddChild(namefield).AttachComponent<Vector3uSyncObserver>();
                obs.fieldName.Value = namefield;
                obs.target.Target = (Sync<Vector3u>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else if (gType == typeof(Vector4u))
            {
                var obs = Entity.AddChild(namefield).AttachComponent<Vector4uSyncObserver>();
                obs.fieldName.Value = namefield;
                obs.target.Target = (Sync<Vector4u>)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
            else
			{
				var obs = Entity.AddChild(namefield).AttachComponent<PrimitiveSyncObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (IPrimitiveEditable)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
		}

		private void BuildSyncAbstractObjListMember(Type type, IWorker worker, string namefield)
		{
			var gType = type.GetGenericArguments()[0];
			if (gType == typeof(Component))
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<SyncComponentListObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (ISyncList)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else
			{
				BuildSyncObjListMember(false,worker, namefield);
			}
		}

		private void BuildSyncObjListMember(bool withAdd, IWorker worker, string namefield)
		{
			if (withAdd)
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<SyncListObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (ISyncList)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
			else
			{
				var obs = Entity.AddChild(fieldName.Value).AttachComponent<NoAddSyncListObserver>();
				obs.fieldName.Value = namefield;
				obs.target.Target = (ISyncList)worker;
                if (worker == target.Target)
                {
                    root.Target = obs;
                }
                else
                {
                    children.Add().Target = obs;
                }
            }
		}

		private void BuildSyncMember(Type type, IWorker worker, string namefield,bool multiLine = false)
		{
			var typeg = type;
			if (type.IsGenericType)
			{
				typeg = type.GetGenericTypeDefinition();
			}
			if (typeof(Sync<>).IsAssignableFrom(typeg))
			{
				BuildBasicSyncMember(type,worker, namefield, multiLine);
			}
			else if (typeof(SyncAbstractObjList<>).IsAssignableFrom(typeg))
			{
				BuildSyncAbstractObjListMember(type, worker, namefield);
			}
			else if (!(!typeof(SyncRefList<>).IsAssignableFrom(typeg) && !typeof(SyncValueList<>).IsAssignableFrom(typeg) && !typeof(SyncAssetRefList<>).IsAssignableFrom(typeg) && !typeof(SyncObjList<>).IsAssignableFrom(typeg)))
			{
				BuildSyncObjListMember(true, worker, namefield);
			}
			else if (!(!typeof(SyncDelegate<>).IsAssignableFrom(typeg) && typeg != typeof(SyncDelegate)))
			{

			}
			else if (typeof(AssetRef<>).IsAssignableFrom(typeg))
			{
				var a = typeof(World.Asset.AssetProvider<>).MakeGenericType(type.GetGenericArguments()[0]);
				BuildSyncRef(a, worker, namefield);
			}
			else if (typeof(Driver<>).IsAssignableFrom(typeg))
			{
				var a = typeof(IDriveMember<>).MakeGenericType(type.GetGenericArguments()[0]);
				BuildSyncRef(a, worker, namefield);
			}
			else if (typeof(SyncRef<>).IsAssignableFrom(typeg))
			{
				BuildSyncRef(type.GetGenericArguments()[0], worker, namefield);
			}
			else if (typeof(SyncUserList).IsAssignableFrom(typeg))
			{
				BuildSyncObjListMember(false, worker,namefield);
			}
			else if (typeof(UserStream).IsAssignableFrom(typeg))
            {
                if(worker == target.Target)
                {
                    BuildWorker();
                }
                else
                {
                    
                }
            }
            else
			{
				Console.WriteLine("Unknown Sync Type" + typeg.FullName);
			}
		}

		private void BuildSyncRef(Type type, IWorker worker, string namefield)
		{
			var a = typeof(SyncRefObserver<>).MakeGenericType(type);
			var obs = (SyncRefObserver)Entity.AddChild(fieldName.Value).AttachComponent(a);
			obs.fieldName.Value = namefield;
			obs.target.Target = (ISyncRef)worker;
            if (worker == target.Target)
            {
                root.Target = obs;
            }
            else
            {
                children.Add().Target = obs;
            }
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
                Vector2 max;
                Vector2 min;
                if (ImGui.TreeNodeEx($"{target.Target?.GetType().GetFormattedName() ?? "null"} ID:({target.Target?.ReferenceID.id.ToHexString() ?? "null"}) ##{ReferenceID.id}", ImGuiTreeNodeFlags.Framed))
                {
                    max = ImGui.GetItemRectMax();
                    min = ImGui.GetItemRectMin();
                    Helper.ThreadSafeForEach(children, (item) => ((SyncRef<IPropertiesElement>)item).Target?.ImguiRender(imGuiRenderer, canvas));
                    ImGui.TreePop();
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