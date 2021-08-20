using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Physics
{
    [Flags]
    public enum RCollisionFilterGroups
    {
        AllFilter = -1,
        None = 0,
        DefaultFilter = 1,
        StaticFilter = 2,
        KinematicFilter = 4,
        DebrisFilter = 8,
        SensorTrigger = 16,
        CharacterFilter = 32,
        Custome1 = 64,
        Custome2 = 128,
        Custome3 = 256,
        Custome4 = 512,
        Custome5 = 1024,

    }

    public enum SyncLevel
    {
        Local = 0,
        SyncUpdate = 1,
        ManagingUser = 2,
        CreateingUser = 4,
        HostUser = 5,
    }

}
