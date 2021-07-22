using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhubarbEngine.World.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
namespace RhubarbEngine.World.DataStructure.Tests
{
    [TestClass()]
    public class DataNodeTests
    {
        [TestMethod()]
        public void setByteArrayTest()
        {
            testSerlizer();
        }


        [TestMethod()]
        public void getByteArrayTest()
        {
            testSerlizer();
        }


        public void testSerlizer()
        {
            var testValue = new Vector3f(1, 321, 1232);
            DataNode<Vector3f> val = new DataNode<Vector3f>(testValue);
            var e = val.getByteArray();

            DataNode<Vector3f> newval = new DataNode<Vector3f>();
            newval.setByteArray(e);

            Assert.AreNotSame(testValue, newval.Value);
        }
    }
}