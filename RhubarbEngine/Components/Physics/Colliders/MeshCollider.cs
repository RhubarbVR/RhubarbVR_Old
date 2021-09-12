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
using RNumerics;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;
using RhubarbEngine.World.Asset;

namespace RhubarbEngine.Components.Physics.Colliders
{

	[Category(new string[] { "Physics/Colliders" })]
	public class MeshCollider : Collider
	{
		public AssetRef<RMesh> mesh;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			mesh = new AssetRef<RMesh>(this, newRefIds);
			mesh.loadChange += Mesh_loadChange;
		}

		private void Mesh_loadChange(RMesh obj)
		{
			BuildShape();
		}

		public override void onLoaded()
		{
			base.onLoaded();
			BuildShape();
		}

		private void goNull()
		{
			BuildCollissionObject(null);
		}

		public int[] index = new int[] { };
		public BulletSharp.Math.Vector3[] vertices = new BulletSharp.Math.Vector3[] { };

		public override void BuildShape()
		{
			if (mesh.Asset == null)
			{ goNull(); return; };
			if (!mesh.Target?.loaded ?? false)
			{ goNull(); return; };
			// Initialize TriangleIndexVertexArray with Vector3 array
			vertices = new BulletSharp.Math.Vector3[mesh.Asset.meshes[0].VertexCount];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new BulletSharp.Math.Vector3(
					mesh.Asset.meshes[0].GetVertex(i).x,
					mesh.Asset.meshes[0].GetVertex(i).y,
					mesh.Asset.meshes[0].GetVertex(i).z);
			}
			var e = mesh.Asset.meshes[0].RenderIndices().ToArray();

			// Initialize TriangleIndexIndexArray with int array
			int[] index = new int[e.Length];
			for (int i = 0; i < index.Length; i++)
			{
				index[i] = e[i];
			}
			if (index.Length < 3)
				return;
			var indexVertexArray2 = new TriangleIndexVertexArray(index, vertices);
			BvhTriangleMeshShape trys = new BvhTriangleMeshShape(indexVertexArray2, true);
			StartShape(trys);
		}

		public MeshCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public MeshCollider()
		{
		}
	}
}
