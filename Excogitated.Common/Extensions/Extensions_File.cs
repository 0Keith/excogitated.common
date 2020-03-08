using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
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
                await AsyncTimer.Delay(w.Elapsed);
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
                await AsyncTimer.Delay(w.Elapsed);
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

        public static ValueTask MoveAsync(this FileInfo source, string destination) => source.MoveAsync(new FileInfo(destination));
        public static async ValueTask MoveAsync(this FileInfo source, FileInfo destination)
        {
            source.NotNull(nameof(source));
            source.Refresh();
            if (source.Exists)
            {
                var backup = new FileInfo($"{destination.FullName}.backup");
                await backup.DeleteAsync();
                var sourceLength = source.Length;
                source.Replace(destination.FullName, backup.FullName);
                var w = Stopwatch.StartNew();
                while (!backup.Exists || destination.Length != sourceLength)
                {
                    await AsyncTimer.Delay(w.Elapsed);
                    destination.Refresh();
                    backup.Refresh();
                    if (w.ElapsedMilliseconds > _maxExecutionTime)
                        throw new Exception($"Exceeded max execution time. Source: {source}, Destination: {destination}");
                }
                await source.DeleteAsync();
                await backup.DeleteAsync();
            }
        }

        public static int Start(this FileInfo file)
        {
            file.NotNull(nameof(file));
            using var p = Process.Start("explorer", $"\"{file.FullName}\"");
            return p.Id;
        }

        public static async Task Zip<T>(this FileInfo tempFile, T item, FileInfo zipFile)
        {
            await tempFile.DeleteAsync();
            using (var zip = ZipFile.Open(tempFile.FullName, ZipArchiveMode.Update))
            {
                var name = zipFile.Name.SkipLast(zipFile.Extension.Length).AsString();
                using var stream = zip.CreateEntry(name, CompressionLevel.Optimal).Open();
                await Jsonizer.SerializeAsync(item, stream);
            }
            await tempFile.MoveAsync(zipFile);
        }
    }
}
