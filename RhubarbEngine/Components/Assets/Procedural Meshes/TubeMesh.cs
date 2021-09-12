using g3;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TubeMesh : ProceduralMesh
	{
		private readonly OpenCylinderGenerator _generator = new OpenCylinderGenerator();

		public Sync<float> BaseRadius;
		public Sync<float> TopRadius;
		public Sync<float> Height;
		public Sync<float> StartAngleDeg;
		public Sync<float> EndAngleDeg;
		public Sync<int> Slices;
		public Sync<bool> NoSharedVertices;

		public override void buildSyncObjs(bool newRefIds)
		{
			BaseRadius = new Sync<float>(this, newRefIds);
			BaseRadius.Value = 1.0f;

			TopRadius = new Sync<float>(this, newRefIds);
			TopRadius.Value = 1.0f;

			Height = new Sync<float>(this, newRefIds);
			Height.Value = 1.0f;

			StartAngleDeg = new Sync<float>(this, newRefIds);
			StartAngleDeg.Value = 0.0f;

			EndAngleDeg = new Sync<float>(this, newRefIds);
			EndAngleDeg.Value = 360.0f;

			Slices = new Sync<int>(this, newRefIds);
			Slices.Value = 16;

			NoSharedVertices = new Sync<bool>(this, newRefIds);
			NoSharedVertices.Value = false;
		}

		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.BaseRadius = BaseRadius.Value;
			_generator.TopRadius = TopRadius.Value;
			_generator.Height = Height.Value;
			_generator.StartAngleDeg = StartAngleDeg.Value;
			_generator.EndAngleDeg = EndAngleDeg.Value;
			_generator.Slices = Slices.Value;
			_generator.NoSharedVertices = NoSharedVertices.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public TubeMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public TubeMesh()
		{
		}

	}


}
