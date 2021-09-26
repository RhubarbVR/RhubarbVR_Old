using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine
{
    public enum TestType
    {
        Worker,
    }
    public class NoneTestAttribute : Attribute
    {
        public TestType testType;
        public NoneTestAttribute(TestType test)
        {
            testType = test;
        }
    }
}
