using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhubarbEngine.World.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RNumerics;
namespace RhubarbEngine.World.DataStructure.Tests
{
	[TestClass()]
	public class DataNodeTests
	{
		[TestMethod()]
		public void SetByteArrayTest()
		{
			TestSerlizer();
		}


		[TestMethod()]
		public void GetByteArrayTest()
		{
			TestSerlizer();
		}


		public void TestSerlizer()
		{
			var testValue = new Vector3f(1, 321, 1232);
			var val = new DataNode<Vector3f>(testValue);
			var e = val.GetByteArray();

			var newval = new DataNode<Vector3f>();
			newval.SetByteArray(e);

			Assert.AreNotSame(testValue, newval.Value);
		}
	}
}