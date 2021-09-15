using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class BoxMesh : ProceduralMesh
	{
		private readonly TrivialBox3Generator _generator = new();

		public Sync<Vector3d> Center;
		public Sync<Vector3d> AxisX;
		public Sync<Vector3d> AxisY;
		public Sync<Vector3d> AxisZ;
		public Sync<Vector3d> Extent;

		public Sync<bool> NoSharedVertices;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Center = new Sync<Vector3d>(this, newRefIds)
            {
                Value = Vector3d.Zero
            };
            AxisX = new Sync<Vector3d>(this, newRefIds)
            {
                Value = Vector3d.AxisX
            };
            AxisY = new Sync<Vector3d>(this, newRefIds)
            {
                Value = Vector3d.AxisY
            };
            AxisZ = new Sync<Vector3d>(this, newRefIds)
            {
                Value = Vector3d.AxisZ
            };
            Extent = new Sync<Vector3d>(this, newRefIds)
            {
                Value = new Vector3d(0.5f)
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
			_generator.Box.Center = Center.Value;
			_generator.Box.AxisX = AxisX.Value;
			_generator.Box.AxisY = AxisY.Value;
			_generator.Box.AxisZ = AxisZ.Value;
			_generator.Box.Extent = Extent.Value;
			_generator.NoSharedVertices = NoSharedVertices.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.renderManager.gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public BoxMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public BoxMesh()
		{
		}
	}
}
