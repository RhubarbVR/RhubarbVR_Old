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
			var shader = e.World.staticAssets.basicUnlitShader;
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
	}
}
