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
using Veldrid;
using System.Runtime.CompilerServices;
using RhubarbEngine.Render;
using System.Numerics;
using Veldrid.Utilities;
namespace RhubarbEngine.World.Asset
{
	public class RMesh : IAsset
	{

		public List<IMesh> Meshes { get; private set; } = new List<IMesh>();

		public List<MeshPiece> MeshPieces { get; private set; } = new List<MeshPiece>();

		public List<IDisposable> disposables = new();
		public virtual int LayerCount
		{
			get
			{
				return Meshes.Count;
			}
		}

		public BoundingBox boundingBox;


		public void CreateMeshesBuffers(GraphicsDevice _gd)
		{
            if (_gd is null)
            {
                return;
            }
            foreach (var mesh in Meshes)
			{
				IList<Vector3> Vertices = new List<Vector3>(mesh.VertexCount);
				IList<Vector2> UV = new List<Vector2>(mesh.VertexCount);
				for (var i = 0; i < mesh.VertexCount; i++)
				{
					var e = mesh.GetVertexAll(i);
					Vertices.Add(e.v);
					UV.Add((Vector2)e.uv);
				}

				var verts = Vertices.ToArray();
				boundingBox = BoundingBox.Combine(boundingBox, BoundingBox.CreateFromVertices(verts));
				var positions = CreateDeviceBuffer(_gd, verts, BufferUsage.VertexBuffer);
				var texCoords = CreateDeviceBuffer(_gd,
					UV.ToArray(),
					BufferUsage.VertexBuffer);
				var indices = CreateDeviceBuffer(_gd, mesh.RenderIndices().ToArray(), BufferUsage.IndexBuffer);
				var pic = new MeshPiece(positions, texCoords, indices);
				AddDisposable(pic);
				MeshPieces.Add(pic);

			}
		}

		public void Dispose()
		{
			foreach (var dep in disposables)
			{
				dep.Dispose();
			}
		}

		public void AddDisposable(IDisposable val)
		{
			disposables.Add(val);
		}
		public static DeviceBuffer CreateDeviceBuffer<T>(GraphicsDevice _gd, IList<T> list, BufferUsage usage) where T : unmanaged
		{
			var buffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * list.Count), usage));
			_gd.UpdateBuffer(buffer, 0, list.ToArray());
			return buffer;
		}
		public virtual IMesh GetLayer(int mitlayer)
		{
			return Meshes[mitlayer];
		}

		public RMesh(params IMesh[] single)
		{
			foreach (var item in single)
			{
				Meshes.Add(item);
			}
		}
		public RMesh()
		{
		}
		//Had Problems Have not looked deep into what causes it not to update
		//public void updateMeshesBuffers(GraphicsDevice _gd,bool updatedbondingbox,params IMesh[] single)
		//{
		//    boundingBox = default;
		//    for (int i = 0; i < single.Length; i++)
		//    {
		//        if(meshPieces.Count > i)
		//        {
		//            IList<Vector3> Vertices = new List<Vector3>(single[i].VertexCount);
		//            IList<Vector2> UV = new List<Vector2>(single[i].VertexCount);
		//            for (int v = 0; v < single[i].VertexCount; v++)
		//            {
		//                var e = single[i].GetVertexAll(i);
		//                Vertices.Add(e.v);
		//                UV.Add((Vector2)e.uv);
		//            }
		//            var verts = Vertices.ToArray();
		//            _gd.UpdateBuffer(meshPieces[i].Positions, 0, verts);
		//            _gd.UpdateBuffer(meshPieces[i].TexCoords, 0, UV.ToArray());
		//            _gd.UpdateBuffer(meshPieces[i].Indices, 0, single[i].RenderIndices().ToArray());
		//        }
		//        else
		//        {
		//            IList<Vector3> Vertices = new List<Vector3>(single[i].VertexCount);
		//            IList<Vector2> UV = new List<Vector2>(single[i].VertexCount);
		//            for (int v = 0; v < single[i].VertexCount; v++)
		//            {
		//                var e = single[i].GetVertexAll(i);
		//                Vertices.Add(e.v);
		//                UV.Add((Vector2)e.uv);
		//            }
		//            var verts = Vertices.ToArray();
		//            if (updatedbondingbox)
		//                boundingBox = BoundingBox.Combine(boundingBox, BoundingBox.CreateFromVertices(verts));
		//            DeviceBuffer positions = CreateDeviceBuffer(_gd, verts, BufferUsage.VertexBuffer);
		//            DeviceBuffer texCoords = CreateDeviceBuffer(_gd,
		//                UV.ToArray(),
		//                BufferUsage.VertexBuffer);
		//            DeviceBuffer indices = CreateDeviceBuffer(_gd, single[i].RenderIndices().ToArray(), BufferUsage.IndexBuffer);
		//            var pic = new MeshPiece(positions, texCoords, indices);
		//            addDisposable(pic);
		//            meshPieces.Add(pic);
		//        }
		//    }
		//    if(meshPieces.Count > single.Length)
		//    {
		//        for (int i = single.Length; i < meshPieces.Count; i++)
		//        {
		//            meshPieces.RemoveAt(single.Length);
		//            meshPieces[single.Length].Dispose();
		//        }
		//    }
		//}
	}
}
