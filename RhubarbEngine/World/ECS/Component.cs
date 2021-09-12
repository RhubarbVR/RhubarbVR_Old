using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.ECS
{
	public abstract class Component : Worker
	{
		public Sync<bool> enabled;
		[NoShow]
		[NoSync]
		[NoSave]
		private Entity _entity;
		[NoShow]
		[NoSync]
		[NoSave]
		public Entity entity { get { return _entity; } }

		public override void inturnalSyncObjs(bool newRefIds)
		{
			enabled = new Sync<bool>(this, newRefIds);
			enabled.Value = true;
			_entity = (Entity)(parent.Parent);
			LoadToWorld();
		}

		public override void onLoaded()
		{
			base.onLoaded();
			ListObject(entity.IsEnabled);
		}

		public virtual void LoadToWorld()
		{

		}
		public virtual void OnAttach()
		{

		}

		public override void Dispose()
		{
			ListObject(false);
			base.Dispose();
		}

		public void ListObject(bool load)
		{
			if (load)
			{
				LoadListObject();
			}
			else
			{
				RemoveListObject();
			}
		}

		public virtual void LoadListObject()
		{

		}

		public virtual void RemoveListObject()
		{

		}

		public virtual void CommonUpdate(DateTime startTime, DateTime Frame)
		{

		}
		public Component(IWorldObject _parent, bool newRefIds = true) : base(_parent.World, _parent, newRefIds, true)
		{
		}

		public override void initialize(World _world, IWorldObject _parent, bool newRefID = true)
		{
			base.initialize(_world, _parent, newRefID);
			_entity = (Entity)(_parent.Parent);
		}

		public Component()
		{
		}
	}
}
