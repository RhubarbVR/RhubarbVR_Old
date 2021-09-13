using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CapsuleMesh : ProceduralMesh
	{
		private readonly CapsuleGenerator _generator = new CapsuleGenerator();

		public Sync<int> Longitudes;
		public Sync<int> Latitudes;
		public Sync<int> Rings;
		public Sync<float> Depth;
		public Sync<float> Radius;
		public Sync<CapsuleGenerator.UvProfile> Profile;

		public override void BuildSyncObjs(bool newRefIds)
		{
			Longitudes = new Sync<int>(this, newRefIds);
			Longitudes.Value = 32;
			Latitudes = new Sync<int>(this, newRefIds);
			Latitudes.Value = 16;
			Rings = new Sync<int>(this, newRefIds);
			Rings.Value = 0;
			Depth = new Sync<float>(this, newRefIds);
			Depth.Value = 1.0f;
			Radius = new Sync<float>(this, newRefIds);
			Radius.Value = 0.5f;
			Profile = new Sync<CapsuleGenerator.UvProfile>(this, newRefIds);
			Profile.Value = CapsuleGenerator.UvProfile.Aspect;
		}
		public override void OnChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.Longitudes = Longitudes.Value;
			_generator.Latitudes = Latitudes.Value;
			_generator.Rings = Rings.Value;
			_generator.Depth = Depth.Value;
			_generator.Radius = Radius.Value;
			_generator.Profile = Profile.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(World.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void OnLoaded()
		{
			updateMesh();
		}
		public CapsuleMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CapsuleMesh()
		{
		}
	}
}
