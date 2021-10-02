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
using BulletSharp;
using BulletSharp.Math;
using Veldrid;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.World.Asset;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Input;

namespace RhubarbEngine.Components.Physics
{

	[Category(new string[] { "Physics/Intraction" })]
	public class MeshInputPlane : Collider, IInputPlane
	{
		public AssetRef<RMesh> mesh;

		public Sync<Vector2u> pixelSize;

		public Sync<bool> FocusedOverride;

		public SyncDelegate onFocusLost;

		public IReadOnlyList<KeyEvent> KeyEvents { get { return !Focused ? new List<KeyEvent>() : Input.MainWindows.FrameSnapshot.KeyEvents; } }

		public IReadOnlyList<MouseEvent> MouseEvents { get { return IsNotTakingInput ? new List<MouseEvent>() : Input.MainWindows.FrameSnapshot.MouseEvents; } }

		public IReadOnlyList<char> KeyCharPresses { get { return !Focused ? new List<char>() : Input.MainWindows.FrameSnapshot.KeyCharPresses; } }

		private Vector2 _mousePosition = Vector2.Zero;

        public Vector2 MousePosition
        {
            get
            {
                return _mousePosition;
            }
        }

        public void SetCursor(Cursors cursor)
		{
			if (!IsNotTakingInput)
			{
				switch (Source)
				{
					case InteractionSource.LeftLaser:
						Input.LeftLaser.Cursor = cursor;
						break;
					case InteractionSource.RightLaser:
						Input.RightLaser.Cursor = cursor;
						break;
					case InteractionSource.HeadLaser:
						Input.RightLaser.Cursor = cursor;
						break;
					default:
						break;
				}
			}
		}
		public void Click(Vector2 pos, InteractionSource sourc)
		{
			Setfocused();
			Source = sourc;
			_mousePosition = pos;
			StopMousePos = false;
		}

		public void UpdatePos(Vector2 pos, InteractionSource sourc)
		{
			if (sourc != Source)
            {
                return;
            }

            if (!_focused && !Input.IsKeyboardinuse)
			{
				Setfocused();
				StopMousePos = false;
			}
			if (StopMousePos)
            {
                return;
            }

            _mousePosition = pos;
			_hover = 0;
		}

		public float WheelDelta
		{
			get
			{
                return IsNotTakingInput ? 0f : Input.MainWindows.FrameSnapshot.WheelDelta;
            }
        }

        public InteractionSource Source { get; private set; } = InteractionSource.RightLaser;

        public bool IsNotTakingInput
        {
            get
            {
                return !_focused || (_hover > 3);
            }
        }

        private byte _hover = 0;

		private bool _focused = false;

        public bool Focused
        {
            get
            {
                return Input.IsKeyboardinuse ? _focused : !IsNotTakingInput;
            }
        }

        public bool StopMousePos = false;

		public bool StopMouse { get { return StopMousePos; } set { StopMousePos = value; } }

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
            pixelSize = new Sync<Vector2u>(this, newRefIds)
            {
                Value = new Vector2u(600, 600)
            };
            FocusedOverride = new Sync<bool>(this, newRefIds);
			onFocusLost = new SyncDelegate(this, newRefIds);
			mesh = new AssetRef<RMesh>(this, newRefIds);
			mesh.LoadChange += Mesh_loadChange;
			Entity.EnabledChanged += Entity_enabledChanged;
		}

		private void Entity_enabledChanged()
		{
			if ((!Entity.IsEnabled) && Focused)
			{
				Removefocused();
			}
		}

		private void Mesh_loadChange(RMesh obj)
		{
			BuildShape();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			BuildShape();
		}
		private void GoNull()
		{
			BuildCollissionObject(null);
		}
		public int[] index = Array.Empty<int>();
		public BulletSharp.Math.Vector3[] vertices = Array.Empty<BulletSharp.Math.Vector3>();


		public unsafe override void BuildShape()
		{
			if (mesh.Asset == null)
			{ GoNull(); return; };
			if (!mesh.Target?.loaded ?? false)
			{ GoNull(); return; };

			// Initialize TriangleIndexVertexArray with Vector3 array
			vertices = new BulletSharp.Math.Vector3[mesh.Asset.Meshes[0].VertexCount];
			for (var i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new BulletSharp.Math.Vector3(
					(float)mesh.Asset.Meshes[0].GetVertex(i).x,
                    (float)mesh.Asset.Meshes[0].GetVertex(i).y,
                    (float)mesh.Asset.Meshes[0].GetVertex(i).z);
			}
			var e = mesh.Asset.Meshes[0].RenderIndices().ToArray();

			// Initialize TriangleIndexIndexArray with int array
			index = new int[e.Length];
			for (var i = 0; i < index.Length; i++)
			{
				index[i] = e[i];
			}
			if (index.Length < 3)
            {
                return;
            }

            var indexVertexArray2 = new TriangleIndexVertexArray(index, vertices);
			var trys = new BvhTriangleMeshShape(indexVertexArray2, false);
			StartShape(trys);
		}
		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
			base.CommonUpdate(startTime, Frame);
			if (_hover > 3)
            {
                return;
            }

            _hover++;
		}
		public bool IsMouseDown(MouseButton button)
		{
			if (StopMousePos)
            {
                return false;
            }

            if (IsNotTakingInput)
            {
                return false;
            }

            if (Engine.OutputType == VirtualReality.OutputType.Screen)
			{
				return Engine.InputManager.MainWindows.GetMouseButton(button);
			}
			switch (Source)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					switch (button)
					{
						case MouseButton.Left:
							// need to make not pur frame
							return Input.PrimaryPress(RhubarbEngine.Input.Creality.Left);
						case MouseButton.Middle:
							return Input.SecondaryPress(RhubarbEngine.Input.Creality.Left);
						case MouseButton.Right:
							return Input.GrabPress(RhubarbEngine.Input.Creality.Left);
						case MouseButton.Button1:
							break;
						case MouseButton.Button2:
							break;
						case MouseButton.Button3:
							break;
						case MouseButton.Button4:
							break;
						case MouseButton.Button5:
							break;
						case MouseButton.Button6:
							break;
						case MouseButton.Button7:
							break;
						case MouseButton.Button8:
							break;
						case MouseButton.Button9:
							break;
						case MouseButton.LastButton:
							break;
						default:
							break;
					}
					break;
				case InteractionSource.LeftFinger:
					break;
				case InteractionSource.RightLaser:
					switch (button)
					{
						case MouseButton.Left:
							return Input.PrimaryPress(RhubarbEngine.Input.Creality.Right);
						case MouseButton.Middle:
							return Input.SecondaryPress(RhubarbEngine.Input.Creality.Right);
						case MouseButton.Right:
							return Input.GrabPress(RhubarbEngine.Input.Creality.Right);
						case MouseButton.Button1:
							break;
						case MouseButton.Button2:
							break;
						case MouseButton.Button3:
							break;
						case MouseButton.Button4:
							break;
						case MouseButton.Button5:
							break;
						case MouseButton.Button6:
							break;
						case MouseButton.Button7:
							break;
						case MouseButton.Button8:
							break;
						case MouseButton.Button9:
							break;
						case MouseButton.LastButton:
							break;
						default:
							break;
					}
					break;
				case InteractionSource.RightFinger:
					break;
				case InteractionSource.HeadLaser:
					return Engine.InputManager.MainWindows.GetMouseButton(button);
				case InteractionSource.HeadFinger:
					break;
				default:
					break;
			}
			return false;
		}

		public void Setfocused()
		{
			if (_focused)
			{
				return;
			}
			_focused = true;
			if (!FocusedOverride.Value)
			{
				Input.InvokeRemoveFocus();
			}
			Input.RemoveFocus += Removefocused;
		}

		public void Removefocused()
		{
			onFocusLost.Target?.Invoke();
			Input.RemoveFocus -= Removefocused;
			_focused = false;
		}

		public MeshInputPlane(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MeshInputPlane()
		{
		}
	}
}
