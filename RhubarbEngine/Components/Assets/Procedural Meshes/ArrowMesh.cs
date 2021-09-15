using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class ArrowMesh : ProceduralMesh
	{
		private readonly Radial3DArrowGenerator _generator = new();

		public Sync<float> StickRadius;
		public Sync<float> StickLength;
		public Sync<float> HeadBaseRadius;
		public Sync<float> TipRadius;
		public Sync<float> HeadLength;
		public Sync<bool> DoubleSided;

		public override void BuildSyncObjs(bool newRefIds)
		{
            StickRadius = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };

            StickLength = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };

            HeadBaseRadius = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };

            TipRadius = new Sync<float>(this, newRefIds)
            {
                Value = 0.0f
            };

            HeadLength = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };

            DoubleSided = new Sync<bool>(this, newRefIds)
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
			_generator.StickRadius = StickRadius.Value;
			_generator.StickLength = StickLength.Value;
			_generator.HeadBaseRadius = HeadBaseRadius.Value;
			_generator.TipRadius = TipRadius.Value;
			_generator.HeadLength = HeadLength.Value;
			_generator.DoubleSided = DoubleSided.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.renderManager.gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public ArrowMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ArrowMesh()
		{
		}
	}
}
