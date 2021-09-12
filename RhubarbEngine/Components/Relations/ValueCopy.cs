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

namespace RhubarbEngine.Components.Relations
{
	[Category(new string[] { "Relations" })]
	public class ValueCopy<T> : Component where T : IConvertible
	{
		public Driver<T> driver;

		public SyncRef<ValueSource<T>> source;

		public Sync<bool> writeBack;

		public override void buildSyncObjs(bool newRefIds)
		{
			driver = new Driver<T>(this, newRefIds);
			source = new SyncRef<ValueSource<T>>(this, newRefIds);
			writeBack = new Sync<bool>(this, newRefIds);
		}

		private IChangeable linckedSource;

		private IChangeable linckedTarget;
		public void sourceChange(IChangeable val)
		{
			if (source.Target != null && driver.Linked)
			{
				driver.Drivevalue = source.Target.Value;
			}
		}

		public void targetChange(IChangeable val)
		{
			if (writeBack.Value && source.Target != null && driver.Linked)
			{
				source.Target.Value = driver.Drivevalue;
			}
		}
		public override void onChanged()
		{
			if (source.Target != null && driver.Linked)
			{
				if (linckedSource != null)
				{
					linckedTarget.Changed -= sourceChange;
				}
				if (linckedTarget != null)
				{
					linckedTarget.Changed -= targetChange;
				}
				linckedSource = source.Target;
				linckedTarget = driver.Target;
				linckedTarget.Changed += targetChange;
				linckedTarget.Changed += sourceChange;

			}
		}
		public override void CommonUpdate(DateTime startTime, DateTime Frame)
		{
		}
		public ValueCopy(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
		{

		}
		public ValueCopy()
		{
		}
	}
}
