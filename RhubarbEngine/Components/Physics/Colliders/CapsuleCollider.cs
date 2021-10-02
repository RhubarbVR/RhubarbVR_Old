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
	public class CapsuleCollider : Collider
	{
		public Sync<float> radius;
		public Sync<float> height;

		public override void BuildSyncObjs(bool newRefIds)
		{
			// Change the default values I guess

			base.BuildSyncObjs(newRefIds);
            radius = new Sync<float>(this, newRefIds)
            {
                Value = 0.5f
            };
            radius.Changed += UpdateChange;

            height = new Sync<float>(this, newRefIds)
            {
                Value = 1.0f
            };
            height.Changed += UpdateChange;
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
			StartShape(new CapsuleShape(radius.Value, height.Value));
		}

		public CapsuleCollider(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public CapsuleCollider()
		{
		}
	}
}
