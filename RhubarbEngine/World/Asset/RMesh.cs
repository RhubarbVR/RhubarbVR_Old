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

        private List<IMesh> meshes = new List<IMesh>();

        public List<MeshPiece> meshPieces;

        public List<IDisposable> disposables = new List<IDisposable>();
        public virtual int layerCount
        {
            get
            {
                return meshes.Count;
            }
        }

        public BoundingBox boundingBox;
        public void buildBoundingBox()
        {
        
        }


        public void createMeshesBuffers(GraphicsDevice _gd)
        {
            Logger.Log("Loading Mesh");
            List<MeshPiece> _meshPieces = new List<MeshPiece>();
            foreach (IDisposable dep in disposables)
            {
                dep.Dispose();
            }
            disposables.Clear();
            foreach (IMesh mesh in meshes)
            {
                IList<Vector3d> Vertices = new List<Vector3d>();
                IList<Vector2f> UV = new List<Vector2f>();
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var e = mesh.GetVertexAll(i);
                    Vertices.Add(e.v);
                    UV.Add(e.uv);
                }
                DeviceBuffer positions = CreateDeviceBuffer(_gd, Vertices.Select(v3 => new Vector3((float)v3.x, (float)v3.y, (float)v3.z)).ToArray(), BufferUsage.VertexBuffer);
                DeviceBuffer texCoords = CreateDeviceBuffer(_gd,
                    UV.Select(v3 => new Vector2(v3.x, v3.y)).ToArray(),
                    BufferUsage.VertexBuffer);
                DeviceBuffer indices = CreateDeviceBuffer(_gd, mesh.RenderIndices().Select(v3 => (uint)v3).ToArray(), BufferUsage.IndexBuffer);

                _meshPieces.Add(new MeshPiece(positions, texCoords, indices));
            }
            Logger.Log($"Mesh has {_meshPieces.Count}");
            meshPieces = _meshPieces;
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
            addDisposable(buffer);
            _gd.UpdateBuffer(buffer, 0, list.ToArray());
            return buffer;
        }
        public virtual IMesh GetLayer(int mitlayer)
        {
            return meshes[mitlayer];
        }

        public RMesh(IMesh single)
        {
            meshes.Add(single);
        }
        public RMesh()
        {
        }
    }
}
