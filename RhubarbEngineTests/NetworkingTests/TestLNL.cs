using Microsoft.VisualStudio.TestTools.UnitTesting;

using RhubarbEngine.World;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.World.ECS;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Tests
{ 
    public class GameInstances : FakeGame
    {
        public GameInstances(int instances):base(false)
        {
            StartRhubarbEngine($"-{instances}");
        }
    }

    public class MultiGameInstaces : IEnumerable<GameInstances>
    {

        public IEnumerator<GameInstances> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }


    public class LNLTester
    {

    }
}