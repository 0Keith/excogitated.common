using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Extensions
{
    public static class Extensions_File
    {
        private const int _maxExecutionTime = 30000;

        public static async ValueTask DeleteAsync(this FileInfo file)
        {
            file.NotNull(nameof(file));
            file.Refresh();
            var w = Stopwatch.StartNew();
            while (file.Exists)
            {
                file.Delete();
                await Task.Delay(w.Elapsed);
                file.Refresh();
                if (w.ElapsedMilliseconds > _maxExecutionTime)
                    throw new Exception($"Exceeded max execution time. File: {file}");
            }
        }

        public static void CreateStrong(this DirectoryInfo dir)
        {
            dir.NotNull(nameof(dir));
            dir.Refresh();
            var w = Stopwatch.StartNew();
            while (!dir.Exists)
            {
                dir.Create();
                Thread.Sleep(w.Elapsed);
                dir.Refresh();
                if (w.ElapsedMilliseconds > _maxExecutionTime)
                    throw new Exception($"Exceeded max execution time. Directory: {dir}");
            }
        }

        public static async Task CreateStrongAsync(this DirectoryInfo dir)
        {
            dir.NotNull(nameof(dir));
            dir.Refresh();
            var w = Stopwatch.StartNew();
            while (!dir.Exists)
            {
                dir.Create();
                await Task.Delay(w.Elapsed);
                dir.Refresh();
                if (w.ElapsedMilliseconds > _maxExecutionTime)
                    throw new Exception($"Exceeded max execution time. Directory: {dir}");
            }
        }

        public static async Task CopyToAsync(this Stream source, string fileName)
        {
            source.NotNull(nameof(source));
            using var file = File.Create(fileName);
            await source.CopyToAsync(file);
            await file.FlushAsync();
        }

        public static int Start(this FileInfo file)
        {
            file.NotNull(nameof(file));
            using var p = Process.Start("explorer", $"\"{file.FullName}\"");
            return p.Id;
        }
    }
}
