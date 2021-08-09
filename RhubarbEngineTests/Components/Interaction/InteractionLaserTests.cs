using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhubarbEngine.Components.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Components.Interaction.Tests
{
    [TestClass()]
    public class InteractionLaserTests
    {
        [TestMethod()]
        public void getUVPosOnTryTest()
        {
            var val = InteractionLaser.getUVPosOnTry(new g3.Vector3d(2, 2, 0), new g3.Vector2f(1, 1), new g3.Vector3d(0, 0, 0), new g3.Vector2f(0, 0), new g3.Vector3d(2, 0, 0), new g3.Vector2f(1, 0), new g3.Vector3d(1, 1, 0));
            if (val != new g3.Vector2f(0.5, 0.5)) 
            {
                Assert.Fail(val.ToString());
            }
        }
    }
}