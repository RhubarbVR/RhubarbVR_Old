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

namespace RhubarbEngine.Components.ImGUI
{
    

    [Category("ImGUI/Text")]
    public class ImGUIText : UIWidget
    {
        public enum TextType
        {
            Unformatted,
            Normal,
            Disabled,
            Bullet,
            Wrapped,
            LogText,
            CalcTextSize,
        }

        public Sync<string> text;
        public Sync<TextType> textType;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            text = new Sync<string>(this, newRefIds);
            textType = new Sync<TextType>(this, newRefIds);
            textType.value = TextType.Normal;
        }

        public ImGUIText(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ImGUIText()
        {
        }

        public override void ImguiRender()
        {
            string value = text.value == null ? "" : text.value;
            switch (textType.value)
            {
                case TextType.Normal:
                    ImGui.Text(value);
                    break;
                case TextType.Disabled:
                    ImGui.TextDisabled(value);
                    break;
                case TextType.Bullet:
                    ImGui.BulletText(value);
                    break;
                case TextType.Wrapped:
                    ImGui.TextWrapped(value);
                    break;
                case TextType.LogText:
                    ImGui.LogText(value);
                    break;
                case TextType.CalcTextSize:
                    ImGui.CalcTextSize(value);
                    break;
                default:
                    ImGui.TextUnformatted(value);
                    break;
            }
        }
    }
}
