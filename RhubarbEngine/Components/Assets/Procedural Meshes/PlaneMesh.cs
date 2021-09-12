using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using g3;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
	public class PlaneMesh : ProceduralMesh
	{
		private readonly TrivialRectGenerator _generator = new TrivialRectGenerator();

		public Sync<float> Width;
		public Sync<float> Height;
		public Sync<Vector3f> Normal;
		public Sync<Index2i> IndicesMap;
		public Sync<TrivialRectGenerator.UVModes> UVMode;

		public override void buildSyncObjs(bool newRefIds)
		{
			Width = new Sync<float>(this, newRefIds);
			Width.Value = 1f;
			Height = new Sync<float>(this, newRefIds);
			Height.Value = 1f;
			Normal = new Sync<Vector3f>(this, newRefIds);
			Normal.Value = Vector3f.AxisZ;
			IndicesMap = new Sync<Index2i>(this, newRefIds);
			IndicesMap.Value = new Index2i(1, 3);
			UVMode = new Sync<TrivialRectGenerator.UVModes>(this, newRefIds);
			UVMode.Value = TrivialRectGenerator.UVModes.FullUVSquare;
		}
		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.Width = Width.Value;
			_generator.Height = Height.Value;
			_generator.Normal = Normal.Value;
			_generator.IndicesMap = IndicesMap.Value;
			_generator.UVMode = UVMode.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public PlaneMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public PlaneMesh()
		{
		}


	}
}
