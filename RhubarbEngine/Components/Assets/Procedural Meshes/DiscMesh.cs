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
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class DiscMesh : ProceduralMesh
	{
		private readonly TrivialDiscGenerator _generator = new();

		public Sync<float> Radius;
		public Sync<float> StartAngleDeg;
		public Sync<float> EndAngleDeg;
		public Sync<int> Slices;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Radius = new Sync<float>(this, newRefIds)
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
                Value = 32
            };
        }

		public override void OnChanged()
		{
			UpdateMesh();
		}

		private void UpdateMesh()
		{
			_generator.Radius = Radius.Value;
			_generator.StartAngleDeg = StartAngleDeg.Value;
			_generator.EndAngleDeg = EndAngleDeg.Value;
			_generator.Slices = Slices.Value;
			var newmesh = _generator.Generate();
			var kite = new RMesh(newmesh.MakeDMesh());
			kite.CreateMeshesBuffers(World.worldManager.engine.RenderManager.gd);
			Load(kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public DiscMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public DiscMesh()
		{
		}
	}
}

