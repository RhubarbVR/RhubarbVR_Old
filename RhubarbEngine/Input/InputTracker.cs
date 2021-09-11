using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.Sdl2;
using Veldrid;

namespace RhubarbEngine.Input
{
	public class CustomFrame : InputSnapshot
	{
		public List<KeyEvent> keyEvents;
		public List<KeyEvent> UpkeyEvents = new List<KeyEvent>();

		public IReadOnlyList<KeyEvent> KeyEvents => keyEvents;
		public List<MouseEvent> mouseEvents;
		public List<MouseEvent> UpmouseEvents = new List<MouseEvent>();
		public IReadOnlyList<MouseEvent> MouseEvents => mouseEvents;
		public List<char> UpkeyCharPresses = new List<char>();

		public List<char> keyCharPresses;
		public IReadOnlyList<char> KeyCharPresses => keyCharPresses;

		public Vector2 mousePosition;
		public Vector2 MousePosition => mousePosition;

		public float wheelDelta;
		public float WheelDelta => wheelDelta;

		public bool IsMouseDown(MouseButton button)
		{
			foreach (var item in mouseEvents)
			{
				if (item.MouseButton == button)
				{
					return item.Down;
				}
			}
			return false;
		}

		public void MouseClick(MouseButton mouseButton)
		{
			UpmouseEvents.Add(new MouseEvent(mouseButton, true));
		}

		public void CustomFram(InputSnapshot s)
		{
			keyEvents = new List<KeyEvent>(s.KeyEvents);
			foreach (var item in UpkeyEvents)
			{
				keyEvents.Add(item);
			}
			List<KeyEvent> ew = new List<KeyEvent>();
			foreach (var item in UpkeyEvents)
			{
				if (item.Down)
				{
					ew.Add(new KeyEvent(item.Key, false, item.Modifiers));
				}
			}
			UpkeyEvents.Clear();
			foreach (var item in ew)
			{
				UpkeyEvents.Add(item);
			}
			mouseEvents = new List<MouseEvent>(s.MouseEvents);

			foreach (var item in UpmouseEvents)
			{
				mouseEvents.Add(item);
			}
			List<MouseEvent> ewm = new List<MouseEvent>();
			foreach (var item in UpmouseEvents)
			{
				if (item.Down)
				{
					ewm.Add(new MouseEvent(item.MouseButton, false));
				}
			}
			UpmouseEvents.Clear();
			foreach (var item in ewm)
			{
				UpmouseEvents.Add(item);
			}

			keyCharPresses = new List<char>(s.KeyCharPresses);
			foreach (var item in UpkeyCharPresses)
			{
				keyCharPresses.Add(item);
			}
			UpkeyCharPresses.Clear();
			wheelDelta = s.WheelDelta;
			mousePosition = s.MousePosition;
		}

		public void PressChar(char key, ModifierKeys e)
		{
			UpkeyCharPresses.Add(key);
			Key ekey = Key.Unknown;
			switch (key)
			{
				case 'A':
					ekey = Key.A;
					e |= ModifierKeys.Shift;
					break;
				case 'a':
					ekey = Key.A;
					break;
				case 'B':
					ekey = Key.B;
					e |= ModifierKeys.Shift;
					break;
				case 'b':
					ekey = Key.B;
					break;
				case 'D':
					ekey = Key.D;
					e |= ModifierKeys.Shift;
					break;
				case 'd':
					ekey = Key.D;
					break;
				case 'E':
					ekey = Key.E;
					e |= ModifierKeys.Shift;
					break;
				case 'e':
					ekey = Key.E;
					break;
				case 'F':
					ekey = Key.F;
					e |= ModifierKeys.Shift;
					break;
				case 'f':
					ekey = Key.F;
					break;
				case 'g':
					ekey = Key.G;
					break;
				case 'G':
					ekey = Key.G;
					e |= ModifierKeys.Shift;
					break;
				case 'H':
					ekey = Key.H;
					e |= ModifierKeys.Shift;
					break;
				case 'h':
					ekey = Key.H;
					break;
				case 'I':
					ekey = Key.I;
					e |= ModifierKeys.Shift;
					break;
				case 'i':
					ekey = Key.I;
					break;
				case 'j':
					ekey = Key.J;
					e |= ModifierKeys.Shift;
					break;
				case 'J':
					ekey = Key.J;
					break;
				case 'K':
					ekey = Key.K;
					e |= ModifierKeys.Shift;
					break;
				case 'k':
					ekey = Key.K;
					break;
				case 'L':
					ekey = Key.L;
					e |= ModifierKeys.Shift;
					break;
				case 'l':
					ekey = Key.L;
					break;
				case 'M':
					ekey = Key.M;
					e |= ModifierKeys.Shift;
					break;
				case 'm':
					ekey = Key.M;
					break;
				case 'N':
					ekey = Key.N;
					e |= ModifierKeys.Shift;
					break;
				case 'n':
					ekey = Key.N;
					break;
				case 'O':
					ekey = Key.O;
					e |= ModifierKeys.Shift;
					break;
				case 'o':
					ekey = Key.O;
					break;
				case 'P':
					ekey = Key.P;
					e |= ModifierKeys.Shift;
					break;
				case 'p':
					ekey = Key.P;
					break;
				case 'Q':
					ekey = Key.Q;
					e |= ModifierKeys.Shift;
					break;
				case 'q':
					ekey = Key.Q;
					break;
				case 'R':
					ekey = Key.R;
					e |= ModifierKeys.Shift;
					break;
				case 'r':
					ekey = Key.R;
					break;
				case 'S':
					ekey = Key.S;
					e |= ModifierKeys.Shift;
					break;
				case 's':
					ekey = Key.S;
					break;
				case 'T':
					ekey = Key.T;
					e |= ModifierKeys.Shift;
					break;
				case 't':
					ekey = Key.T;
					break;
				case 'U':
					ekey = Key.U;
					e |= ModifierKeys.Shift;
					break;
				case 'u':
					ekey = Key.U;
					break;
				case 'V':
					ekey = Key.V;
					e |= ModifierKeys.Shift;
					break;
				case 'v':
					ekey = Key.V;
					break;
				case 'W':
					ekey = Key.W;
					e |= ModifierKeys.Shift;
					break;
				case 'w':
					ekey = Key.W;
					break;
				case 'Z':
					ekey = Key.Z;
					e |= ModifierKeys.Shift;
					break;
				case 'z':
					ekey = Key.Z;
					break;
				case 'Y':
					ekey = Key.Y;
					e |= ModifierKeys.Shift;
					break;
				case 'y':
					ekey = Key.Y;
					break;
				case ' ':
					ekey = Key.Space;
					break;
				case '\n':
					ekey = Key.Enter;
					break;
				default:
					break;
			}
			PressKey(ekey, e);
		}
		public void PressKey(Key key, ModifierKeys e)
		{
			UpkeyEvents.Add(new KeyEvent(key, true, e));
		}

	}


	public class InputTracker
	{
		private HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
		private HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

		private HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
		private HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

		public Vector2 MousePosition;
		public Vector2 MouseDelta;

		public CustomFrame FrameSnapshot { get; private set; } = new CustomFrame();


		public bool GetKey(Key key)
		{
			return _currentlyPressedKeys.Contains(key);
		}

		public bool GetKeyDown(Key key)
		{
			return _newKeysThisFrame.Contains(key);
		}

		public bool GetMouseButton(MouseButton button)
		{
			return _currentlyPressedMouseButtons.Contains(button);
		}

		public bool GetMouseButtonDown(MouseButton button)
		{
			return _newMouseButtonsThisFrame.Contains(button);
		}

		public void UpdateFrameInput(InputSnapshot snapshot, Sdl2Window window)
		{
			FrameSnapshot.CustomFram(snapshot);
			_newKeysThisFrame.Clear();
			_newMouseButtonsThisFrame.Clear();

			MousePosition = snapshot.MousePosition;
			MouseDelta = window.MouseDelta;
			for (int i = 0; i < snapshot.KeyEvents.Count; i++)
			{
				KeyEvent ke = snapshot.KeyEvents[i];
				if (ke.Down)
				{
					KeyDown(ke.Key);
				}
				else
				{
					KeyUp(ke.Key);
				}
			}
			for (int i = 0; i < snapshot.MouseEvents.Count; i++)
			{
				MouseEvent me = snapshot.MouseEvents[i];
				if (me.Down)
				{
					MouseDown(me.MouseButton);
				}
				else
				{
					MouseUp(me.MouseButton);
				}
			}
		}

		private void MouseUp(MouseButton mouseButton)
		{
			_currentlyPressedMouseButtons.Remove(mouseButton);
			_newMouseButtonsThisFrame.Remove(mouseButton);
		}

		private void MouseDown(MouseButton mouseButton)
		{
			if (_currentlyPressedMouseButtons.Add(mouseButton))
			{
				_newMouseButtonsThisFrame.Add(mouseButton);
			}
		}

		private void KeyUp(Key key)
		{
			_currentlyPressedKeys.Remove(key);
			_newKeysThisFrame.Remove(key);
		}

		private void KeyDown(Key key)
		{
			if (_currentlyPressedKeys.Add(key))
			{
				_newKeysThisFrame.Add(key);
			}
		}
	}
}
