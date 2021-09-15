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

		public override void BuildSyncObjs(bool newRefIds)
		{
			driver = new Driver<T>(this, newRefIds);
			source = new SyncRef<ValueSource<T>>(this, newRefIds);
			writeBack = new Sync<bool>(this, newRefIds);
		}

		private IChangeable _linckedSource;

		private IChangeable _linckedTarget;
		public void SourceChange(IChangeable val)
		{
			if (source.Target != null && driver.Linked)
			{
				driver.Drivevalue = source.Target.Value;
			}
		}

		public void TargetChange(IChangeable val)
		{
			if (writeBack.Value && source.Target != null && driver.Linked)
			{
				source.Target.Value = driver.Drivevalue;
			}
		}
		public override void OnChanged()
		{
			if (source.Target != null && driver.Linked)
			{
				if (_linckedSource != null)
				{
					_linckedTarget.Changed -= SourceChange;
				}
				if (_linckedTarget != null)
				{
					_linckedTarget.Changed -= TargetChange;
				}
				_linckedSource = source.Target;
				_linckedTarget = driver.Target;
				_linckedTarget.Changed += TargetChange;
				_linckedTarget.Changed += SourceChange;

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
