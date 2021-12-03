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
using RNumerics;
using System.Numerics;
using RhubarbEngine.Components.Transform;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Assets;
using RhubarbEngine.Components.Assets.Procedural_Meshes;
using RhubarbEngine.Components.Rendering;
using RhubarbEngine.Components.Color;
using RhubarbEngine.Components.Users;
using RhubarbEngine.Components.ImGUI;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Components.PrivateSpace;
using RhubarbEngine.Components.Interaction;
using Veldrid;

namespace RhubarbEngine.Components.Interaction
{
    [Category(new string[] { "Interaction" })]
    public class GrabbableHolder : Component
	{
		public SyncRef<Entity> holder;

		public SyncRef<IWorldObject> Referencer;

        public SyncRef<Entity> RefrencerEntity;

        public SyncRef<User> user;

		public Sync<InteractionSource> source;

        public List<Grabbable> GrabbedObjects = new();

        bool _gripping = false;

        public void DeleteGrabObjects()
        {
            foreach (var item in GrabbedObjects)
            {
                item.DestroyGrabbedObject();
            }
        }

        public bool CanDestroyAnyGabbed {
            get 
            {
                if(GrabbedObjects.Count <= 0)
                {
                    return false;
                }
                foreach (var item in GrabbedObjects)
                {
                    if (!item.CanNotDestroy.Value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public IWorldObject HolderReferen
        {
            get
            {
                if ((World.HeadLaserGrabbableHolder.Referencer.Target != this)&& World.HeadLaserGrabbableHolder.Referencer.Target is not null)
                {
                    if(World.HeadLaserGrabbableHolder.Referencer.Target is not null)
                    {
                        return World.HeadLaserGrabbableHolder.Referencer.Target;
                    }
                }
                return Referencer.Target;
            }
        }

        public void InitializeGrabHolder(InteractionSource _source)
		{
			user.Target = World.LocalUser;
			source.Value = _source;
			switch (_source)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					World.LeftLaserGrabbableHolder = this;
					break;
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					World.RightLaserGrabbableHolder = this;
					break;
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					World.HeadLaserGrabbableHolder = this;
					break;
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
		}

        public bool DropedRef
        {
            get
            {
                return (_timeout <= 4) && (_timeout != 0) && !_gripping;
            }
        }

        byte _timeout = 0;
        bool _primeClick = false;

		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (user.Target == null)
            {
                return;
            }

            if (holder.Target == null)
            {
                return;
            }

            if (user.Target != World.LocalUser)
            {
                return;
            }

            var isClickingPrime = false;
            switch (source.Value)
            {
                case InteractionSource.LeftLaser:
                    isClickingPrime = Input.PrimaryPress(RhubarbEngine.Input.Creality.Left) || Input.MainWindows.GetMouseButton(Veldrid.MouseButton.Left);
                    break;
                case InteractionSource.RightLaser:
                    isClickingPrime = Input.PrimaryPress(RhubarbEngine.Input.Creality.Right) || Input.MainWindows.GetMouseButton(Veldrid.MouseButton.Left);
                    break;
                case InteractionSource.HeadLaser:
                    isClickingPrime = Input.MainWindows.GetMouseButton(Veldrid.MouseButton.Left);
                    break;
                default:
                    break;
            }
            if ((_primeClick != isClickingPrime) && isClickingPrime)
            {
                try
                {
                    HolderReferen?.OpenWindow();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed To open Window Error:" + e.ToString());
                }
            }
            _primeClick = isClickingPrime;
            if (source.Value == InteractionSource.HeadLaser)
			{
				var mousepos = Engine.InputManager.MainWindows.MousePosition;
				var size = new System.Numerics.Vector2(Engine.WindowManager.MainWindow?.Width??640, Engine.WindowManager.MainWindow?.Height??640);
				var x = (2.0f * mousepos.X / size.X) - 1.0f;
				var y = (2.0f * mousepos.Y / size.Y) - 1.0f;
				var ar = size.X / size.Y;
				var tan = (float)Math.Tan(Engine.SettingsObject.RenderSettings.DesktopRenderSettings.fov * Math.PI / 360);
				var vectforward = new Vector3f(-x * tan * ar, y * tan, 1);
				var vectup = new Vector3f(0, 1, 0);
				holder.Target.rotation.Value = Quaternionf.LookRotation(vectforward, vectup);
			}
			if (OnGriping() != _gripping)
			{
				_gripping = !_gripping;
				if (!_gripping)
				{

					foreach (var child in holder.Target._children.GetCopy())
					{
						foreach (var grab in child.GetAllComponents<Grabbable>())
						{
							grab.Drop();
						}
					}
					switch (source.Value)
					{
						case InteractionSource.LeftLaser:
							Input.LeftLaser.UnLock();
							break;
						case InteractionSource.RightLaser:
							Input.RightLaser.UnLock();
							break;
						case InteractionSource.HeadLaser:
							Input.RightLaser.UnLock();
							break;
						default:
							break;
					}
                    GrabbedObjects.Clear();
                }
			}
			if (Referencer.Target == null)
            {
                return;
            }

            if (!_gripping)
			{
				_timeout++;
				if (_timeout > 4)
				{
					Referencer.Target = null;
                }
			}
		}

		private bool OnGriping()
		{
			if ((Engine.OutputType == VirtualReality.OutputType.Screen) && (source.Value == InteractionSource.RightLaser))
			{
				return Engine.InputManager.MainWindows.GetMouseButton(MouseButton.Right);
			}
			switch (source.Value)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					return Input.GrabPress(RhubarbEngine.Input.Creality.Left);
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					return Input.GrabPress(RhubarbEngine.Input.Creality.Right);
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					return Engine.InputManager.MainWindows.GetMouseButton(MouseButton.Right);
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
			return false;
		}

		public override void OnAttach()
		{
			base.OnAttach();
			holder.Target = Entity.AddChild("Holder");
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			holder = new SyncRef<Entity>(this, newRefIds);
			user = new SyncRef<User>(this, newRefIds);
			source = new Sync<InteractionSource>(this, newRefIds);
			Referencer = new SyncRef<IWorldObject>(this, newRefIds);
			Referencer.Changed += Referencer_Changed;
            RefrencerEntity = new SyncRef<Entity>(this, newRefIds);
        }

		private void Referencer_Changed(IChangeable obj)
		{
			_timeout = 0;
			Console.WriteLine("Referencer Changed To " + Referencer.Target?.ReferenceID.id.ToHexString());
            RefrencerEntity.Target?.Destroy();
            if (Referencer.Target is not null)
            {
                var entity = ((Engine.OutputType == VirtualReality.OutputType.Screen)? World.HeadLaserGrabbableHolder.Entity:Entity).AddChild("Referencer Visual");
                RefrencerEntity.Target = entity;
                var looke = entity.AttachComponent<LookAtUser>();
                looke.offset.Value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
                entity.position.Value = (Engine.OutputType == VirtualReality.OutputType.Screen) ? new Vector3f(0.15f, -0.15f, -0.6f) : new Vector3f(0, 0, -0.5f);
                entity.scale.Value = (Engine.OutputType == VirtualReality.OutputType.Screen) ? new Vector3f(0.02f) : new Vector3f(0.04f);
                looke.UpdateRotation();
                var text = entity.AttachComponent<UpdateingTextPlane>();
                text.TextColor.Value = Colorf.ForestGreen;
                if (Referencer.Target.GetType().IsAssignableTo(typeof(Entity)))
                {
                    text.Text.Value = $"Entity: {((Entity)Referencer.Target).name.Value}{Referencer.Target.GetFieldName()} Parent {(((Entity)Referencer.Target).parent.Target?.name.Value??"Null")} (ID:{Referencer.Target.ReferenceID.id.ToHexString()})";
                }
                else
                {
                    text.Text.Value = $"{Referencer.Target.GetType().GetFormattedName()}{Referencer.Target.GetFieldName()} on {Referencer.Target.GetExtendedNameString()} (ID:{Referencer.Target.ReferenceID.id.ToHexString()})";
                }
                var Visual = entity.AddChild("Visual");
                Visual.scale.Value = new Vector3f(6.5);
                Visual.position.Value = new Vector3f(-2.5f, 0,-4f);
                if (Referencer.Target.GetType().IsAssignableTo(typeof(AssetProvider<RTexture2D>)))
                {
                    LoadTextureVisual(Visual,(AssetProvider<RTexture2D>)Referencer.Target);
                }
                if (Referencer.Target.GetType().IsAssignableTo(typeof(AssetProvider<RMaterial>)))
                {
                    LoadMaterialVisual(Visual, (AssetProvider<RMaterial>)Referencer.Target);
                }
                if (Referencer.Target.GetType().IsAssignableTo(typeof(AssetProvider<RMesh>)))
                {
                    LoadMeshVisual(Visual, (AssetProvider<RMesh>)Referencer.Target);
                }
            }
        }

        private void LoadTextureVisual(Entity entity, AssetProvider<RTexture2D> texture)
        {
            var plane = Helpers.MeshHelper.AddMeshToEntity<PlaneMesh>(entity, texture);
            var driver = entity.AttachComponent<TextureRatioDriver>();
            driver.WidthRatio.Target = plane.Width;
            driver.texture.Target = texture;
        }

        private void LoadMaterialVisual(Entity entity, AssetProvider<RMaterial> mit)
        {
            Helpers.MeshHelper.AddMeshToEntity<SphereMesh>(entity, mit);

        }

        private void LoadMeshVisual(Entity entity, AssetProvider<RMesh> mesh)
        {
            var mit = entity.AttachComponent<RMaterial>();
            mit.Shader.Target = World.staticAssets.BasicUnlitShader;
            mit.SetValueAtField("TintColor", Render.Shader.ShaderType.MainFrag, new Colorf(161, 16, 193, 20));
            var(shear,render) =Helpers.MeshHelper.AddMeshToEntityGetRender<SphereMesh>(entity, mit);
            render.RenderOrderOffset.Value = ((uint)int.MaxValue) - 1;
            var spin = entity.AddChild("spin");
            spin.AttachComponent<Spinner>().speed.Value = new Vector3f(1);
            var e = spin.AddChild("mesh");
            var mite = entity.AttachComponent<RMaterial>();
            mite.Shader.Target = World.staticAssets.WireFrameShader;
            e.AttachComponent<CenteredMeshDisplay>().Mesh.Target = mesh;
            var meshRender = e.AttachComponent<MeshRender>();
            meshRender.Materials.Add().Target = mite;
            meshRender.Mesh.Target = mesh;

        }

        public GrabbableHolder(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public GrabbableHolder()
		{
		}
	}
}
