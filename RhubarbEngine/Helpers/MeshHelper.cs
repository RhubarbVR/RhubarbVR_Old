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
using RNumerics;
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
using BulletSharp;
using System.Numerics;
using System.Net;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics;
using RhubarbEngine.Components.Audio;

namespace RhubarbEngine.Helpers
{
    public static class MeshHelper
    {
        public static (Entity, Window, T) AttachWindow<T>(Entity ea, string name = "Window") where T : Component, IUIElement
        {
            if (name == "Window")
            {
                name = typeof(T).Name;
            }
            var e = ea.AddChild(name);
            var window = e.AttachComponent<Window>();
            var trains = e.AttachComponent<T>();
            window.element.Target = trains;
            return (e, window, trains);
        }

        public static (Entity, Window) AttachWindow(Entity ea, UIWidget uI, string name = "Window")
        {
            var e = ea.AddChild(name);
            var window = e.AttachComponent<Window>();
            window.element.Target = uI;
            return (e, window);
        }

        public static (Entity, T) AddMesh<T>(Entity ea, string name = "Entity") where T : ProceduralMesh
        {
            var e = ea.AddChild(name);
            var shader = e.World.staticAssets.BasicUnlitShader;
            var bmesh = e.AttachComponent<T>();
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            var textue2DFromUrl = e.AttachComponent<Textue2DFromUrl>();

            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = textue2DFromUrl;

            return (e, bmesh);
        }
        public static T AddMeshToEntity<T>(Entity e) where T : ProceduralMesh
        {
            var shader = e.World.staticAssets.BasicUnlitShader;
            var bmesh = e.AttachComponent<T>();
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            var textue2DFromUrl = e.AttachComponent<Textue2DFromUrl>();

            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = textue2DFromUrl;

            return bmesh;
        }
        public static (Entity, T, RMaterial) AddMesh<T>(Entity ea, AssetProvider<RShader> shader, string name = "Entity", uint renderOffset = int.MaxValue) where T : ProceduralMesh
        {
            var e = ea.AddChild(name);
            var bmesh = e.AttachComponent<T>();
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            meshRender.RenderOrderOffset.Value = renderOffset;
            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;

            return (e, bmesh, mit);
        }

        public static (Entity, T, RMaterial,MeshRender) AddMeshWithMeshRender<T>(Entity ea, AssetProvider<RShader> shader, string name = "Entity", uint renderOffset = int.MaxValue) where T : ProceduralMesh
        {
            var e = ea.AddChild(name);
            var bmesh = e.AttachComponent<T>();
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            meshRender.RenderOrderOffset.Value = renderOffset;
            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;

            return (e, bmesh, mit, meshRender);
        }
        public static Entity AddMesh<T>(Entity ea) where T : ProceduralMesh
        {
            var e = ea.AddChild();
            var shader = e.World.staticAssets.BasicUnlitShader;
            var bmesh = e.AttachComponent<T>();
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            var textue2DFromUrl = e.AttachComponent<Textue2DFromUrl>();

            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = textue2DFromUrl;


            return e;
        }

        public static (RMaterial, PlaneMesh,Entity,MeshRender) BlankWorld(World.World world)
        {
            world.RootEntity.AttachComponent<SimpleSpawn>();
            var floor = world.RootEntity.AddChild("Floor");
            var mit = floor.AttachComponent<RMaterial>();
            mit.Shader.Target = world.staticAssets.TilledUnlitShader;
            var planemesh = floor.AttachComponent<PlaneMesh>();
            var planecol = floor.AttachComponent<BoxCollider>();
            planemesh.Width.Value = 1000f;
            planemesh.Height.Value = 1000f;
            planecol.boxExtents.Value = new Vector3f(100f, 0.1f, 100f);
            var meshRender = floor.AttachComponent<MeshRender>();
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = planemesh;
            return (mit, planemesh, floor, meshRender);
        }

        public static void BuildLocalWorld(World.World world)
        {
            var (mit, _, floor, _) = BlankWorld(world);
            var tilefield = mit.GetField<Render.Material.Fields.Vec2Field>("Tile", Render.Shader.ShaderType.MainFrag);
            tilefield.field.Value = new Vector2f(500, 500);

            var textue2DF = floor.AttachComponent<GridTextue2D>();
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = textue2DF;

            var webBrowser = world.RootEntity.AddChild("Web Browser");
            webBrowser.position.Value = new Vector3f(0, 4.3, -5);

            BuildWebBrowser(webBrowser, new Vector2u(1920, 1080) / 2, new Vector2f(16 / 2, 9 / 2));


            AttachSpiningCubes(world.RootEntity.AddChild("Cubes"), textue2DF);




        }

        public static ImGUICanvas BuildUI(Entity e)
        {

            var shader = e.World.staticAssets.BasicUnlitShader;
            var bmesh = e.AttachComponent<PlaneMesh>();
            var bmeshcol = e.AttachComponent<InputPlane>();
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.Value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            var imGUICanvas = e.AttachComponent<ImGUICanvas>();
            var imGUIText = e.AttachComponent<ImGUIInputText>();
            imGUICanvas.imputPlane.Target = bmeshcol;
            imGUICanvas.element.Target = imGUIText;
            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = imGUICanvas;
            return imGUICanvas;
        }

        public static WebBrowser BuildWebBrowser(Entity e, Vector2u pixsize, Vector2f size, bool globalAudio = false)
        {
            var shader = e.World.staticAssets.BasicUnlitShader;
            var bmesh = e.AttachComponent<PlaneMesh>();
            var bmeshcol = e.AttachComponent<InputPlane>();
            bmesh.Width.Value = size.x;
            bmesh.Height.Value = size.y;
            bmeshcol.size.Value = size / 2;
            //e.attachComponent<Spinner>().speed.value = new Vector3f(10f);
            e.rotation.Value = Quaternionf.CreateFromEuler(0f, -90f, 0f);
            var mit = e.AttachComponent<RMaterial>();
            var meshRender = e.AttachComponent<MeshRender>();
            var imGUICanvas = e.AttachComponent<WebBrowser>();
            bmeshcol.pixelSize.Value = imGUICanvas.scale.Value = pixsize;
            imGUICanvas.imputPlane.Target = bmeshcol;
            mit.Shader.Target = shader;
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = imGUICanvas;
            if (!globalAudio)
            {
                var audio = e.AttachComponent<AudioOutput>();
                audio.audioSource.Target = imGUICanvas;
                imGUICanvas.globalAudio.Value = false;
            }
            else
            {
                imGUICanvas.globalAudio.Value = true;
            }
            return imGUICanvas;
        }

        public static void AttachSpiningCubes(Entity root, AssetProvider<RTexture2D> textue2D)
        {
            var speed = 0.5f;
            var group1 = root.AddChild("group1");
            group1.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, 0, 0);
            var group2 = root.AddChild("group2");
            group2.AttachComponent<Spinner>().speed.Value = new Vector3f(0, speed, 0);
            var group3 = root.AddChild("group3");
            group3.AttachComponent<Spinner>().speed.Value = new Vector3f(0, 0, speed / 2);
            var group4 = root.AddChild("group4");
            group4.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, speed, 0);
            var group5 = root.AddChild("group5");
            group5.AttachComponent<Spinner>().speed.Value = new Vector3f(speed / 2, speed, speed);
            var group6 = root.AddChild("group6");
            group6.AttachComponent<Spinner>().speed.Value = new Vector3f(speed, 0, speed / 2);
            var group11 = root.AddChild("group1");
            group11.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, 0, 0);
            var group21 = root.AddChild("group2");
            group21.AttachComponent<Spinner>().speed.Value = new Vector3f(0, -speed, 0);
            var group31 = root.AddChild("group3");
            group31.AttachComponent<Spinner>().speed.Value = new Vector3f(0, 0, -speed);
            var group41 = root.AddChild("group4");
            group41.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, -speed / 2, 0);
            var group51 = root.AddChild("group5");
            group51.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed / 2, -speed, -speed);
            var group61 = root.AddChild("group6");
            group61.AttachComponent<Spinner>().speed.Value = new Vector3f(-speed, 0, -speed);


            var shader = root.World.staticAssets.BasicUnlitShader;
            var bmesh = root.AttachComponent<BoxMesh>();
            var mit = root.AttachComponent<RMaterial>();
            mit.Shader.Target = shader;
            var field = mit.GetField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
            field.field.Target = textue2D;
            BuildGroup(bmesh, mit, group1);
            BuildGroup(bmesh, mit, group2);
            BuildGroup(bmesh, mit, group3);
            BuildGroup(bmesh, mit, group4);
            BuildGroup(bmesh, mit, group5);
            BuildGroup(bmesh, mit, group6);
            BuildGroup(bmesh, mit, group11);
            BuildGroup(bmesh, mit, group21);
            BuildGroup(bmesh, mit, group31);
            BuildGroup(bmesh, mit, group41);
            BuildGroup(bmesh, mit, group51);
            BuildGroup(bmesh, mit, group61);

        }


        static readonly Random _random = new();
        static float NextFloat()
        {
            var buffer = new byte[4];
            _random.NextBytes(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static void BuildGroup(BoxMesh bmesh, RMaterial mit, Entity entity)
        {
            for (var i = 0; i < 6; i++)
            {
                var cubeholder = entity.AddChild("CubeHolder");
                cubeholder.rotation.Value = Quaternionf.CreateFromEuler(NextFloat(), NextFloat(), NextFloat());
                var cube = cubeholder.AddChild("Cube");
                cube.position.Value = new Vector3f(0, 15, 0);
                cube.scale.Value = new Vector3f(0.5f);
                AttachRender(bmesh, mit, cube);
            }
        }

        public static void AttachRender(BoxMesh bmesh, RMaterial mit, Entity entity)
        {
            var meshRender = entity.AttachComponent<MeshRender>();
            meshRender.Materials.Add().Target = mit;
            meshRender.Mesh.Target = bmesh;
        }
    }
}
