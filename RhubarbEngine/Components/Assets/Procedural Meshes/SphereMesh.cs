using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class SphereMesh : ProceduralMesh
	{
        private readonly Sphere3Generator_NormalizedCube _sphereGen = new();

		public Sync<double> Radius;
		public Sync<int> EdgeVertices;


		public Sync<bool> NoSharedVertices;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Radius = new Sync<double>(this, newRefIds)
            {
                Value = 0.5f
            };
            EdgeVertices = new Sync<int>(this, newRefIds)
            {
                Value = 8
            };

            NoSharedVertices = new Sync<bool>(this, newRefIds);
		}
		public override void OnChanged()
		{
			UpdateMesh();
		}

        private void UpdateMesh()
		{
			_sphereGen.Radius = Radius.Value;
			_sphereGen.EdgeVertices = EdgeVertices.Value;
			_sphereGen.NoSharedVertices = NoSharedVertices.Value;
			var newmesh = _sphereGen.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.renderManager.gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public SphereMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SphereMesh()
		{
		}
	}
}
