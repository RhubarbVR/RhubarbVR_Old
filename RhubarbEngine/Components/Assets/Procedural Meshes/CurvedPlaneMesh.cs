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
	public class CurvedPlaneMesh : ProceduralMesh
	{
		private readonly CurvedPlaneMeshGenerator _generator = new CurvedPlaneMeshGenerator();
		public Sync<float> TopRadius;
		public Sync<float> BottomRadius;
		public Sync<int> Slices;
		public Sync<float> Height;
		public Sync<float> Width;

		public override void buildSyncObjs(bool newRefIds)
		{
			TopRadius = new Sync<float>(this, newRefIds);
			TopRadius.Value = 180f;
			BottomRadius = new Sync<float>(this, newRefIds);
			BottomRadius.Value = 180f;
			Width = new Sync<float>(this, newRefIds);
			Width.Value = 1.0f;
			Height = new Sync<float>(this, newRefIds);
			Height.Value = 1.0f;
			Slices = new Sync<int>(this, newRefIds);
			Slices.Value = 10;
		}

		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.bRadius = BottomRadius.Value;
			_generator.tRadius = TopRadius.Value;
			_generator.Height = Height.Value;
			_generator.Slices = Slices.Value;
			_generator.Width = Width.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public CurvedPlaneMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CurvedPlaneMesh()
		{
		}

	}
}
