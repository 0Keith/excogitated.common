using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Zip
    {
        public static async ValueTask ExtractToDirectoryAsync(this ZipArchive zip, DirectoryInfo dir)
        {
            zip.NotNull(nameof(zip));
            foreach (var entry in zip.Entries)
                await ExtractToFileAsync(entry, dir);
        }

        public static async ValueTask ExtractToFileAsync(this ZipArchiveEntry entry, DirectoryInfo dir)
        {
            await dir.CreateStrongAsync();
            entry.NotNull(nameof(entry));
            using var stream = entry.Open();
            var fileName = Path.Combine(dir.FullName, entry.Name);
            using var file = File.Create(fileName);
            await stream.CopyToAsync(file);
            await file.FlushAsync();
        }
    }
}
