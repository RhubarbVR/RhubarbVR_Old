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

		public SyncRef<User> user;

		public Sync<InteractionSource> source;

		bool _gripping = false;

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
				}
				else
				{
					World.lastHolder = this;
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
		}

		private void Referencer_Changed(IChangeable obj)
		{
			_timeout = 0;
			Console.WriteLine("Changed To " + Referencer.Target?.ReferenceID.id.ToHexString());
		}

		public GrabbableHolder(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public GrabbableHolder()
		{
		}
	}
}
