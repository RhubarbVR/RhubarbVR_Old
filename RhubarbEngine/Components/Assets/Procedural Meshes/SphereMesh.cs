using g3;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class SphereMesh : ProceduralMesh
	{
		private readonly Sphere3Generator_NormalizedCube sphereGen = new Sphere3Generator_NormalizedCube();

		public Sync<double> Radius;
		public Sync<int> EdgeVertices;


		public Sync<bool> NoSharedVertices;

		public override void buildSyncObjs(bool newRefIds)
		{
			Radius = new Sync<double>(this, newRefIds);
			Radius.value = 0.5f;
			EdgeVertices = new Sync<int>(this, newRefIds);
			EdgeVertices.value = 8;

			NoSharedVertices = new Sync<bool>(this, newRefIds);
		}
		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			sphereGen.Radius = Radius.value;
			sphereGen.EdgeVertices = EdgeVertices.value;
			sphereGen.NoSharedVertices = NoSharedVertices.value;
			MeshGenerator newmesh = sphereGen.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public SphereMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SphereMesh()
		{
		}
	}
}
