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
        public bool Overwrite { get; internal set; }
    }

    public class JsonFileStore<T> where T : class, IVersionable
    {
        private readonly AtomicDictionary<string, AsyncLock> _fileLocks = new AtomicDictionary<string, AsyncLock>();
        private readonly CowList<Action<JsonTransaction<T>>> _initializers = new CowList<Action<JsonTransaction<T>>>();
        private readonly CowList<Func<T, ValueTask>> _upserters = new CowList<Func<T, ValueTask>>();
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
                        using (var backup = ZipFile.Open(tempFile.FullName, ZipArchiveMode.Update))
                            foreach (var file in files.Watch(files.Count))
                                using (await _fileLocks.GetOrAdd(file.FullName).EnterAsync())
                                {
                                    using var zip = ZipFile.Open(file.FullName, ZipArchiveMode.Update);
                                    foreach (var entry in zip.Entries)
                                    {
                                        using var source = entry.Open();
                                        using var target = backup.CreateEntry(entry.FullName, CompressionLevel.Optimal).Open();
                                        await source.CopyToAsync(target);
                                        await target.FlushAsync();
                                    }
                                }
                        await tempFile.MoveAsync(_settings.BackupFile);
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
                    using var backup = ZipFile.OpenRead(file.FullName);
                    foreach (var e in backup.Entries)
                        if (e.FullName.EndsWith(".zip"))
                            await e.ExtractToFileAsync(_dataDir);
                        else
                        {
                            var path = Path.Combine(_dataDir.FullName, $"{e.Name}.zip");
                            using var zip = ZipFile.Open(path, ZipArchiveMode.Update);
                            using var target = zip.CreateEntry(e.FullName, CompressionLevel.Optimal).Open();
                            using var source = e.Open();
                            await source.CopyToAsync(target);
                            await target.FlushAsync();
                        }
                    return await InitializeFromData();
                }

                var count = new AtomicInt32();
                var loaded = new AtomicHashSet<string>();
                var threads = Environment.ProcessorCount / 2;
                await Task.WhenAll(Enumerable.Range(0, threads).Select(i => Task.Run(async () =>
                {
                    using var backup = ZipFile.OpenRead(file.FullName);
                    foreach (var entry in backup.Entries.OrderBy(e => e.Length).Where(e => loaded.TryAdd(e.FullName)).Watch(backup.Entries.Count / threads))
                    {
                        using var stream = entry.Open();
                        if (entry.FullName.EndsWith(".zip"))
                        {
                            using var data = new ZipArchive(stream, ZipArchiveMode.Read);
                            await InitializeFromZip(data, false);
                        }
                        else
                            await InitializeFromStream(stream, false);
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
                .Where(f => f.Length > 0)
                .Where(f => f.Name.StartsWith("db."))
                .Where(f => f.Name.EndsWith(".json.zip"))
                .ToList();
            if (files.Count > 0)
                await files.ToAsync().Watch(files.Count).Batch(async file =>
                {
                    try
                    {
                        using var zipFile = ZipFile.OpenRead(file.FullName);
                        await InitializeFromZip(zipFile, false);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error loading: {file}", e);
                    }
                });

            var tempFiles = _dataDir.EnumerateFiles()
                .Where(f => f.Length > 0)
                .Where(f => f.Name.StartsWith("db."))
                .Where(f => f.Name.EndsWith(".json.zip.temp"))
                .ToList();
            if (tempFiles.Count > 0)
                await tempFiles.ToAsync().Watch(tempFiles.Count).Batch(async temp =>
                {
                    try
                    {
                        var file = new FileInfo(temp.FullName.Substring(0, temp.FullName.Length - 5));
                        if (!file.Exists || file.Length == 0)
                        {
                            using var zipFile = ZipFile.OpenRead(temp.FullName);
                            await InitializeFromZip(zipFile, true);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error loading: {temp}", e);
                    }
                });
            return files.Count + tempFiles.Count;
        }

        private async Task InitializeFromZip(ZipArchive zip, bool overwrite)
        {
            foreach (var entry in zip.Entries)
            {
                using var stream = entry.Open();
                await InitializeFromStream(stream, overwrite);
            }
        }

        private async Task InitializeFromStream(Stream stream, bool overwrite)
        {
            var item = await Jsonizer.DeserializeAsync<T>(stream);
            foreach (var initialize in _initializers)
                initialize(new JsonTransaction<T>
                {
                    Item = item,
                    Overwrite = overwrite
                });
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
            _initializers.Add(transaction =>
            {
                var key = keySelector(transaction.Item);
                if (key is null)
                    throw new Exception($"Initialize failed, key is null. Key: {keySelectorExpression}, Document: {typeof(T).FullName}");
                else if (!transaction.Overwrite && index.ContainsKey(key))
                    throw new Exception($"Duplicate entry found in db.json files. Key: {key}");
                index[key] = transaction.Item;
            });
            _upserters.Add(async item =>
            {
                var key = keySelector(item);
                if (key is null)
                    throw new Exception($"Upsert failed, key is null. Key: {keySelectorExpression}, Document: {typeof(T).FullName}");

                if (_settings.DataDir.IsNotNullOrWhiteSpace())
                {
                    var zipFile = GetFile(key.ToString());
                    var zipPath = zipFile.FullName;
                    using (await _fileLocks.GetOrAdd(zipPath).EnterAsync())
                    {
                        IncrementVersion(item, index, key);
                        await new FileInfo($"{zipPath}.temp").Zip(item, zipFile);
                        index[key] = item;
                    }
                }
                else
                    lock (key.ToString())
                    {
                        IncrementVersion(item, index, key);
                        index[key] = item;
                    }
            });
            _deleters.Add(async item =>
            {
                var key = keySelector(item);
                if (key is null)
                    throw new Exception($"Delete failed, key is null. Key: {keySelectorExpression}, Document: {typeof(T).FullName}");
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
                return Jsonizer.DeepCopy(index.TryGetValue(key, out var item) ? item : default);
            };
        }

        private static void IncrementVersion<Key>(T item, AtomicDictionary<Key, T> index, Key key)
        {
            if (index.TryGetValue(key, out var current) && current.Version != item.Version)
                throw new Exception($"Upsert failed, version has changed. Expected: {item.Version}, Actual: {current.Version}");
            item.Version++;
        }

        public Func<Key, ValueTask<T>> CreateIndexMany<Key>(Func<T, IEnumerable<Key>> keySelector)
        {
            keySelector.NotNull(nameof(keySelector));
            var index = new AtomicDictionary<Key, T>();
            _initializers.Add(transaction =>
            {
                foreach (var key in keySelector(transaction.Item))
                    index[key] = transaction.Item;
            });
            _upserters.Add(async item =>
            {
                foreach (var key in keySelector(item))
                    index[key] = item;
            });
            _deleters.Add(async item =>
            {
                foreach (var key in keySelector(item))
                    index.TryRemove(key);
            });
            return async key =>
            {
                await Initialize();
                return Jsonizer.DeepCopy(index.TryGetValue(key, out var item) ? item : default);
            };
        }

        public async ValueTask Upsert(T document)
        {
            document.NotNull(nameof(document));
            await Initialize();
            foreach (var upsert in _upserters)
                await upsert(document);
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
