using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class RevolveMesh : ProceduralMesh
	{
        private readonly Curve3Axis3RevolveGenerator _generator = new();

		public SyncValueList<Vector3d> Curve;

		public Sync<int> Slices;
		public Sync<bool> Capped;
		public Sync<bool> NoSharedVertices;


		public override void BuildSyncObjs(bool newRefIds)
		{
			Curve = new SyncValueList<Vector3d>(this, newRefIds);
			Curve.Add().Value = new Vector3d(1.0f, 0.25f, 0.0f);
			Curve.Add().Value = new Vector3d(1.25f, 0.0f, 0.0f);
			Curve.Add().Value = new Vector3d(1.0f, -0.25f, 0.0f);
			Curve.Add().Value = new Vector3d(0.75f, 0.0f, 0.0f);

            Slices = new Sync<int>(this, newRefIds)
            {
                Value = 16
            };

            Capped = new Sync<bool>(this, newRefIds)
            {
                Value = false
            };
            NoSharedVertices = new Sync<bool>(this, newRefIds);
		}
		public override void OnChanged()
		{
			UpdateMesh();
		}
		public override void OnLoaded()
		{
			var tempArray = new Vector3d[Curve.Count];
			for (var i = 0; i < Curve.Count; i++)
			{
				tempArray[i] = Curve[i].Value;
			}
			_generator.Curve = tempArray;

			_generator.Slices = Slices.Value;
			_generator.Capped = Capped.Value;
			_generator.NoSharedVertices = NoSharedVertices.Value;
			UpdateMesh();
		}
		private void UpdateMesh()
		{
			var tempMesh = new RMesh(_generator.Generate().MakeDMesh());
			tempMesh.CreateMeshesBuffers(World.worldManager.engine.RenderManager.Gd);
			Load(tempMesh, true);
		}

		public RevolveMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{
		}
		public RevolveMesh()
		{
		}
	}
}
