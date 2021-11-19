using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbDataTypes;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;
using RNumerics;
using RhubarbEngine.Components.Interaction;
using RhubarbEngine.Components.Physics.Colliders;

//TODO: Button interacting with laser or finger press, mostly functional.

namespace RhubarbEngine.Components.Interaction
{
    [Category(new string[] { "Interaction" })]
    public class Button3D : Component
    {
        public SyncRef<Entity> ClickVisual;
        public Driver<Vector3f> PositionDriver;
        public Sync<Vector3f> ClickAxis;
        public Sync<bool> IsLaserClickable;

        public SyncDelegate OnClicked;

        [NoSave] [NoShow] [NoSync]
        private Entity _lastVisual;

        

        public override void BuildSyncObjs(bool newRefIds)
        {
            base.BuildSyncObjs(newRefIds);
            ClickVisual = new SyncRef<Entity>(this, newRefIds);
            PositionDriver = new Driver<Vector3f>(this, newRefIds);
            ClickAxis = new Sync<Vector3f>(this, newRefIds);
            IsLaserClickable = new Sync<bool>(this, newRefIds);

            OnClicked = new SyncDelegate(this, newRefIds)
            {
                Target = TestFunction
            };

            ClickVisual.Changed += ClickVisual_Changed;
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            base.CommonUpdate(startTime, Frame);
            
            //TODO: lerp ClickVisial position on axis when pressed
        }
        public override void OnLoaded()
        {
            base.OnLoaded();
        }
        private void ClickVisual_Changed(IChangeable obj)
        {
            if(_lastVisual is not null)
            {
                _lastVisual.OnClick -= Target_OnClick;
            }
            if (ClickVisual.Target is not null)
            {
                ClickVisual.Target.OnClick += Target_OnClick;
                PositionDriver.SetDriveTarget(ClickVisual.Target.position);
                
            }

            _lastVisual = ClickVisual.Target;
        }
        private void Target_OnClick(bool obj)
        {
            if (IsLaserClickable.Value)
            {
                OnClicked.Target?.Invoke();    
            }
        }

        public void TestFunction()
        {
            //TODO: testing purposes only, please delete.
            Logger.Log("Button3D Clicked", true);
        }
        public Button3D(IWorldObject _parent, bool newRefIds = true) : base(_parent, newRefIds)
        {
        }
        public Button3D(){}
    }
}
