using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
	public class PlaneMesh : ProceduralMesh
	{
		private readonly TrivialRectGenerator _generator = new();

		public Sync<float> Width;
		public Sync<float> Height;
		public Sync<Vector3f> Normal;
		public Sync<Index2i> IndicesMap;
		public Sync<TrivialRectGenerator.UVModes> UVMode;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Width = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
            Height = new Sync<float>(this, newRefIds)
            {
                Value = 1f
            };
            Normal = new Sync<Vector3f>(this, newRefIds)
            {
                Value = Vector3f.AxisZ
            };
            IndicesMap = new Sync<Index2i>(this, newRefIds)
            {
                Value = new Index2i(1, 3)
            };
            UVMode = new Sync<TrivialRectGenerator.UVModes>(this, newRefIds)
            {
                Value = TrivialRectGenerator.UVModes.FullUVSquare
            };
        }
		public override void OnChanged()
		{
			UpdateMesh();
		}

		private void UpdateMesh()
		{
			_generator.Width = Width.Value;
			_generator.Height = Height.Value;
			_generator.Normal = Normal.Value;
			_generator.IndicesMap = IndicesMap.Value;
			_generator.UVMode = UVMode.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.RenderManager.Gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public PlaneMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public PlaneMesh()
		{
		}


	}
}
