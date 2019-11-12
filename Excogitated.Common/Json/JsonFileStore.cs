using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public interface IVersionable
    {
        public long Version { get; set; }
    }

    public struct JsonFileStoreSettings
    {
        public string DataDir { get; set; }
        public string BackupFile { get; set; }

        public override string ToString() => Jsonizer.Serialize(this);
    }

    public struct JsonTransaction<T> where T : class, IVersionable
    {
        public T Item { get; internal set; }
        public bool Write { get; internal set; }
    }

    public class JsonFileStore<T> where T : class, IVersionable
    {
        private readonly AtomicDictionary<string, AsyncLock> _fileLocks = new AtomicDictionary<string, AsyncLock>();
        private readonly CowList<Func<JsonTransaction<T>, ValueTask>> _upserters = new CowList<Func<JsonTransaction<T>, ValueTask>>();
        private readonly CowList<Func<T, ValueTask>> _deleters = new CowList<Func<T, ValueTask>>();
        private readonly JsonFileStoreSettings _settings;
        private readonly DirectoryInfo _dataDir;

        public JsonFileStore(JsonFileStoreSettings settings)
        {
            _settings = settings;
            if (settings.DataDir.IsNotNullOrWhiteSpace())
            {
                _dataDir = new DirectoryInfo(settings.DataDir);
                _dataDir.CreateStrong();
                if (_settings.BackupFile.IsNotNullOrWhiteSpace())
                    StartBackups();
            }
        }

        private async void StartBackups()
        {
            while (true)
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    if (_initialized)
                    {
                        var tempFile = new FileInfo(_settings.BackupFile + ".temp");
                        await tempFile.DeleteAsync();
                        var files = _dataDir.EnumerateFiles()
                            .Where(f => f.Name.StartsWith("db."))
                            .Where(f => f.Name.EndsWith(".json.zip"))
                            .ToList();
                        using (var zip = ZipFile.Open(tempFile.FullName, ZipArchiveMode.Update))
                            foreach (var file in files.Watch(files.Count))
                                using (await _fileLocks.GetOrAdd(file.FullName).EnterAsync())
                                {
                                    var entry = zip.CreateEntry(file.Name, CompressionLevel.Optimal);
                                    using var stream = entry.Open();
                                    using var reader = file.OpenRead();
                                    await reader.CopyToAsync(stream);
                                    await stream.FlushAsync();
                                }
                        File.Move(tempFile.FullName, _settings.BackupFile);
                    }
                }
                catch (Exception e)
                {
                    Loggers.Error(e);
                }
        }

        public async IAsyncEnumerable<T> GetItems()
        {
            await Initialize();
            if (_items == null)
                throw new Exception("Primary Key has not been created.");
            foreach (var item in _items)
                yield return item;
        }

        private readonly AtomicBool _initialized = new AtomicBool();
        private readonly AsyncLock _initializedLock = new AsyncLock();
        public async ValueTask Initialize()
        {
            if (_initialized) return;
            using (await _initializedLock.EnterAsync())
                if (!_initialized)
                {
                    var count = await InitializeFromData();
                    if (count == 0)
                    {
                        count = await InitializeFromBackup();
                        if (count == 0 && _settings.DataDir.IsNullOrWhiteSpace())
                            throw new Exception($"No data available: {_settings}");
                    }
                    _initialized.Value = true;
                }
        }

        private async ValueTask<int> InitializeFromBackup()
        {
            if (_settings.BackupFile.IsNullOrWhiteSpace())
                return 0;
            var file = new FileInfo(_settings.BackupFile);
            if (file.Exists && file.Length > 0)
            {
                if (_settings.DataDir.IsNotNullOrWhiteSpace())
                {
                    using var zip = ZipFile.OpenRead(file.FullName);
                    await zip.ExtractToDirectoryAsync(_dataDir);
                    return await InitializeFromData();
                }

                var count = new AtomicInt32();
                var loaded = new AtomicHashSet<string>();
                var threads = Environment.ProcessorCount / 2;
                await Task.WhenAll(Enumerable.Range(0, threads).Select(i => Task.Run(async () =>
                {
                    using var zip = ZipFile.OpenRead(file.FullName);
                    foreach (var entry in zip.Entries.OrderBy(e => e.Length).Where(e => loaded.TryAdd(e.FullName)).Watch(zip.Entries.Count / threads))
                    {
                        using var stream = entry.Open();
                        using var data = new ZipArchive(stream, ZipArchiveMode.Read);
                        await InitializeFromZip(data);
                        count.Increment();
                    }
                })));
                return count;
            }
            return 0;
        }

        private async ValueTask<int> InitializeFromData()
        {
            if (_dataDir is null)
                return 0;
            var files = _dataDir.EnumerateFiles()
                .Where(f => f.Name.StartsWith("db."))
                .Where(f => f.Name.EndsWith(".json.zip"))
                .ToList();
            if (files.Count > 0)
                await files.ToAsync().Watch(files.Count).Batch(async file =>
                {
                    try
                    {
                        using var zipFile = ZipFile.OpenRead(file.FullName);
                        await InitializeFromZip(zipFile);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error loading: {file}", e);
                    }
                });
            return files.Count;
        }

        private async Task InitializeFromZip(ZipArchive zip)
        {
            foreach (var entry in zip.Entries)
            {
                using var stream = entry.Open();
                var item = await Jsonizer.DeserializeAsync<T>(stream);
                foreach (var upsert in _upserters)
                    await upsert(new JsonTransaction<T>
                    {
                        Item = item,
                        Write = false,
                    });
            }
        }

        private FileInfo GetFile(string key)
        {
            var fileName = $"db.{key}.json.zip".EscapeFileName();
            var path = Path.Combine(_dataDir.FullName, fileName);
            return new FileInfo(path);
        }

        private readonly AtomicBool _primaryKeyCreated = new AtomicBool();
        private IEnumerable<T> _items;

        public Func<Key, ValueTask<T>> CreatePrimaryKey<Key>(Expression<Func<T, Key>> keySelectorExpression)
        {
            keySelectorExpression.NotNull(nameof(keySelectorExpression));
            if (!_primaryKeyCreated.TrySet(true))
                throw new Exception("Primary Key already exists");

            var keySelector = keySelectorExpression.Compile();
            var index = new AtomicDictionary<Key, T>();
            _items = index.Values;
            _upserters.Add(async transaction =>
            {
                var key = keySelector(transaction.Item);
                if (key is null)
                    throw new Exception($"Upsert failed, key is null. Key: {keySelectorExpression.ToString()}, Document: {typeof(T).FullName}");
                if (transaction.Write && _settings.DataDir.IsNotNullOrWhiteSpace())
                {
                    var zip = GetFile(key.ToString());
                    var zipPath = zip.FullName;
                    using (await _fileLocks.GetOrAdd(zipPath).EnterAsync())
                    {
                        var tempFile = $"{zipPath}.temp";
                        using (var zipFile = ZipFile.Open(tempFile, ZipArchiveMode.Update))
                        {
                            var name = zip.Name.SkipLast(zip.Extension.Length).AsString();
                            using var stream = zipFile.CreateEntry(name, CompressionLevel.Optimal).Open();
                            await Jsonizer.SerializeAsync(transaction.Item, stream);
                        }
                        File.Move(tempFile, zipPath);
                    }
                }
                else if (index.ContainsKey(key))
                    throw new Exception($"Duplicate entry found in db.json files. Key: {key}");
                index[key] = transaction.Item;
            });
            _deleters.Add(async item =>
            {
                var key = keySelector(item);
                if (key is null)
                    throw new Exception($"Delete failed, key is null. Key: {keySelectorExpression.ToString()}, Document: {typeof(T).FullName}");
                if (index.TryRemove(key) && _settings.DataDir.IsNotNullOrWhiteSpace())
                {
                    var zip = GetFile(key.ToString());
                    using (await _fileLocks.GetOrAdd(zip.FullName).EnterAsync())
                        await zip.DeleteAsync();
                }
            });
            return async key =>
            {
                await Initialize();
                return index.TryGetValue(key, out var item) ? item : default;
            };
        }

        public Func<Key, ValueTask<T>> CreateIndexMany<Key>(Func<T, IEnumerable<Key>> keySelector)
        {
            keySelector.NotNull(nameof(keySelector));
            var index = new AtomicDictionary<Key, T>();
            _upserters.Add(async transaction =>
            {
                foreach (var key in keySelector(transaction.Item))
                    index[key] = transaction.Item;
            });
            _deleters.Add(async item =>
            {
                foreach (var key in keySelector(item))
                    index.TryRemove(key);
            });
            return async key =>
            {
                await Initialize();
                return index.TryGetValue(key, out var item) ? item : default;
            };
        }

        public async ValueTask Upsert(T document)
        {
            document.NotNull(nameof(document));
            await Initialize();
            foreach (var upsert in _upserters)
                await upsert(new JsonTransaction<T>
                {
                    Item = document,
                    Write = true,
                });
        }

        public async ValueTask<long> Delete(Func<T, bool> deleteSelector)
        {
            await Initialize();
            var count = 0L;
            await foreach (var item in GetItems().Where(deleteSelector))
            {
                foreach (var delete in _deleters)
                    await delete(item);
                count++;
            }
            return count;
        }
    }
}
