using System;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Task_Catch
    {
        public static async void Catch(this ILogger logger, Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                logger?.Error(e);
            }
        }

        public static async void Catch(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }

        public static async void Catch(this ValueTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }

        public static async void Catch<T>(this ValueTask<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }
    }
}
