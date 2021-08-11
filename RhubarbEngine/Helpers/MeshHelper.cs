using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
using RhubarbEngine.World;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.Render;
using g3;
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

using Org.OpenAPITools.Model;
using BulletSharp;
using System.Numerics;
using System.Net;

namespace RhubarbEngine.Helpers
{
    public static class MeshHelper
    {
        public static (Entity,T) AddMesh<T>(Entity ea) where T : ProceduralMesh
        {
            Entity e = ea.addChild();
            BasicUnlitShader shader = e.attachComponent<BasicUnlitShader>();
            T bmesh = e.attachComponent<T>();
            RMaterial mit = e.attachComponent<RMaterial>();
            MeshRender meshRender = e.attachComponent<MeshRender>();
            Textue2DFromUrl textue2DFromUrl = e.attachComponent<Textue2DFromUrl>();

            mit.Shader.target = shader;
            meshRender.Materials.Add().target = mit;
            meshRender.Mesh.target = bmesh;
            Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.target = textue2DFromUrl;
            // rgbainbowDriver.driver.setDriveTarget(field.field);
            // rgbainbowDriver.speed.value = 50f;
            return (e,bmesh);
        }
    }
}
