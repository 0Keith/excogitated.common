using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_File
    {
        public static async ValueTask DeleteAsync(this FileInfo file)
        {
            file.NotNull(nameof(file));
            file.Refresh();
            if (file.Exists)
            {
                await Task.Run(() => File.WriteAllText(file.FullName, string.Empty));
                await Task.Run(() => file.Delete());
            }
        }

        public static void CreateStrong(this DirectoryInfo dir)
        {
            dir.NotNull(nameof(dir));
            while (!dir.Exists)
            {
                dir.Create();
                dir.Refresh();
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
                await destination.DeleteAsync();
                await Task.Run(() => source.MoveTo(destination.FullName));
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
