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
    public partial class WorkerTests
    {
        public Entity TestingEntity;

        public Entity CreateNewTestingEntity(string name = "TestingEntity")
        {
            if(!(TestingEntity?.IsRemoved??true))
            {
                TestingEntity.Dispose();
                TestingEntity = null;
            }
            TestingEntity = testWorld.RootEntity.AddChild(name);
            return TestingEntity;
        }

        public void TestComponentSaveing(Component worker)
        {
            try
            {
                var data = worker.Serialize(new WorkerSerializerObject(false));
                var loadded = new List<Action>();
                var val = TestingEntity.AttachComponent(worker.GetType());
                val.DeSerialize(data, loadded, false, new Dictionary<ulong, ulong>(), new Dictionary<ulong, List<RefIDResign>>());
                engine.WaitForNextUpdate();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed To Save {worker.GetType().GetFormattedName()}", e);
            }
        }

        public void TestComponent(Component comp)
        {
            TestComponentSaveing(comp);
        }

        public void TestComponent(Type comp)
        {
            if(comp.IsAbstract|| comp.IsInterface|| !comp.IsAssignableTo(typeof(Component))|| (comp.IsGenericType && !comp.IsConstructedGenericType))
            {
                return;
            }
            Console.WriteLine("CompTest: " +comp.GetFormattedName());
            CreateNewTestingEntity();
            var compent = TestingEntity.AttachComponent(comp);
            TestComponent(compent);
            compent.Dispose();
        }


        [TestMethod()]
        public void AllComponentsTest()
        {
            Console.WriteLine("All Components Test Start");
            NewTestWorld();
            Console.WriteLine("Done Clreaing last Test");
            var assem = Assembly.GetAssembly(typeof(Component));
            var types =
              from t in assem.GetTypes().AsParallel()
              where t.IsAssignableTo(typeof(Component))
              select t;
            foreach (var item in types)
            {
                TestComponent(item);
            }
            //Parallel.ForEach(types, TestWorker);
        }
    }
}