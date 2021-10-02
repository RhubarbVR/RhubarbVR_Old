using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.ECS
{
    [NoneTest(TestType.Worker)]
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
		public Entity Entity { get { return _entity; } }

        public override void InturnalSyncObjs(bool newRefIds)
        {
            enabled = new Sync<bool>(this, newRefIds)
            {
                Value = true
            };
			LoadToWorld();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ListObject(Entity.IsEnabled);
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

		public override void Initialize(World _world, IWorldObject _parent, bool newRefID = true, bool childlisten = true)
		{
			_entity = (Entity)(_parent.Parent);
            base.Initialize(_world, _parent, newRefID, childlisten);
        }

        public Component()
		{
		}
	}
}
