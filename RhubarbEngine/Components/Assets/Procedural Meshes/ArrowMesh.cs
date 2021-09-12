using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class ArrowMesh : ProceduralMesh
	{
		private readonly Radial3DArrowGenerator _generator = new Radial3DArrowGenerator();

		public Sync<float> StickRadius;
		public Sync<float> StickLength;
		public Sync<float> HeadBaseRadius;
		public Sync<float> TipRadius;
		public Sync<float> HeadLength;
		public Sync<bool> DoubleSided;

		public override void buildSyncObjs(bool newRefIds)
		{
			StickRadius = new Sync<float>(this, newRefIds);
			StickRadius.Value = 0.5f;

			StickLength = new Sync<float>(this, newRefIds);
			StickLength.Value = 1.0f;

			HeadBaseRadius = new Sync<float>(this, newRefIds);
			HeadBaseRadius.Value = 1.0f;

			TipRadius = new Sync<float>(this, newRefIds);
			TipRadius.Value = 0.0f;

			HeadLength = new Sync<float>(this, newRefIds);
			HeadLength.Value = 0.5f;

			DoubleSided = new Sync<bool>(this, newRefIds);
			DoubleSided.Value = false;
		}
		public override void onChanged()
		{
			updateMesh();
		}

		private void updateMesh()
		{
			_generator.StickRadius = StickRadius.Value;
			_generator.StickLength = StickLength.Value;
			_generator.HeadBaseRadius = HeadBaseRadius.Value;
			_generator.TipRadius = TipRadius.Value;
			_generator.HeadLength = HeadLength.Value;
			_generator.DoubleSided = DoubleSided.Value;
			MeshGenerator newmesh = _generator.Generate();
			RMesh kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public ArrowMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ArrowMesh()
		{
		}
	}
}
