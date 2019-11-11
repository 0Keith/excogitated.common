using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public struct Result<T>
    {
        public bool HasValue { get; }
        public T Value { get; }

        public Result(T value)
        {
            HasValue = true;
            Value = value;
        }
    }

    public class AsyncResult<T>
    {
        private readonly TaskCompletionSource<T> _source = new TaskCompletionSource<T>();
        private readonly AtomicBool _complete = new AtomicBool();

        public ValueTask<T> Source => new ValueTask<T>(_source.Task);
        public bool Completed => _complete.Value;
        public T Value => Completed ? _source.Task.Result : default;

        public bool TryComplete(T result)
        {
            if (!_complete.TrySet(true))
                return false;
            Task.Run(() => _source.TrySetResult(result));
            return true;
        }
    }

    public class AsyncQueue<T> : IAtomicCollection<T>
    {
        private readonly Queue<AsyncResult<Result<T>>> _waiters = new Queue<AsyncResult<Result<T>>>();
        private readonly Queue<AsyncResult<Result<T>>> _peekers = new Queue<AsyncResult<Result<T>>>();
        private readonly Queue<T> _items = new Queue<T>();

        public int Count => _items.Count;

        public void Add(T item)
        {
            lock (this)
            {
                var result = new Result<T>(item);
                while (_waiters.Count > 0)
                    if (_waiters.Dequeue().TryComplete(result))
                        return;

                _items.Enqueue(item);
                while (_peekers.Count > 0)
                    _peekers.Dequeue().TryComplete(result);
            }
        }

        public void Complete()
        {
            lock (this)
            {
                while (_waiters.Count > 0)
                    _waiters.Dequeue().TryComplete(default);
                while (_peekers.Count > 0)
                    _peekers.Dequeue().TryComplete(default);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
                foreach (var item in items)
                    Add(item);
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public bool TryRemove(T item)
        {
            lock (this)
            {
                if (_items.Contains(item))
                {
                    AddRange(GetAndClear().Except(new[] { item }));
                    return true;
                }
                return false;
            }
        }

        public bool TryConsume(out T item)
        {
            lock (this)
            {
                if (_items.Count > 0)
                {
                    item = _items.Dequeue();
                    return true;
                }
                item = default;
                return false;
            }
        }

        public ValueTask<T> ConsumeAsync()
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<T>(_items.Dequeue());
                var waiter = new AsyncResult<Result<T>>();
                _waiters.Enqueue(waiter);
                return waiter.Source.Select(r => r.Value);
            }
        }

        public ValueTask<Result<T>> ConsumeAsync(int timeout)
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Dequeue()));
                while (_waiters.Count > 0 && _waiters.Peek().Completed)
                    _waiters.Dequeue();
                var waiter = new AsyncResult<Result<T>>();
                Task.Run(async () =>
                {
                    await Task.Delay(timeout);
                    waiter.TryComplete(default);
                });
                _waiters.Enqueue(waiter);
                return waiter.Source;
            }
        }

        public ValueTask<T> PeekAsync()
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<T>(_items.Peek());
                var peeker = new AsyncResult<Result<T>>();
                _peekers.Enqueue(peeker);
                return peeker.Source.Select(r => r.Value);
            }
        }

        public ValueTask<Result<T>> PeekAsync(int timeout)
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Peek()));
                while (_peekers.Count > 0 && _peekers.Peek().Completed)
                    _peekers.Dequeue();
                var peeker = new AsyncResult<Result<T>>();
                Task.Run(async () =>
                {
                    await Task.Delay(timeout);
                    peeker.TryComplete(default);
                });
                _peekers.Enqueue(peeker);
                return peeker.Source;
            }
        }

        public IEnumerable<T> GetAndClear()
        {
            lock (this)
            {
                var items = _items.ToList();
                _items.Clear();
                return items;
            }
        }

        public void Clear()
        {
            lock (this)
                _items.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
                return _items.ToList().GetEnumerator();
        }

    }
}