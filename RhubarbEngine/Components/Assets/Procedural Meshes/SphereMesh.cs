using RNumerics;

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

		public override void BuildSyncObjs(bool newRefIds)
		{
			Radius = new Sync<double>(this, newRefIds);
			Radius.Value = 0.5f;
			EdgeVertices = new Sync<int>(this, newRefIds);
			EdgeVertices.Value = 8;

			NoSharedVertices = new Sync<bool>(this, newRefIds);
		}
		public override void OnChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			sphereGen.Radius = Radius.Value;
			sphereGen.EdgeVertices = EdgeVertices.Value;
			sphereGen.NoSharedVertices = NoSharedVertices.Value;
			MeshGenerator newmesh = sphereGen.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(World.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void OnLoaded()
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
