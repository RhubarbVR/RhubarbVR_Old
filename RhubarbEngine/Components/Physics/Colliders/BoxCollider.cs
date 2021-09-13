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
	public class BoxCollider : Collider
	{
		public Sync<Vector3f> boxExtents;

		public override void BuildSyncObjs(bool newRefIds)
		{
			base.BuildSyncObjs(newRefIds);
			boxExtents = new Sync<Vector3f>(this, newRefIds);
			boxExtents.Value = new Vector3f(1f);
			boxExtents.Changed += UpdateChange;
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
			StartShape(new BoxShape(new BulletSharp.Math.Vector3(boxExtents.Value.x / 2, boxExtents.Value.y / 2, boxExtents.Value.z / 2)));
		}

		public BoxCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public BoxCollider()
		{
		}
	}
}
