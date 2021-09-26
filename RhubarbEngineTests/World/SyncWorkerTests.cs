using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RhubarbEngine.World.ECS;

using RNumerics;

namespace RhubarbEngine.World.Tests
{
    public partial class WorkerTests : FakeGame
    {
        public void TestFieldValueChanges(ISyncMember syncMember, Worker worker)
        {

        }

        [TestMethod()]
        public void AllSyncMemberTest()
        {
            NewTestWorld();
            var assem = Assembly.GetAssembly(typeof(Component));
            var synctypes =
              from t in assem.GetTypes().AsParallel()
              where t.IsAssignableTo(typeof(ISyncMember)) && t.IsAssignableTo(typeof(Worker))
              where !t.IsAbstract
              where !t.IsInterface
              select t;
            foreach (var notsimple in synctypes)
            {
                foreach (var item in GetSyncMemberTestTypes(notsimple))
                {
                    TestSyncMember(item);
                }
            }
        }

        public IEnumerable<Type> GetSyncMemberTestTypes(Type type)
        {
            if (type.IsGenericType)
            {
                var constra = from t in type.GetGenericArguments() from l in t.GetGenericParameterConstraints() select l;
                if (constra.Contains(typeof(Enum)))
                {
                    var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(Asset.RMesh)) };
                    var IConvertibleTypes =
                         from assem in assems.AsParallel()
                         from t in assem.GetTypes().AsParallel()
                         where !t.IsGenericType
                         where t.IsEnum
                         select t;
                    foreach (var item in IConvertibleTypes)
                    {
                        yield return type.MakeGenericType(item);
                    }
                }
                else if (constra.Contains(typeof(IAsset)))
                {
                    var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(Asset.RMesh)) };
                    var IConvertibleTypes =
                         from assem in assems.AsParallel()
                         from t in assem.GetTypes().AsParallel()
                         where typeof(IAsset).IsAssignableFrom(t)
                         where !t.IsGenericType
                         where !t.IsEnum
                         select t;
                    foreach (var item in IConvertibleTypes)
                    {
                        if (item != typeof(Enum))
                        {
                            yield return type.MakeGenericType(item);
                        }
                    }
                }
                else if (constra.Contains(typeof(IWorldObject)) || constra.Contains(typeof(IWorker)))
                {
                    var IConvertibleTypes =
                         from t in Assembly.GetAssembly(typeof(IWorldObject)).GetTypes().AsParallel()
                         where typeof(Worker).IsAssignableFrom(t)
                         where !t.IsGenericType
                         where !t.IsEnum
                         select t;
                    foreach (var item in IConvertibleTypes)
                    {
                        if (item != typeof(Enum))
                        {
                            var test = false;
                            var atra = item.GetCustomAttributes(typeof(NoneTestAttribute), true);
                            if (atra.Length >= 0)
                            {
                                foreach (NoneTestAttribute itemAtra in atra)
                                {
                                    if (itemAtra.testType == TestType.Worker)
                                    {
                                        test = true;
                                    }
                                }
                            }
                            if (item.IsAbstract)
                            {
                                test = true;
                            }
                            if (test)
                            {
                                if (!type.IsAssignableTo(typeof(SyncObjList<>)))
                                {
                                    yield return type.MakeGenericType(item);
                                }
                            }
                            else
                            {
                                yield return type.MakeGenericType(item);
                            }
                        }
                    }
                }
                else if (constra.Contains(typeof(IConvertible)))
                {
                    var assems = new Assembly[2] { Assembly.GetAssembly(typeof(Vector2f)), Assembly.GetAssembly(typeof(string)) };
                    var IConvertibleTypes =
                         from assem in assems.AsParallel()
                         from t in assem.GetTypes().AsParallel()
                         where typeof(IConvertible).IsAssignableFrom(t)
                         where !t.IsGenericType
                         where !t.IsEnum
                         select t;
                    foreach (var item in IConvertibleTypes)
                    {
                        if (item != typeof(Enum))
                        {
                            yield return type.MakeGenericType(item);
                        }
                    }
                }
                else if (constra.Contains(typeof(Delegate)))
                {
                    yield return type.MakeGenericType(typeof(Action));
                    yield return type.MakeGenericType(typeof(Action<bool>));
                    yield return type.MakeGenericType(typeof(Func<bool>));
                }
                else
                {
                    throw new Exception($"Generic Type not suppoted yet Count{constra.Count()}  Type 1{constra.First().GetFormattedName()}");
                }
            }
            else
            {
                yield return type;
            }
        }

        public void TestSyncMember(ISyncMember syncMember)
        {
            var syncMemberType = syncMember.GetType();
            if (syncMemberType.IsAssignableTo(typeof(ISyncList)))
            {
                var canRemove = true;
                var value = (ISyncList)syncMember;
                try
                {
                    if (value.Count() != 0)
                    {
                        throw new Exception($"List Count is not 0 on start {syncMemberType.GetFormattedName()}");
                    }
                    if (value.TryToAddToSyncList())
                    {
                        if (value.Count() == 0)
                        {
                            throw new Exception($"Failed To add to list {syncMemberType.GetFormattedName()}");
                        }
                        try
                        {
                            value.Remove(0);
                        }
                        catch (NotSupportedException)
                        {
                            canRemove = false;
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Failed To Remove from list {syncMemberType.GetFormattedName()} Error:{e}");
                        }

                    }
                    else
                    {
                        throw new Exception($"Failed To add to list {syncMemberType.GetFormattedName()}");
                    }
                    if ((value.Count() != 0) && canRemove)
                    {
                        throw new Exception($"Failed To Remove from list {syncMemberType.GetFormattedName()}");
                    }
                    value.TryToAddToSyncList();
                    value.TryToAddToSyncList();
                    value.Clear();
                    if (value.Count() != 0)
                    {
                        throw new Exception($"Failed To Clear list {syncMemberType.GetFormattedName()}");
                    }
                }
                catch (NotSupportedException) { }

            }
            else if (syncMemberType.IsAssignableTo(typeof(ISyncRef)))
            {

            }
        }

        public void TestSyncMember(Type type)
        {
            var worker = CreateWorker(type);
            TestSyncMember((ISyncMember)worker);
            TestWorker(worker);
            worker.Dispose();
        }

        public Worker CreateWorker(Type type)
        {
            var val = (Worker)Activator.CreateInstance(type);
            val.Initialize(testWorld, testWorld, true);
            var actions = new List<Action>();
            foreach (var item in actions)
            {
                item?.Invoke();
            }
            return val;
        }
    }
}
