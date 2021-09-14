using RNumerics;

using RhubarbEngine.World;
using RhubarbEngine.World.Asset;
using RhubarbEngine.World.ECS;

using System;
using System.Collections.Generic;

namespace RhubarbEngine.Components.Assets.Procedural_Meshes
{

	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CurvedTubeMesh : ProceduralMesh
	{
        private readonly TubeGenerator _generator = new();

		public Sync<double> Radius;
		public Sync<int> Steps;
		public Sync<int> CurveSteps;
		public Sync<int> AngleShiftRad;
		public Sync<bool> Capped;
		public Sync<bool> Clockwise;
		public Sync<Vector3d> Endpoint;
		public Sync<Vector3d> EndHandle;
		public Sync<Vector3d> StartHandle;

		public Sync<Vector2d> CapCenter;
		public Sync<bool> OverrideCapCenter;
		public Sync<bool> WantUVs;
		public Sync<bool> ClosedLoop;

		public override void BuildSyncObjs(bool newRefIds)
		{
            Endpoint = new Sync<Vector3d>(this, newRefIds)
            {
                Value = new Vector3d(0, 1, 0)
            };
            EndHandle = new Sync<Vector3d>(this, newRefIds)
            {
                Value = new Vector3d(0, 0, 0)
            };
            StartHandle = new Sync<Vector3d>(this, newRefIds)
            {
                Value = new Vector3d(0, 1, 0)
            };
            CurveSteps = new Sync<int>(this, newRefIds)
            {
                Value = 25
            };
            Radius = new Sync<double>(this, newRefIds)
            {
                Value = 0.01d
            };
            Steps = new Sync<int>(this, newRefIds)
            {
                Value = 3
            };
            AngleShiftRad = new Sync<int>(this, newRefIds)
            {
                Value = 0
            };
            Capped = new Sync<bool>(this, newRefIds);
			CapCenter = new Sync<Vector2d>(this, newRefIds);
			Clockwise = new Sync<bool>(this, newRefIds);
			OverrideCapCenter = new Sync<bool>(this, newRefIds);
			WantUVs = new Sync<bool>(this, newRefIds);
			ClosedLoop = new Sync<bool>(this, newRefIds);
		}

		public override void OnChanged()
		{
			UpdateMesh();
		}


		private void LoadCurve()
		{
			if (_generator.Vertices == null)
			{
				_generator.Vertices = new List<Vector3d>();
			}
			_generator.Vertices.Clear();
			for (var i = 0; i < CurveSteps.Value; i++)
			{
				var poser = (float)(i) / ((float)CurveSteps.Value - 1);
				_generator.Vertices.Add(Vector3d.bezier(Vector3d.Zero, StartHandle.Value, EndHandle.Value, Endpoint.Value, poser));
			}
		}
		RMesh _kite;

		private void UpdateMesh()
		{
			_generator.Clockwise = Clockwise.Value;
			_generator.OverrideCapCenter = OverrideCapCenter.Value;
			_generator.CapCenter = CapCenter.Value;
			_generator.ClosedLoop = ClosedLoop.Value;
			_generator.WantUVs = WantUVs.Value;
			_generator.Capped = Capped.Value;
			_generator.Polygon = Polygon2d.MakeCircle(Radius.Value, Steps.Value, AngleShiftRad.Value);
			LoadCurve();
			var newmesh = _generator.Generate();
			_kite = new RMesh(newmesh.MakeSimpleMesh());
			_kite.createMeshesBuffers(World.worldManager.engine.renderManager.gd);
			load(_kite, true);
		}
		public override void OnLoaded()
		{
			UpdateMesh();
		}
		public CurvedTubeMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CurvedTubeMesh()
		{
		}

	}


}
