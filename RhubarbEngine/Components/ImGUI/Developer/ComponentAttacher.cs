using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using RNumerics;
using System.Linq;
using System.Collections.Generic;

namespace RhubarbEngine.Components.ImGUI
{


	[Category("ImGUI/Developer")]
	public class ComponentAttacher : UIWidget
	{
		public SyncRef<IWorker> target;

		public Sync<string> path;

		public SyncRefList<ComponentAttacherField> children;

		public SyncRef<Entity> Tentity;

		[NoSave]
		[NoShow]
		[NoSync]
		Entity _list;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			target = new SyncRef<IWorker>(this, newRefIds);
			children = new SyncRefList<ComponentAttacherField>(this, newRefIds);
            path = new Sync<string>(this, newRefIds)
            {
                Value = "/"
            };
            path.Changed += Path_Changed;
			Tentity = new SyncRef<Entity>(this, newRefIds);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			LoadList();
		}

		private void Path_Changed(IChangeable obj)
		{
			LoadList();
		}

		private static bool PathCheck(string[] Path, string[] strings)
		{
			if (strings.Length < Path.Length)
			{
				return false;
			}
			for (var i = 0; i < Path.Length; i++)
			{
				if (Path[i] != strings[i])
				{
					return false;
				}
			}
			return true;
		}

		private void LoadList()
		{
			if (World.LocalUser != Entity.Manager)
            {
                return;
            }

            if (path.Value.Contains("`1"))
            {
                return;
            }

            if (_list != null)
            {
                _list.Destroy();
            }

            _list = Entity.AddChild("CompList");
			_list.persistence.Value = false;
			string[] pa;
			if (string.IsNullOrEmpty(path.Value))
			{
				pa = Array.Empty<string>();
			}
			else
			{
				IEnumerable<string> p;
				try
				{
					p = from e in path.Value.Split('/', '\\')
						where !string.IsNullOrEmpty(e)
						select e;
				}
				catch { p = null; };
				pa = p == null ? Array.Empty<string>() : p.ToArray();
            }

			try
			{
				var assem = Assembly.GetAssembly(typeof(Component));
				var types =
				  from t in assem.GetTypes().AsParallel()
				  let att = t.GetCustomAttribute<Category>()
				  where att != null
				  where PathCheck(pa, att.Paths)
				  select new { Type = t, Attribute = att };
				if (pa.Length != 0)
				{
					var comp = _list.AttachComponent<ComponentAttacherPath>();
					children.Add().Target = comp;
					comp.target.Target = this;
					comp.path.Value = "../";
				}
				var addedPaths = new List<string>();
				var attachLater = new List<ComponentAttacherAttach>();
				foreach (var item in types)
				{
					var a = item.Attribute;
					if (a.Paths.Length > pa.Length)
					{
						if (!addedPaths.Contains(a.Paths[pa.Length]))
						{
							var comp = _list.AttachComponent<ComponentAttacherPath>();
							children.Add().Target = comp;
							comp.target.Target = this;
							comp.path.Value = a.Paths[pa.Length] + "/";
							addedPaths.Add(a.Paths[pa.Length]);
						}
					}
					else
					{
						var comp = _list.AttachComponent<ComponentAttacherAttach>();
						attachLater.Add(comp);
						comp.target.Target = this;
						comp.type.Value = item.Type.FullName;
					}
				}
				foreach (var item in attachLater)
				{
					children.Add().Target = item;
				}
			}
			catch (Exception e)
			{
				Logger.Log("Failed To build Comp Minue " + e.ToString());
			}
		}

		public void AttachComponent(Type type)
		{
			if (typeof(Component).IsAssignableFrom(type))
			{
				if (type.IsGenericType)
				{
					if (!type.IsConstructedGenericType)
					{
						var constra = from t in type.GetGenericArguments() from l in t.GetGenericParameterConstraints() select l;
						if (constra.Contains(typeof(Enum)))
						{
							if (_list != null)
                            {
                                _list.Destroy();
                            }

                            _list = Entity.AddChild("CompList");
							_list.persistence.Value = false;
							var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(World.Asset.RMesh)) };
							var IConvertibleTypes =
								 from assem in assems.AsParallel()
								 from t in assem.GetTypes().AsParallel()
								 where t.IsEnum
								 select t;
							var comp = _list.AttachComponent<ComponentAttacherPath>();
							children.Add().Target = comp;
							comp.target.Target = this;
							comp.path.Value = "../";
							foreach (var item in IConvertibleTypes)
							{
								var compa = _list.AttachComponent<ComponentAttacherAttach>();
								compa.target.Target = this;
								compa.type.Value = type.MakeGenericType(item).FullName;
								children.Add().Target = compa;
							}
							path.Value += "`1";
						}
						else if (constra.Contains(typeof(IAsset)))
						{
							if (_list != null)
                            {
                                _list.Destroy();
                            }

                            _list = Entity.AddChild("CompList");
							_list.persistence.Value = false;
							var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(World.Asset.RMesh)) };
							var IConvertibleTypes =
								 from assem in assems.AsParallel()
								 from t in assem.GetTypes().AsParallel()
								 where typeof(IAsset).IsAssignableFrom(t)
								 where !t.IsEnum
								 select t;
							var comp = _list.AttachComponent<ComponentAttacherPath>();
							children.Add().Target = comp;
							comp.target.Target = this;
							comp.path.Value = "../";
							foreach (var item in IConvertibleTypes)
							{
								if (item != typeof(Enum))
								{
									var compa = _list.AttachComponent<ComponentAttacherAttach>();
									compa.target.Target = this;
									compa.type.Value = type.MakeGenericType(item).FullName;
									children.Add().Target = compa;
								}
							}
							path.Value += "`1";
						}
						else if (constra.Contains(typeof(IWorldObject)) || constra.Contains(typeof(IWorker)))
						{
							if (_list != null)
                            {
                                _list.Destroy();
                            }

                            _list = Entity.AddChild("CompList");
							_list.persistence.Value = false;
							var IConvertibleTypes =
								 from t in Assembly.GetAssembly(typeof(IWorldObject)).GetTypes().AsParallel()
                                 where typeof(Worker).IsAssignableFrom(t)
                                 where !t.IsEnum
								 select t;
							var comp = _list.AttachComponent<ComponentAttacherPath>();
							children.Add().Target = comp;
							comp.target.Target = this;
							comp.path.Value = "../";
							foreach (var item in IConvertibleTypes)
							{
								if (item != typeof(Enum))
								{
									var compa = _list.AttachComponent<ComponentAttacherAttach>();
									compa.target.Target = this;
									compa.type.Value = type.MakeGenericType(item).FullName;
									children.Add().Target = compa;
								}
							}
							path.Value += "`1";
						}
						else if (constra.Contains(typeof(IConvertible)))
						{
							if (_list != null)
                            {
                                _list.Destroy();
                            }

                            _list = Entity.AddChild("CompList");
							_list.persistence.Value = false;
							var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(string)) };
							var IConvertibleTypes =
								 from assem in assems.AsParallel()
								 from t in assem.GetTypes().AsParallel()
								 where typeof(IConvertible).IsAssignableFrom(t)
								 where !t.IsEnum
								 select t;
							var comp = _list.AttachComponent<ComponentAttacherPath>();
							children.Add().Target = comp;
							comp.target.Target = this;
							comp.path.Value = "../";
							foreach (var item in IConvertibleTypes)
							{
								if (item != typeof(Enum))
								{
									var compa = _list.AttachComponent<ComponentAttacherAttach>();
									compa.target.Target = this;
									compa.type.Value = type.MakeGenericType(item).FullName;
									children.Add().Target = compa;
								}
							}
							path.Value += "`1";
						}
						else
						{
							throw new Exception("Generic Type not suppoted yet");
						}
					}
					else
					{
						Tentity.Target?.AttachComponent(type);
						Entity.Destroy();
					}
				}
				else
				{
					Tentity.Target?.AttachComponent(type);
					Entity.Destroy();
				}
			}
			else
			{
				throw new Exception("Not Component");
			}
		}

		public ComponentAttacher(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ComponentAttacher()
		{
		}

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			foreach (var item in children)
			{
				item.Target?.ImguiRender(imGuiRenderer, canvas);
			}
		}
	}
}