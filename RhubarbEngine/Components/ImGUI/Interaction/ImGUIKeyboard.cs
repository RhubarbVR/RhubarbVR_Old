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
using ImGuiNET;
using Veldrid;

namespace RhubarbEngine.Components.ImGUI
{

	[Category("ImGUI/Interaction")]
	public class ImGUIKeyboard : ImGUIBeginChild
	{
		public SyncRef<ImGUIButtonRow> rowOne;

		public SyncRef<ImGUIButtonRow> rowTwo;

		public SyncRef<ImGUIButtonRow> rowThree;

		public SyncRef<ImGUIButtonRow> rowFour;

		public SyncRef<ImGUIButtonRow> rowFive;

		public SyncRef<ImGUIButtonRow> rowSix;

		public Sync<bool> shift;

		public Sync<bool> ctrl;

		public Sync<bool> alt;

		public Sync<bool> gui;

		public Sync<bool> caps;

		public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
		{
			ImGui.Dummy(new Vector2(0, 10));
			if (ImGui.BeginChild(id.Value ?? "", new Vector2(size.Value.x, size.Value.y), border.Value, windowflag.Value))
			{
				foreach (var item in children)
				{
					item.Target?.ImguiRender(imGuiRenderer, canvas);
				}
				ImGui.EndChild();
			}
		}

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			rowOne = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			rowTwo = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			rowThree = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			rowFour = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			rowFive = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			rowSix = new SyncRef<ImGUIButtonRow>(this, newRefIds);
			shift = new Sync<bool>(this, newRefIds);
			ctrl = new Sync<bool>(this, newRefIds);
			alt = new Sync<bool>(this, newRefIds);
			gui = new Sync<bool>(this, newRefIds);
			caps = new Sync<bool>(this, newRefIds);
		}

		private void clickKey(string key)
		{
			ModifierKeys mkey = ModifierKeys.None;
			if (shift.Value)
			{
				mkey |= ModifierKeys.Shift;
				shift.Value = false;
			}
			if (ctrl.Value)
			{
				mkey |= ModifierKeys.Control;
			}
			if (alt.Value)
			{
				mkey |= ModifierKeys.Alt;
			}
			if (gui.Value)
			{
				mkey |= ModifierKeys.Gui;
			}
			var i = Input.mainWindows.FrameSnapshot;
			if (key.Length > 1)
			{
				switch (key)
				{
					case "F1":
						i.PressKey(Key.F1, mkey);
						break;
					case "F2":
						i.PressKey(Key.F2, mkey);
						break;
					case "F3":
						i.PressKey(Key.F3, mkey);
						break;
					case "F4":
						i.PressKey(Key.F4, mkey);
						break;
					case "F5":
						i.PressKey(Key.F5, mkey);
						break;
					case "F6":
						i.PressKey(Key.F6, mkey);
						break;
					case "F7":
						i.PressKey(Key.F7, mkey);
						break;
					case "F8":
						i.PressKey(Key.F8, mkey);
						break;
					case "F9":
						i.PressKey(Key.F9, mkey);
						break;
					case "F10":
						i.PressKey(Key.F10, mkey);
						break;
					case "F11":
						i.PressKey(Key.F11, mkey);
						break;
					case "F12":
						i.PressKey(Key.F12, mkey);
						break;
					case "Esc":
						i.PressKey(Key.Escape, mkey);
						break;
					case "<--":
						i.PressKey(Key.BackSpace, mkey);
						break;
					case "Tab":
						i.PressKey(Key.Tab, mkey);
						break;
					case "sHIFT":
						i.PressKey(Key.ShiftLeft, mkey);
						shift.Value = false;
						ReloadKeyboard();
						break;
					case "Shift":
						i.PressKey(Key.ShiftLeft, mkey);
						shift.Value = true;
						ReloadKeyboard();
						break;
					case "Enter":
						Input.mainWindows.FrameSnapshot.PressChar('\n', mkey);
						break;
					case "Space":
						Input.mainWindows.FrameSnapshot.PressChar(' ', mkey);
						break;
					case "Caps\nLock":
						i.PressKey(Key.F1, mkey);
						caps.Value = !caps.Value;
						ReloadKeyboard();
						break;
					case "Ctrl":
						i.PressKey(Key.ControlLeft, mkey);
						ctrl.Value = !ctrl.Value;
						break;
					case "Alt":
						i.PressKey(Key.AltLeft, mkey);
						alt.Value = !alt.Value;
						break;
					case "<=":
						i.PressKey(Key.Left, mkey);
						break;
					case "=>":
						i.PressKey(Key.Right, mkey);
						break;
					case "/\\":
						i.PressKey(Key.Up, mkey);
						break;
					case "\\/":
						i.PressKey(Key.Down, mkey);
						break;
					default:
						break;
				}
			}
			else
			{
				Input.mainWindows.FrameSnapshot.PressChar(key.ToCharArray()[0], mkey);
			}

		}

		public override void OnAttach()
		{
			base.OnAttach();
			rowOne.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowTwo.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowThree.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowFour.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowFive.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowSix.Target = Entity.AttachComponent<ImGUIButtonRow>();
			rowOne.Target.action.Target = clickKey;
			rowTwo.Target.action.Target = clickKey;
			rowThree.Target.action.Target = clickKey;
			rowFour.Target.action.Target = clickKey;
			rowFive.Target.action.Target = clickKey;
			rowSix.Target.action.Target = clickKey;
			float val = 0.125f;
			rowOne.Target.hight.Value = val;
			rowTwo.Target.hight.Value = val;
			rowThree.Target.hight.Value = val;
			rowFour.Target.hight.Value = val;
			rowFive.Target.hight.Value = val;
			rowSix.Target.hight.Value = val;
			children.Add().Target = rowOne.Target;
			children.Add().Target = rowTwo.Target;
			children.Add().Target = rowThree.Target;
			children.Add().Target = rowFour.Target;
			children.Add().Target = rowFive.Target;
			children.Add().Target = rowSix.Target;
			ReloadKeyboard();
		}

		public void ReloadKeyboard()
		{
			BuildRows(shift.Value || caps.Value);
		}

		private void BuildRows(bool shift)
		{
			BuildRowOne(shift);
			BuildRowTwo(shift);
			BuildRowThree(shift);
			BuildRowFour(shift);
			BuildRowFive(shift);
			BuildRowSix(shift);
		}

		private void BuildBasicKey(IList<string> list, ImGUIButtonRow row, float width)
		{
			foreach (var item in list)
			{
				row.widths.Add().Value = width;
				row.labels.Add().Value = item;
			}
		}

		private void BuildRowOne(bool shift)
		{
			if (rowOne.Target == null)
				return;
			rowOne.Target.labels.Clear();
			rowOne.Target.widths.Clear();
			var row = rowOne.Target;
			if (shift)
			{
				BuildBasicKey(new string[] {
				"Esc",
				"F1",
				"F2",
				"F3",
				"F4",
				"F5",
				"F6",
				"F7",
				"F8",
				"F9",
				"F10",
				"F11",
				"F12"
				}, row, 0.06f);
			}
			else
			{
				BuildBasicKey(new string[] {
				"Esc",
				"F1",
				"F2",
				"F3",
				"F4",
				"F5",
				"F6",
				"F7",
				"F8",
				"F9",
				"F10",
				"F11",
				"F12"
				}, row, 0.06f);
			}
		}
		private void BuildRowTwo(bool shift)
		{
			if (rowTwo.Target == null)
				return;
			rowTwo.Target.labels.Clear();
			rowTwo.Target.widths.Clear();
			var row = rowTwo.Target;
			if (shift)
			{
				BuildBasicKey(new string[] {
				"~",
				"!",
				"@",
				"#",
				"$",
				"%",
				"^",
				"&",
				"*",
				"(",
				")",
				"_",
				"+"
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "<--";
			}
			else
			{
				BuildBasicKey(new string[] {
				"`",
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"8",
				"9",
				"0",
				"-",
				"+"
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "<--";
			}
		}
		private void BuildRowThree(bool shift)
		{
			if (rowThree.Target == null)
				return;
			rowThree.Target.labels.Clear();
			rowThree.Target.widths.Clear();
			var row = rowThree.Target;
			if (shift)
			{
				row.widths.Add().Value = 0.075f;
				row.labels.Add().Value = "Tab";
				BuildBasicKey(new string[] {
				"Q",
				"W",
				"E",
				"R",
				"T",
				"Y",
				"U",
				"I",
				"O",
				"P",
				"{",
				"}"
				}, row, 0.05f);
				row.widths.Add().Value = 0.075f;
				row.labels.Add().Value = "|";
			}
			else
			{
				row.widths.Add().Value = 0.075f;
				row.labels.Add().Value = "Tab";
				BuildBasicKey(new string[] {
				"q",
				"w",
				"e",
				"r",
				"t",
				"y",
				"u",
				"i",
				"o",
				"p",
				"[",
				"]"
				}, row, 0.05f);
				row.widths.Add().Value = 0.075f;
				row.labels.Add().Value = "\\";
			}
		}
		private void BuildRowFour(bool shift)
		{
			if (rowFour.Target == null)
				return;
			rowFour.Target.labels.Clear();
			rowFour.Target.widths.Clear();
			var row = rowFour.Target;
			if (shift)
			{
				row.widths.Add().Value = 0.085f;
				row.labels.Add().Value = "Caps\nLock";
				BuildBasicKey(new string[] {
				"A",
				"S",
				"D",
				"F",
				"G",
				"H",
				"J",
				"K",
				"L",
				":",
				"\"",
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "Enter";
			}
			else
			{
				row.widths.Add().Value = 0.085f;
				row.labels.Add().Value = "Caps\nLock";
				BuildBasicKey(new string[] {
				"a",
				"s",
				"d",
				"f",
				"g",
				"h",
				"j",
				"k",
				"l",
				";",
				"'",
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "Enter";
			}
		}
		private void BuildRowFive(bool shift)
		{
			if (rowFive.Target == null)
				return;
			rowFive.Target.labels.Clear();
			rowFive.Target.widths.Clear();
			var row = rowFive.Target;
			if (shift)
			{
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "sHIFT";
				BuildBasicKey(new string[] {
				"Z",
				"X",
				"C",
				"V",
				"B",
				"N",
				"M",
				"<",
				">",
				"?",
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "/\\";
			}
			else
			{
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "Shift";
				BuildBasicKey(new string[] {
				"z",
				"x",
				"c",
				"v",
				"b",
				"n",
				"m",
				",",
				".",
				"/",
				}, row, 0.05f);
				row.widths.Add().Value = 0.12f;
				row.labels.Add().Value = "/\\";
			}
		}
		private void BuildRowSix(bool shift)
		{
			if (rowSix.Target == null)
				return;
			rowSix.Target.labels.Clear();
			rowSix.Target.widths.Clear();
			var row = rowSix.Target;
			if (shift)
			{
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Ctrl";
				row.widths.Add().Value = 0.06f;
				row.labels.Add().Value = "Close";
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Alt";
				row.widths.Add().Value = 0.32f;
				row.labels.Add().Value = "Space";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "Ctx";
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Ctrl";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "<=";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "\\/";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "=>";
			}
			else
			{
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Ctrl";
				row.widths.Add().Value = 0.06f;
				row.labels.Add().Value = "Close";
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Alt";
				row.widths.Add().Value = 0.32f;
				row.labels.Add().Value = "Space";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "Ctx";
				row.widths.Add().Value = 0.08f;
				row.labels.Add().Value = "Ctrl";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "<=";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "\\/";
				row.widths.Add().Value = 0.05f;
				row.labels.Add().Value = "=>";
			}
		}
		public ImGUIKeyboard(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ImGUIKeyboard()
		{
		}
	}
}
