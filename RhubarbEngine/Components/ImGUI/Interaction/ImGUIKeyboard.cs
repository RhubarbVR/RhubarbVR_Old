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

        public override void ImguiRender(ImGuiRenderer imGuiRenderer)
        {
            ImGui.Dummy(new Vector2(0, 10));
            if (ImGui.BeginChild(id.value ?? "", new Vector2(size.value.x, size.value.y), border.value, windowflag.value))
            {
                foreach (var item in children)
                {
                    item.target?.ImguiRender(imGuiRenderer);
                }
                ImGui.EndChild();
            }
        }

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
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
            if (shift.value)
            {
                mkey |= ModifierKeys.Shift;
                shift.value = false;
            }
            if (ctrl.value)
            {
                mkey |= ModifierKeys.Control;
            }
            if (alt.value)
            {
                mkey |= ModifierKeys.Alt;
            }
            if (gui.value)
            {
                mkey |= ModifierKeys.Gui;
            }
            var i = input.mainWindows.FrameSnapshot;
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
                        shift.value = false;
                        ReloadKeyboard();
                        break;
                    case "Shift":
                        i.PressKey(Key.ShiftLeft, mkey);
                        shift.value = true;
                        ReloadKeyboard();
                        break;
                    case "Enter":
                        input.mainWindows.FrameSnapshot.PressChar('\n', mkey);
                        break;
                    case "Space":
                        input.mainWindows.FrameSnapshot.PressChar(' ', mkey);
                        break;
                    case "Caps\nLock":
                        i.PressKey(Key.F1, mkey);
                        caps.value = !caps.value;
                        ReloadKeyboard();
                        break;
                    case "Ctrl":
                        i.PressKey(Key.ControlLeft, mkey);
                        ctrl.value = !ctrl.value;
                        break;
                    case "Alt":
                        i.PressKey(Key.AltLeft, mkey);
                        alt.value = !alt.value;
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
                input.mainWindows.FrameSnapshot.PressChar(key.ToCharArray()[0], mkey);
            }

        }

        public override void OnAttach()
        {
            base.OnAttach();
            rowOne.target = entity.attachComponent<ImGUIButtonRow>();
            rowTwo.target = entity.attachComponent<ImGUIButtonRow>();
            rowThree.target = entity.attachComponent<ImGUIButtonRow>();
            rowFour.target = entity.attachComponent<ImGUIButtonRow>();
            rowFive.target = entity.attachComponent<ImGUIButtonRow>();
            rowSix.target = entity.attachComponent<ImGUIButtonRow>();
            rowOne.target.action.Target = clickKey;
            rowTwo.target.action.Target = clickKey;
            rowThree.target.action.Target = clickKey;
            rowFour.target.action.Target = clickKey;
            rowFive.target.action.Target = clickKey;
            rowSix.target.action.Target = clickKey;
            float val = 0.125f;
            rowOne.target.hight.value = val;
            rowTwo.target.hight.value = val;
            rowThree.target.hight.value = val;
            rowFour.target.hight.value = val;
            rowFive.target.hight.value = val;
            rowSix.target.hight.value = val;
            children.Add().target = rowOne.target;
            children.Add().target = rowTwo.target;
            children.Add().target = rowThree.target;
            children.Add().target = rowFour.target;
            children.Add().target = rowFive.target;
            children.Add().target = rowSix.target;
            ReloadKeyboard();
        }

        public void ReloadKeyboard()
        {
            BuildRows(shift.value || caps.value);
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
                row.widths.Add().value = width;
                row.labels.Add().value = item;
            }
        }

        private void BuildRowOne(bool shift)
        {
            if (rowOne.target == null) return;
            rowOne.target.labels.Clear();
            rowOne.target.widths.Clear();
            var row = rowOne.target;
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
            if (rowTwo.target == null) return;
            rowTwo.target.labels.Clear();
            rowTwo.target.widths.Clear();
            var row = rowTwo.target;
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "<--";
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "<--";
            }
        }
        private void BuildRowThree(bool shift)
        {
            if (rowThree.target == null) return;
            rowThree.target.labels.Clear();
            rowThree.target.widths.Clear();
            var row = rowThree.target;
            if (shift)
            {
                row.widths.Add().value = 0.075f;
                row.labels.Add().value = "Tab";
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
                row.widths.Add().value = 0.075f;
                row.labels.Add().value = "|";
            }
            else
            {
                row.widths.Add().value = 0.075f;
                row.labels.Add().value = "Tab";
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
                row.widths.Add().value = 0.075f;
                row.labels.Add().value = "\\";
            }
        }
        private void BuildRowFour(bool shift)
        {
            if (rowFour.target == null) return;
            rowFour.target.labels.Clear();
            rowFour.target.widths.Clear();
            var row = rowFour.target;
            if (shift)
            {
                row.widths.Add().value = 0.085f;
                row.labels.Add().value = "Caps\nLock";
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "Enter";
            }
            else
            {
                row.widths.Add().value = 0.085f;
                row.labels.Add().value = "Caps\nLock";
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "Enter";
            }
        }
        private void BuildRowFive(bool shift)
        {
            if (rowFive.target == null) return;
            rowFive.target.labels.Clear();
            rowFive.target.widths.Clear();
            var row = rowFive.target;
            if (shift)
            {
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "sHIFT";
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "/\\";
            }
            else
            {
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "Shift";
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
                row.widths.Add().value = 0.12f;
                row.labels.Add().value = "/\\";
            }
        }
        private void BuildRowSix(bool shift)
        {
            if (rowSix.target == null) return;
            rowSix.target.labels.Clear();
            rowSix.target.widths.Clear();
            var row = rowSix.target;
            if (shift)
            {
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Ctrl";
                row.widths.Add().value = 0.06f;
                row.labels.Add().value = "Close";
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Alt";
                row.widths.Add().value = 0.32f;
                row.labels.Add().value = "Space";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "Ctx";
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Ctrl";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "<=";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "\\/";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "=>";
            }
            else
            {
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Ctrl";
                row.widths.Add().value = 0.06f;
                row.labels.Add().value = "Close";
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Alt";
                row.widths.Add().value = 0.32f;
                row.labels.Add().value = "Space";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "Ctx";
                row.widths.Add().value = 0.08f;
                row.labels.Add().value = "Ctrl";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "<=";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "\\/";
                row.widths.Add().value = 0.05f;
                row.labels.Add().value = "=>";
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
