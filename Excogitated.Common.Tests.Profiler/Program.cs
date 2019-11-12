using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Test.Profiler
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                using var logger = FileLogger.AppendDefault(typeof(Program));
                await logger.ClearLog();
                var types = typeof(RngTests).Assembly.DefinedTypes.Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(TestClassAttribute)));
                foreach (var type in types.Where(t => t.CustomAttributes.All(a => a.AttributeType != typeof(IgnoreAttribute))))
                {
                    var instance = Activator.CreateInstance(type);
                    var methods = type.GetMethods().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(TestMethodAttribute)));
                    foreach (var method in methods.Where(m => m.CustomAttributes.All(a => a.AttributeType != typeof(IgnoreAttribute))))
                        try
                        {
                            var time = Stopwatch.StartNew();
                            var result = method.Invoke(instance, null);
                            if (result is Task task)
                                await task;
                            logger.Debug(new { Success = true, Time = time.Elapsed, Type = type.Name, Method = method.Name });
                        }
                        catch (Exception e)
                        {
                            logger.Error(new { Success = false, Type = type.Name, Method = method.Name, Exception = e.Message });
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Task.Delay(15000);
            }
        }
    }
}
