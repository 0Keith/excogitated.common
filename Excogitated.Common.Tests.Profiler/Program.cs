using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests.Profiler
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                using var logger = FileLogger.AppendDefault(typeof(Program));
                await logger.ClearLog();
                logger.LogFile.Start();
                var methods = typeof(RngTests).Assembly.DefinedTypes
                    .Where(t => t.HasAttribute<TestClassAttribute>())
                    .Where(t => !t.HasAttribute<IgnoreAttribute>())
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.HasAttribute<TestMethodAttribute>())
                    .Where(m => !m.HasAttribute<IgnoreAttribute>())
                    .ToList();

                //init assembly
                foreach (var m in methods.Select(m => m.DeclaringType).Distinct().SelectMany(t => t.GetMethods()).Where(m => m.HasAttribute<AssemblyInitializeAttribute>()))
                    m.Invoke(null, null);

                //run tests
                var successful = 0;
                object instance = null;
                foreach (var method in methods)
                {
                    var type = method.DeclaringType;
                    if (!type.IsInstanceOfType(instance))
                    {
                        //cleanup class
                        if (instance.IsNotNull())
                            foreach (var m in instance.GetType().GetMethods().Where(m => m.HasAttribute<ClassCleanupAttribute>()))
                                m.Invoke(instance, null);
                        if (instance is IDisposable d)
                            d.Dispose();

                        //init class
                        instance = Activator.CreateInstance(type);
                        foreach (var m in instance.GetType().GetMethods().Where(m => m.HasAttribute<ClassInitializeAttribute>()))
                            m.Invoke(instance, null);
                    }

                    //init test
                    foreach (var m in instance.GetType().GetMethods().Where(m => m.HasAttribute<TestInitializeAttribute>()))
                        m.Invoke(instance, null);

                    //run test
                    try
                    {
                        var time = Stopwatch.StartNew();
                        var result = method.Invoke(instance, null);
                        if (result is Task task)
                            await task;
                        logger.Debug(new { Success = true, Time = time.Elapsed, Type = type.Name, Method = method.Name });
                        successful++;
                    }
                    catch (Exception e)
                    {
                        logger.Error(new { Success = false, Type = type.Name, Method = method.Name, Exception = e.Message });
                    }

                    //cleanup test
                    foreach (var m in instance.GetType().GetMethods().Where(m => m.HasAttribute<TestCleanupAttribute>()))
                        m.Invoke(instance, null);
                }

                //cleanup assembly
                foreach (var m in methods.Select(m => m.DeclaringType).Distinct().SelectMany(t => t.GetMethods()).Where(m => m.HasAttribute<AssemblyCleanupAttribute>()))
                    m.Invoke(null, null);
                logger.Debug(new { Tests = methods.Count, Successful = successful });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await AsyncTimer.Delay(15000);
            }
        }
    }
}
