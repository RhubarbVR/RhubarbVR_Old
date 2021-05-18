using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using BaseR;

namespace RhubarbEngine.World
{
    public interface DriveMember<T> : ValueSource<T>,Driveable
    {
        void drive(IDriver source);
        void forceDrive(IDriver source);

    }
}
