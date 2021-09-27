using Microsoft.VisualStudio.TestTools.UnitTesting;

using RhubarbEngine.World;
using RhubarbEngine.World.DataStructure;
using RhubarbEngine.World.ECS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.World.Tests
{
    [TestClass()]
    public partial class WorkerTests
    {
        public void TestWorkerSaveing(Worker worker)
        {
            try
            {
                var data = worker.Serialize(new WorkerSerializerObject(false));
                var loadded = new List<Action>();
                var val = CreateWorker(worker.GetType());
                val.Initialize(testWorld, testWorld, true);
                val.DeSerialize(data, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                engine.WaitForNextUpdate();
            }
            catch(Exception e)
            {
                throw new Exception($"Failed To Save {worker.GetType().GetFormattedName()}",e);
            }
        }

        public void TestWorker(Worker worker)
        {
            engine.WaitForNextUpdate();
            TestWorkerSaveing(worker);
            engine.WaitForNextUpdate();
            TestWorkerUpdates(worker);
            engine.WaitForNextUpdate();
            TestWorkersFields(worker);
            engine.WaitForNextUpdate();
        }

        public void TestWorkerUpdates(Worker worker)
        {
            worker.OnUserJoined(testWorld.LocalUser);
        }

        public void TestWorkersFields(Worker worker)
        {
            var fields = worker.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(ISyncMember).IsAssignableFrom(field.FieldType) && !((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length > 0)))
                {
                    if (((ISyncMember)field.GetValue(worker)) != null)
                    {
                        TestFieldValueChanges((ISyncMember)field.GetValue(worker), worker);
                    }
                }
            }
        }

        public void TestWorker(Type type)
        {
            var atra = type.GetCustomAttributes(typeof(NoneTestAttribute), true);
            if (atra.Length >= 0)
            {
                foreach (NoneTestAttribute item in atra)
                {
                    if(item.testType == TestType.Worker)
                    {
                        return;
                    }
                }
            }
            if (type.IsAssignableTo(typeof(ISyncMember))||type.IsAbstract||type.IsInterface)
            {
                return;
            }
            var val = CreateWorker(type);
            TestWorker(val);
            val.Dispose();
        }


        [TestMethod()]
        public void AllWorkersTest()
        {
            Console.WriteLine("All Workers Test Start");
            NewTestWorld();
            Console.WriteLine("Done Clreaing last Test");
            var assem = Assembly.GetAssembly(typeof(Component));
            var types =
              from t in assem.GetTypes().AsParallel()
              where t.IsAssignableTo(typeof(Worker))
              select t;
            foreach (var item in types)
            {
                Console.WriteLine(item.GetFormattedName());
                TestWorker(item);
            }
            //Parallel.ForEach(types, TestWorker);
        }
    }
}