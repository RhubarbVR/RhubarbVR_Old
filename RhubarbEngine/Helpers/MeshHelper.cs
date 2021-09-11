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
using RhubarbEngine.Components.Interaction;

namespace RhubarbEngine.Helpers
{
	public static class MeshHelper
	{
		public static (Entity, Window, T) attachWindow<T>(Entity ea, string name = "Window") where T : Component, IUIElement
		{
			if (name == "Window")
			{
				name = typeof(T).Name;
			}
			Entity e = ea.addChild(name);
			Window window = e.attachComponent<Window>();
			T trains = e.attachComponent<T>();
			window.element.target = trains;
			return (e, window, trains);
		}

		public static (Entity, Window) attachWindow(Entity ea, UIWidget uI, string name = "Window")
		{
			Entity e = ea.addChild(name);
			Window window = e.attachComponent<Window>();
			window.element.target = uI;
			return (e, window);
		}

		public static (Entity, T) AddMesh<T>(Entity ea, string name = "Entity") where T : ProceduralMesh
		{
			Entity e = ea.addChild(name);
			BasicUnlitShader shader = e.world.staticAssets.basicUnlitShader;
			T bmesh = e.attachComponent<T>();
			RMaterial mit = e.attachComponent<RMaterial>();
			MeshRender meshRender = e.attachComponent<MeshRender>();
			Textue2DFromUrl textue2DFromUrl = e.attachComponent<Textue2DFromUrl>();

			mit.Shader.target = shader;
			meshRender.Materials.Add().target = mit;
			meshRender.Mesh.target = bmesh;
			Render.Material.Fields.Texture2DField field = mit.getField<Render.Material.Fields.Texture2DField>("Texture", Render.Shader.ShaderType.MainFrag);
			field.field.target = textue2DFromUrl;

			return (e, bmesh);
		}

		public static (Entity, T, RMaterial) AddMesh<T>(Entity ea, AssetProvider<RShader> shader, string name = "Entity", uint renderOffset = int.MaxValue) where T : ProceduralMesh
		{
			Entity e = ea.addChild(name);
			T bmesh = e.attachComponent<T>();
			RMaterial mit = e.attachComponent<RMaterial>();
			MeshRender meshRender = e.attachComponent<MeshRender>();
			meshRender.RenderOrderOffset.value = renderOffset;
			mit.Shader.target = shader;
			meshRender.Materials.Add().target = mit;
			meshRender.Mesh.target = bmesh;

			return (e, bmesh, mit);
		}
	}
}
