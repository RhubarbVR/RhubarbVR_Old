using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Veldrid;
using g3;
using ImGuiNET;
using System.Collections.Generic;

namespace RhubarbEngine.Components.ImGUI
{

    [Category("ImGUI/Developer")]
    public class ComponentAttacherPath : ComponentAttacherField
    {
        public Sync<string> path;

        public override void buildSyncObjs(bool newRefIds)
        {
            base.buildSyncObjs(newRefIds);
            path = new Sync<string>(this, newRefIds);
        }


        public ComponentAttacherPath(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public ComponentAttacherPath()
        {
        }

        public override void ImguiRender(ImGuiRenderer imGuiRenderer, ImGUICanvas canvas)
        {
            if (ImGui.Button(path.value + "##"+referenceID.id,new System.Numerics.Vector2(ImGui.GetWindowContentRegionWidth(), 20)))
            {
                if(path.value == "../")
                {
                    if (target.target != null)
                    {
                        string news = "/";
                        string temp = "";
                        foreach (var item in target.target.path.value.Split('/', '\\'))
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                news += temp;
                                temp = item;
                            }
                        }
                        Console.WriteLine("Thing " + news);
                        target.target.path.value = news;
                    }
                }
                else
                {
                    if (target.target != null)
                        target.target.path.value += path.value;
                }
            }
        }
    }
}