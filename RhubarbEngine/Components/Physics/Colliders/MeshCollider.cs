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

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			mesh = new AssetRef<RMesh>(this, newRefIds);
			mesh.LoadChange += Mesh_loadChange;
		}

		private void Mesh_loadChange(RMesh obj)
		{
			BuildShape();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			BuildShape();
		}

		private void GoNull()
		{
			BuildCollissionObject(null);
		}

		public int[] index = Array.Empty<int>();
		public BulletSharp.Math.Vector3[] vertices = Array.Empty<BulletSharp.Math.Vector3>();

		public override void BuildShape()
		{
			if (mesh.Asset == null)
			{ GoNull(); return; };
			if (!mesh.Target?.loaded ?? false)
			{ GoNull(); return; };
			// Initialize TriangleIndexVertexArray with Vector3 array
			vertices = new BulletSharp.Math.Vector3[mesh.Asset.Meshes[0].VertexCount];
			for (var i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new BulletSharp.Math.Vector3(
					(float)mesh.Asset.Meshes[0].GetVertex(i).x,
                    (float)mesh.Asset.Meshes[0].GetVertex(i).y,
                    (float)mesh.Asset.Meshes[0].GetVertex(i).z);
			}
			var e = mesh.Asset.Meshes[0].RenderIndices().ToArray();

			// Initialize TriangleIndexIndexArray with int array
			var index = new int[e.Length];
			for (var i = 0; i < index.Length; i++)
			{
				index[i] = e[i];
			}
			if (index.Length < 3)
            {
                return;
            }

            var indexVertexArray2 = new TriangleIndexVertexArray(index, vertices);
			var trys = new BvhTriangleMeshShape(indexVertexArray2, true);
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
