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

namespace RhubarbEngine.Components.PrivateSpace
{
    public class DashManager : Component
    {


        public override void buildSyncObjs(bool newRefIds)
        {

        }


        public override void OnAttach()
        {
            base.onLoaded();
            var e = entity.addChild("Main Panel");
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(90f, -90f, -90f);
            e.position.value = new Vector3f(0, 0, -1);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
            ImGUIInputText imGUIText = e.attachComponent<ImGUIInputText>();
            imGUICanvas.imputPlane.target = bmeshcol;
            imGUICanvas.element.target = imGUIText;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            imGUICanvas.noCloseing.value = true;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
        }

        private DateTime opened = DateTime.UtcNow;
        
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (DateTime.UtcNow <= opened + new TimeSpan(0, 0, 2)) return;
            if (((input.mainWindows.GetKey(Veldrid.Key.ControlLeft) || input.mainWindows.GetKey(Veldrid.Key.ControlLeft)) && input.mainWindows.GetKey(Veldrid.Key.Space)) || input.mainWindows.GetKeyDown(Veldrid.Key.Escape))
            {
                entity.enabled.value = !entity.enabled.value;
                opened = DateTime.UtcNow; 
            }
            if (input.MenuPress(Input.Creality.None))
            {
                entity.enabled.value = !entity.enabled.value;
                opened = DateTime.UtcNow;
            }
        }

        public DashManager(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public DashManager()
        {
        }
    }
}
