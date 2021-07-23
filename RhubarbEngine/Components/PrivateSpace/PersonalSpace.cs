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

namespace RhubarbEngine.Components.PrivateSpace
{
    public class PersonalSpace : Component
    {
        public override void OnAttach()
        {
            base.onLoaded();
            var e = entity.addChild("Thing");
            StaicMainShader shader = e.attachComponent<StaicMainShader>();
            PlaneMesh bmesh = e.attachComponent<PlaneMesh>();
            InputPlane bmeshcol = e.attachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            e.position.value = new Vector3f(-1, -1, -1);
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            ImGUICanvas imGUICanvas = e.attachComponent<ImGUICanvas>();
            ImGUIButton imGUIText = e.attachComponent<ImGUIButton>();
            imGUICanvas.imputPlane.target = bmeshcol;
            imGUICanvas.element.target = imGUIText;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;

            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;
        }

        public PersonalSpace(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public PersonalSpace()
        {
        }
    }
}
