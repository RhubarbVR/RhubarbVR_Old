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
using g3;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;
using Veldrid;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics.Colliders;
using RhubarbEngine.Input;

namespace RhubarbEngine.Components.Physics
{

	[Category(new string[] { "Physics/Intraction" })]
	public class InputPlane : Collider, IinputPlane
	{
		public Sync<Vector2f> size;

		public Sync<Vector2u> pixelSize;

		public Sync<float> depth;

		public Sync<bool> FocusedOverride;

		public SyncDelegate onFocusLost;
		public IReadOnlyList<KeyEvent> KeyEvents { get { return !focused ? new List<KeyEvent>() : input.mainWindows.FrameSnapshot.KeyEvents; } }

		public IReadOnlyList<MouseEvent> MouseEvents { get { return IsNotTakingInput ? new List<MouseEvent>() : input.mainWindows.FrameSnapshot.MouseEvents; } }

		public IReadOnlyList<char> KeyCharPresses { get { return !focused ? new List<char>() : input.mainWindows.FrameSnapshot.KeyCharPresses; } }

		private Vector2 _mousePosition = Vector2.Zero;

        public Vector2 MousePosition
        {
            get
            {
                return _mousePosition;
            }
        }

        public void Click(Vector2 pos, InteractionSource sourc)
		{
			Setfocused();
			source = sourc;
			_mousePosition = pos;
			StopMousePos = false;
		}

		public void UpdatePos(Vector2 pos, InteractionSource sourc)
		{
			if (sourc != source)
            {
                return;
            }

            if (!_focused && !input.IsKeyboardinuse)
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
                return IsNotTakingInput ? 0f : input.mainWindows.FrameSnapshot.WheelDelta;
            }
        }

        public InteractionSource source { get; private set; } = InteractionSource.RightLaser;

        private bool _focused = false;

        public bool IsNotTakingInput
        {
            get
            {
                return !_focused || (_hover > 3);
            }
        }

        private byte _hover = 0;

        public bool focused
        {
            get
            {
                return input.IsKeyboardinuse ? _focused : !IsNotTakingInput;
            }
        }

        public bool StopMousePos = false;

		public bool StopMouse { get { return StopMousePos; } set { StopMousePos = value; } }

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
            size = new Sync<Vector2f>(this, newRefIds)
            {
                Value = new Vector2f(0.5f)
            };
            depth = new Sync<float>(this, newRefIds)
            {
                Value = 0.01f
            };
            pixelSize = new Sync<Vector2u>(this, newRefIds)
            {
                Value = new Vector2u(600, 600)
            };
            FocusedOverride = new Sync<bool>(this, newRefIds);
			onFocusLost = new SyncDelegate(this, newRefIds);
			size.Changed += UpdateChange;
		}

		public void UpdateChange(IChangeable val)
		{
			BuildShape();
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

		public override void onLoaded()
		{
			base.onLoaded();
			BuildShape();
			entity.EnabledChanged += Entity_enabledChanged;
		}

		private void Entity_enabledChanged()
		{
			if ((!entity.IsEnabled) && focused)
			{
				Removefocused();
			}
		}

		public override void BuildShape()
		{
			StartShape(new BoxShape(new BulletSharp.Math.Vector3(size.Value.x, depth.Value, size.Value.y)));
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

            if (engine.outputType == VirtualReality.OutputType.Screen)
			{
				return engine.inputManager.mainWindows.GetMouseButton(button);
			}
			switch (source)
			{
				case InteractionSource.None:
					break;
				case InteractionSource.LeftLaser:
					switch (button)
					{
						case MouseButton.Left:
							return input.PrimaryPress(Input.Creality.Left);
						case MouseButton.Middle:
							return input.SecondaryPress(Input.Creality.Left);
						case MouseButton.Right:
							return input.GrabPress(Input.Creality.Left);
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
							//Need to make not a single frame
							return input.PrimaryPress(Input.Creality.Right);
						case MouseButton.Middle:
							return input.SecondaryPress(Input.Creality.Right);
						case MouseButton.Right:
							return input.GrabPress(Input.Creality.Right);
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
					return engine.inputManager.mainWindows.GetMouseButton(button);
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
				input.InvokeRemoveFocus();
			}
			input.RemoveFocus += Removefocused;
		}

		public void Removefocused()
		{
			onFocusLost.Target?.Invoke();
			input.RemoveFocus -= Removefocused;
			_focused = false;
		}

		public void SetCursor(Cursors cursor)
		{
			if (!IsNotTakingInput)
			{
				switch (source)
				{
					case InteractionSource.LeftLaser:
						input.LeftLaser.cursor = cursor;
						break;
					case InteractionSource.RightLaser:
						input.RightLaser.cursor = cursor;
						break;
					case InteractionSource.HeadLaser:
						input.RightLaser.cursor = cursor;
						break;
					default:
						break;
				}
			}
		}

		public InputPlane(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public InputPlane()
		{
		}
	}
}
