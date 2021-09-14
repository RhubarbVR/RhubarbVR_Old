using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;



namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{
	public class CurvedPlaneMesh : ProceduralMesh
	{
        private readonly CurvedPlaneMeshGenerator _generator = new();
		public Sync<float> TopRadius;
		public Sync<float> BottomRadius;
		public Sync<int> Slices;
		public Sync<float> Height;
		public Sync<float> Width;

		public override void BuildSyncObjs(bool newRefIds)
		{
            TopRadius = new Sync<float>(this, newRefIds)
            {
                Value = 180f
            };
            BottomRadius = new Sync<float>(this, newRefIds)
            {
                Value = 180f
            };
            Width = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };
            Height = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };
            Slices = new Sync<int>(this, newRefIds)
            {
                Value = 10
            };
        }

		public override void OnChanged()
		{
			UpdateMesh();
		}

        private void UpdateMesh()
		{
			_generator.bRadius = BottomRadius.Value;
			_generator.tRadius = TopRadius.Value;
			_generator.Height = Height.Value;
			_generator.Slices = Slices.Value;
			_generator.Width = Width.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.createMeshesBuffers(World.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public CurvedPlaneMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CurvedPlaneMesh()
		{
		}

	}
}
