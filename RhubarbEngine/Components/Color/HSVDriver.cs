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

namespace RhubarbEngine.Components.Color
{
    [Category(new string[] { "Color" })]
    public class HSVDriver : Component
    {
        public Driver<ColorHSV> driver;

        public Sync<Colorf> Value;
        public override void buildSyncObjs(bool newRefIds)
        {
            driver = new Driver<ColorHSV>(this, newRefIds);
            Value = new Sync<Colorf>(this, newRefIds);
        }

        public override void CommonUpdate(DateTime startTime, DateTime Frame)
        {
            if (driver.Linked)
            {
                driver.Drivevalue = Value.value;
            }
        }
        public HSVDriver(IWorldObject _parent, bool newRefIds = true) : base( _parent, newRefIds)
        {

        }
        public HSVDriver()
        {
        }
    }
}
