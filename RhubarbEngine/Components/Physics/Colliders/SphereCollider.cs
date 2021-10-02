using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;

namespace RhubarbEngine.Components.Physics.Colliders
{

	[Category(new string[] { "Physics/Colliders" })]
	public class SphereCollider : Collider
	{
		public Sync<float> radius;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
            radius = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };
            radius.Changed += UpdateChange;
		}

		public void UpdateChange(IChangeable val)
		{
			BuildShape();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			BuildShape();
		}

		public override void BuildShape()
		{
			StartShape(new SphereShape(radius.Value));
		}

		public SphereCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public SphereCollider()
		{
		}
	}
}
