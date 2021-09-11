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
using g3;
using System.Numerics;
using BulletSharp;
using BulletSharp.Math;

namespace RhubarbEngine.Components.Physics.Colliders
{

	[Category(new string[] { "Physics/Colliders" })]
	public class BoxCollider : Collider
	{
		public Sync<Vector3f> boxExtents;

		public override void buildSyncObjs(bool newRefIds)
		{
			base.buildSyncObjs(newRefIds);
			boxExtents = new Sync<Vector3f>(this, newRefIds);
			boxExtents.value = new Vector3f(1f);
			boxExtents.Changed += UpdateChange;
		}

		public void UpdateChange(IChangeable val)
		{
			BuildShape();
		}

		public override void onLoaded()
		{
			base.onLoaded();
			BuildShape();
		}

		public override void BuildShape()
		{
			startShape(new BoxShape(new BulletSharp.Math.Vector3(boxExtents.value.x / 2, boxExtents.value.y / 2, boxExtents.value.z / 2)));
		}

		public BoxCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public BoxCollider()
		{
		}
	}
}
