using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TubeMesh : ProceduralMesh
	{
		private readonly OpenCylinderGenerator _generator = new();

		public Sync<float> BaseRadius;
		public Sync<float> TopRadius;
		public Sync<float> Height;
		public Sync<float> StartAngleDeg;
		public Sync<float> EndAngleDeg;
		public Sync<int> Slices;
		public Sync<bool> NoSharedVertices;

		public override void BuildSyncObjs(bool newRefIds)
		{
            BaseRadius = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };

            TopRadius = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };

            Height = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };

            StartAngleDeg = new Sync<float>(this, newRefIds)
            {
                Value = 0.0f
            };

            EndAngleDeg = new Sync<float>(this, newRefIds)
            {
                Value = 360.0f
            };

            Slices = new Sync<int>(this, newRefIds)
            {
                Value = 16
            };

            NoSharedVertices = new Sync<bool>(this, newRefIds)
            {
                Value = false
            };
        }

		public override void OnChanged()
		{
			UpdateMesh();
		}

		private void UpdateMesh()
		{
			_generator.BaseRadius = BaseRadius.Value;
			_generator.TopRadius = TopRadius.Value;
			_generator.Height = Height.Value;
			_generator.StartAngleDeg = StartAngleDeg.Value;
			_generator.EndAngleDeg = EndAngleDeg.Value;
			_generator.Slices = Slices.Value;
			_generator.NoSharedVertices = NoSharedVertices.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.RenderManager.gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public TubeMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public TubeMesh()
		{
		}

	}


}
