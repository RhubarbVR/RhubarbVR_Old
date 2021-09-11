using g3;

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
		private readonly TubeGenerator _generator = new TubeGenerator();

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

		public override void buildSyncObjs(bool newRefIds)
		{
			Endpoint = new Sync<Vector3d>(this, newRefIds);
			Endpoint.value = new Vector3d(0, 1, 0);
			EndHandle = new Sync<Vector3d>(this, newRefIds);
			EndHandle.value = new Vector3d(0, 0, 0);
			StartHandle = new Sync<Vector3d>(this, newRefIds);
			StartHandle.value = new Vector3d(0, 1, 0);
			CurveSteps = new Sync<int>(this, newRefIds);
			CurveSteps.value = 25;
			Radius = new Sync<double>(this, newRefIds);
			Radius.value = 0.01d;
			Steps = new Sync<int>(this, newRefIds);
			Steps.value = 3;
			AngleShiftRad = new Sync<int>(this, newRefIds);
			AngleShiftRad.value = 0;
			Capped = new Sync<bool>(this, newRefIds);
			CapCenter = new Sync<Vector2d>(this, newRefIds);
			Clockwise = new Sync<bool>(this, newRefIds);
			OverrideCapCenter = new Sync<bool>(this, newRefIds);
			WantUVs = new Sync<bool>(this, newRefIds);
			ClosedLoop = new Sync<bool>(this, newRefIds);
		}

		public override void onChanged()
		{
			updateMesh();
		}


		private void loadCurve()
		{
			if (_generator.Vertices == null)
			{
				_generator.Vertices = new List<Vector3d>();
			}
			_generator.Vertices.Clear();
			for (int i = 0; i < CurveSteps.value; i++)
			{
				float poser = (float)(i) / ((float)CurveSteps.value - 1);
				_generator.Vertices.Add(Vector3d.bezier(Vector3d.Zero, StartHandle.value, EndHandle.value, Endpoint.value, poser));
			}
		}
		RMesh kite;

		private void updateMesh()
		{
			_generator.Clockwise = Clockwise.value;
			_generator.OverrideCapCenter = OverrideCapCenter.value;
			_generator.CapCenter = CapCenter.value;
			_generator.ClosedLoop = ClosedLoop.value;
			_generator.WantUVs = WantUVs.value;
			_generator.Capped = Capped.value;
			_generator.Polygon = Polygon2d.MakeCircle(Radius.value, Steps.value, AngleShiftRad.value);
			loadCurve();
			MeshGenerator newmesh = _generator.Generate();
			kite = new RMesh(newmesh.MakeSimpleMesh());
			kite.createMeshesBuffers(world.worldManager.engine.renderManager.gd);
			load(kite, true);
		}
		public override void onLoaded()
		{
			updateMesh();
		}
		public CurvedTubeMesh(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CurvedTubeMesh()
		{
		}

	}


}
