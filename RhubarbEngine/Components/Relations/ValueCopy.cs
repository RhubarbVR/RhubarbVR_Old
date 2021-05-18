using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RhubarbEngine.World.DataStructure;
using BaseR;
using RhubarbEngine.World.ECS;
using RhubarbEngine.World;

namespace RhubarbEngine.Components.Relations
{
    [Category(new string[] { "Relations" })]
    public class ValueCopy<T> : Component
    {
        public Driver<T> driver;

        public SyncRef<ValueSource<T>> source;

        public Sync<bool> writeBack;
        public override void buildSyncObjs(bool newRefIds)
        {
            driver = new Driver<T>(this, newRefIds);
            source = new SyncRef<ValueSource<T>>(this, newRefIds);
        }

        private IChangeable linckedSource;

        private IChangeable linckedTarget;
        public void sourceChange(IChangeable val)
        {
            if (source.target != null&& driver.Linked)
            {
                driver.Drivevalue = source.target.value;
            }
        }

        public void targetChange(IChangeable val)
        {
            if (writeBack.value&& source.target != null&& driver.Linked)
            {
                source.target.value = driver.Drivevalue;
            }
        }
        public override void onChanged()
        {
            if(source.target != null&& driver.Linked)
            {
                if (linckedSource != null)
                {
                    linckedTarget.Changed -= sourceChange;
                }
                if (linckedTarget != null)
                {
                    linckedTarget.Changed -= targetChange;
                }
                linckedSource = source.target;
                linckedTarget = driver.target;
                linckedTarget.Changed += targetChange;
                linckedTarget.Changed += sourceChange;

            }
        }
        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
        }
        public ValueCopy(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public ValueCopy()
        {
        }
    }
}
