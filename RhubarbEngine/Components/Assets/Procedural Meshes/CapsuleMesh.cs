using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CapsuleMesh : ProceduralMesh
	{
		private readonly CapsuleGenerator _generator = new();

		public Sync<int> Longitudes;
		public Sync<int> Latitudes;
		public Sync<int> Rings;
		public Sync<float> Depth;
		public Sync<float> Radius;
		public Sync<CapsuleGenerator.UvProfile> Profile;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Longitudes = new Sync<int>(this, newRefIds)
            {
                Value = 32
            };
            Latitudes = new Sync<int>(this, newRefIds)
            {
                Value = 16
            };
            Rings = new Sync<int>(this, newRefIds)
            {
                Value = 0
            };
            Depth = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };
            Radius = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };
            Profile = new Sync<CapsuleGenerator.UvProfile>(this, newRefIds)
            {
                Value = CapsuleGenerator.UvProfile.Aspect
            };
        }
		public override void OnChanged()
		{
			UpdateMesh();
		}

		private void UpdateMesh()
		{
			_generator.Longitudes = Longitudes.Value;
			_generator.Latitudes = Latitudes.Value;
			_generator.Rings = Rings.Value;
			_generator.Depth = Depth.Value;
			_generator.Radius = Radius.Value;
			_generator.Profile = Profile.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.Engine.RenderManager.Gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public CapsuleMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CapsuleMesh()
		{
		}
	}
}
