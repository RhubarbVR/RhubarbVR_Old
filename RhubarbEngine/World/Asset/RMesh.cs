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
using Veldrid;
using System.Runtime.CompilerServices;
using RhubarbEngine.Render;
using System.Numerics;
using Veldrid.Utilities;
namespace RhubarbEngine.World.Asset
{
    public class RMesh : IAsset
    {

        public List<IMesh> meshes { get; private set; } = new List<IMesh>();

        public List<MeshPiece> meshPieces { get; private set; } = new List<MeshPiece>();

        public List<IDisposable> disposables = new List<IDisposable>();
        public virtual int layerCount
        {
            get
            {
                return meshes.Count;
            }
        }

        public BoundingBox boundingBox;


        public void createMeshesBuffers(GraphicsDevice _gd)
        {
            foreach (IMesh mesh in meshes)
            {
                IList<Vector3> Vertices = new List<Vector3>(mesh.VertexCount);
                IList<Vector2> UV = new List<Vector2>(mesh.VertexCount);
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var e = mesh.GetVertexAll(i);
                    Vertices.Add(e.v);
                    UV.Add((Vector2)e.uv);
                }

                var verts = Vertices.ToArray();
                boundingBox = BoundingBox.Combine(boundingBox, BoundingBox.CreateFromVertices(verts));
                DeviceBuffer positions = CreateDeviceBuffer(_gd, verts, BufferUsage.VertexBuffer);
                DeviceBuffer texCoords = CreateDeviceBuffer(_gd,
                    UV.ToArray(),
                    BufferUsage.VertexBuffer);
                DeviceBuffer indices = CreateDeviceBuffer(_gd, mesh.RenderIndices().ToArray(), BufferUsage.IndexBuffer);
                var pic = new MeshPiece(positions, texCoords, indices);
                addDisposable(pic);
                meshPieces.Add(pic);
                
            }
        }

        public void Dispose() {
            foreach (IDisposable dep in disposables)
            {
                dep.Dispose();
            }
        }

        public void addDisposable(IDisposable val)
        {
            disposables.Add(val);
        }
        public DeviceBuffer CreateDeviceBuffer<T>(GraphicsDevice _gd, IList<T> list, BufferUsage usage) where T : unmanaged
        {
            DeviceBuffer buffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * list.Count), usage));
            _gd.UpdateBuffer(buffer, 0, list.ToArray());
            return buffer;
        }
        public virtual IMesh GetLayer(int mitlayer)
        {
            return meshes[mitlayer];
        }

        public RMesh(params IMesh[] single)
        {
            foreach (var item in single)
            {
                meshes.Add(item);
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
