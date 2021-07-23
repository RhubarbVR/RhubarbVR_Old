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
    public class PersonalSpace : Component
    {
        public Driver<bool> cursor;

        public override void buildSyncObjs(bool newRefIds)
        {
            cursor = new Driver<bool>(this, newRefIds);
        }

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
            ImGUIInputText imGUIText = e.attachComponent<ImGUIInputText>();
            imGUICanvas.imputPlane.target = bmeshcol;
            imGUICanvas.element.target = imGUIText;
            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;

            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = imGUICanvas;

            Entity rootent = world.RootEntity.addChild();
            rootent.name.value = $"PersonalSpace User";
            rootent.persistence.value = false;
            UserRoot userRoot = rootent.attachComponent<UserRoot>();
            userRoot.user.target = world.localUser;
            world.localUser.userroot.target = userRoot;
            Entity head = rootent.addChild("Head");
            head.attachComponent<Head>().userroot.target = userRoot;
            head.attachComponent<InteractionLaser>();
            userRoot.Head.target = head;
            Entity left = rootent.addChild("Left hand");
            Entity right = rootent.addChild("Right hand");

            userRoot.LeftHand.target = left;
            userRoot.RightHand.target = right;
            Hand leftcomp = left.attachComponent<Hand>();
            leftcomp.userroot.target = userRoot;
            leftcomp.creality.value = Input.Creality.Left;
            Hand rightcomp = right.attachComponent<Hand>();
            rightcomp.creality.value = Input.Creality.Right;
            rightcomp.userroot.target = userRoot;

            var ileft = left.attachComponent<InteractionLaser>();
            var iright = right.attachComponent<InteractionLaser>();
            ileft.source.value = InteractionSource.LeftLaser;
            iright.source.value = InteractionSource.RightLaser;

            var obj3 = world.worldManager.AddMesh<SphereMesh>(head);
            obj3.scale.value = new Vector3f(0.0001);
            obj3.position.value = new Vector3f(0, 0, -engine.renderManager.nearPlaneDistance);
            cursor.target = obj3.enabled;
            logger.Log("Spawned User PersonalSpace");

        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            bool ison = false;
            switch (engine.outputType)
            {
                case VirtualReality.OutputType.Auto:
                    break;
                case VirtualReality.OutputType.Screen:
                    ison = true;
                    break;
                case VirtualReality.OutputType.SteamVR:
                    break;
                case VirtualReality.OutputType.OculusVR:
                    break;
                default:
                    break;
            }
            if(cursor.target != null)
            {
                cursor.target.value = ison;
            }
        }

        public PersonalSpace(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {

        }
        public PersonalSpace()
        {
        }
    }
}
