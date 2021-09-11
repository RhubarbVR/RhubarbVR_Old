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
			TopRadius.value = 180f;
			BottomRadius = new Sync<float>(this, newRefIds);
			BottomRadius.value = 180f;
			Width = new Sync<float>(this, newRefIds);
			Width.value = 1.0f;
			Height = new Sync<float>(this, newRefIds);
			Height.value = 1.0f;
			Slices = new Sync<int>(this, newRefIds);
			Slices.value = 10;
		}

		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.bRadius = BottomRadius.value;
			_generator.tRadius = TopRadius.value;
			_generator.Height = Height.value;
			_generator.Slices = Slices.value;
			_generator.Width = Width.value;
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
