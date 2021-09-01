using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.World.DataStructure;
using RhubarbDataTypes;

namespace RhubarbEngine.World
{
    public interface DriveMember<T> : ValueSource<T>,Driveable where T: IConvertible
    {
        void drive(IDriver source);
        void forceDrive(IDriver source);

    }
}
