using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    /// <summary>
    /// Holds a Value and indicates whether or not a Value is held.
    /// </summary>
    /// <typeparam name="T">Specifies the Type of Value held.</typeparam>
    public struct Result<T>
    {
        /// <summary>
        /// Whether or not a value has been assigned to Value.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// The assigned value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Set HasValue to True and Value to the passed value. To set HasValue to false use the default constructor.
        /// </summary>
        /// <param name="value">The value to assign to Value</param>
        public Result(T value)
        {
            HasValue = true;
            Value = value;
        }
    }

    /// <summary>
    /// A wrapper for TaskCompletionSource that executes TrySetResult with Task.Run().
    /// This prevents TrySetResult from blocking the current thread if a long running operation is awaiting its completion.
    /// </summary>
    /// <typeparam name="T">Specifies the Type returned on completion.</typeparam>
    public class AsyncResult<T>
    {
        private readonly TaskCompletionSource<T> _source = new TaskCompletionSource<T>();
        private readonly AtomicBool _complete = new AtomicBool();

        /// <summary>
        /// Returns a Task to await completion.
        /// </summary>
        public Task<T> Source => _source.Task;

        /// <summary>
        /// Returns a ValueTask to await completion.
        /// </summary>
        public ValueTask<T> ValueSource => new ValueTask<T>(_source.Task);

        /// <summary>
        /// Whether or not a result has been set.
        /// </summary>
        public bool Completed => _complete;

        /// <summary>
        /// Returns the completed value if result has been set otherwise returns the default for T.
        /// </summary>
        public T ValueOrDefault => Completed ? Source.Result : default;

        /// <summary>
        /// Attempts to set the result value. If already completed will return false.
        /// </summary>
        /// <param name="result">The value for completion</param>
        /// <returns>Whether or not the passed result has been set as the completed value.</returns>
        public bool TryComplete(T result)
        {
            if (!_complete.TrySet(true))
                return false;
            Task.Run(() => _source.TrySetResult(result)).Catch();
            return true;
        }
    }

    /// <summary>
    /// <para>A Queue of values that can be awaited for a new value to be queued.</para>
    /// <para>Enumeration will create a snapshot of items in the queue. This allows concurrent modification but changes will not be reflected in the snapshot.</para>
    /// <para>Locking on an instance of this class will block all methods from executing outside of the lock until it is released.
    /// This can be used to ensure a group of transactions are atomic.</para>
    /// </summary>
    /// <typeparam name="T">Specifies the Type of items that will be queued.</typeparam>
    public class AsyncQueue<T> : IAtomicCollection<T>
    {
        private readonly Queue<AsyncResult<Result<T>>> _waiters = new Queue<AsyncResult<Result<T>>>();
        private readonly Queue<AsyncResult<Result<T>>> _peekers = new Queue<AsyncResult<Result<T>>>();
        private readonly AtomicBool _completed = new AtomicBool();
        private readonly Queue<T> _items;

        /// <summary>
        /// Returns true if Complete() has been invoked, false otherwise.
        /// </summary>
        public bool Completed => _completed;

        /// <summary>
        /// Creates a new instance with default capacity.
        /// </summary>
        public AsyncQueue()
        {
            _items = new Queue<T>();
        }

        /// <summary>
        /// Creates a new instance populated with items available in the passed IEnumerable
        /// </summary>
        /// <param name="items">Items to populate the queue with.</param>
        public AsyncQueue(IEnumerable<T> items)
        {
            _items = new Queue<T>(items.NotNull(nameof(items)));
        }

        /// <summary>
        /// An estimate of items available in the queue. This is an eventually accurate estimate multiple reads may be necessary to determine true item count.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Adds an item to the queue. The first consumer awaiting an item will be notified.
        /// </summary>
        /// <param name="item">The item to add to the queue.</param>
        public void Add(T item)
        {
            lock (this)
            {
                if (_completed)
                    throw new Exception("Queue has been completed.");
                var result = new Result<T>(item);
                while (_waiters.Count > 0)
                    if (_waiters.Dequeue().TryComplete(result))
                        return;

                _items.Enqueue(item);
                while (_peekers.Count > 0)
                    _peekers.Dequeue().TryComplete(result);
            }
        }

        /// <summary>
        /// Notifies all consumers that no more items are expected to be added to the queue.
        /// </summary>
        public void Complete()
        {
            if (_completed.TrySet(true))
                lock (this)
                {
                    while (_waiters.Count > 0)
                        _waiters.Dequeue().TryComplete(default);
                    while (_peekers.Count > 0)
                        _peekers.Dequeue().TryComplete(default);
                }
        }

        /// <summary>
        /// Adds a range of items to the queue.
        /// </summary>
        /// <param name="items">Items to add to the queue.</param>
        public void AddRange(IEnumerable<T> items)
        {
            items.NotNull(nameof(items));
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        /// Adds a range of items to the queue. This will block other methods from executing until all items have been enumerated and added to the queue.
        /// </summary>
        /// <param name="items">Items to add to the queue.</param>
        public void AddRangeUnsafe(IEnumerable<T> items)
        {
            items.NotNull(nameof(items));
            lock (this)
                foreach (var item in items)
                    Add(item);
        }

        /// <summary>
        /// Tries to add an item to the queue. Will succeed if queue has not been completed.
        /// </summary>
        /// <param name="item">The item to add to the queue.</param>
        /// <returns>True if successful, False otherwise.</returns>
        public bool TryAdd(T item)
        {
            if (_completed)
                return false;
            Add(item);
            return true;
        }

        /// <summary>
        /// Implemented for compatibility but use of this method should generally be avoided. It will remove all items from the queue and re-add them except for the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The item to remove.</returns>
        public bool TryRemove(T item)
        {
            lock (this)
            {
                if (_items.Contains(item))
                {
                    var comparer = EqualityComparer<T>.Default;
                    AddRangeUnsafe(GetAndClear().Where(i => comparer.NotEquals(i, item)));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Synchronously attempt to consume an item from the list. Asynchronous consumers will have priority over calls to this method.
        /// </summary>
        /// <param name="item">The item consumed from the queue.</param>
        /// <returns>Whether or not an item was consumed from the queue.</returns>
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

        /// <summary>
        /// Consumes an item from the queue immediately, awaits indefinitely for an item to be added to the queue, or awaits until the queue is Completed.
        /// </summary>
        /// <returns>A result indicating whether or not a value was consumed and the value that was or was not consumed.</returns>
        public ValueTask<Result<T>> TryConsumeAsync()
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Dequeue()));
                if (_completed)
                    return default;
                var waiter = new AsyncResult<Result<T>>();
                _waiters.Enqueue(waiter);
                return waiter.ValueSource;
            }
        }

        /// <summary>
        /// Consumes an item from the queue immediately, awaits the specified amount of time for an item to be added to the queue, or awaits until the queue is Completed.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for an item.</param>
        /// <returns>A result indicating whether or not a value was consumed and the value that was or was not consumed.</returns>
        public ValueTask<Result<T>> TryConsumeAsync(int millisecondsTimeout)
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Dequeue()));
                while (_waiters.Count > 0 && _waiters.Peek().Completed)
                    _waiters.Dequeue();
                if (_completed)
                    return default;
                var waiter = new AsyncResult<Result<T>>();
                AsyncTimer.Delay(millisecondsTimeout).Continue(() => waiter.TryComplete(default)).Catch();
                _waiters.Enqueue(waiter);
                return waiter.ValueSource;
            }
        }

        /// <summary>
        /// Creates an IAsyncEnumerable that will consume items from the queue until Complete() is invoked.
        /// </summary>
        public async IAsyncEnumerable<T> ConsumeAsync()
        {
            var result = await TryConsumeAsync();
            while (result.HasValue)
            {
                yield return result.Value;
                result = await TryConsumeAsync();
            }
        }

        /// <summary>
        /// Creates an IAsyncEnumerable that will consume items from the queue until it is exhausted, Complete() is invoked, or timeout is reached.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for an item before terminating consumption. Must be greater than 0.</param>
        public async IAsyncEnumerable<T> ConsumeAsync(int millisecondsTimeout)
        {
            if (millisecondsTimeout <= 0)
                throw new ArgumentException("millisecondsTimeout <= 0");
            var result = await TryConsumeAsync(millisecondsTimeout);
            while (result.HasValue)
            {
                yield return result.Value;
                result = await TryConsumeAsync(millisecondsTimeout);
            }
        }

        /// <summary>
        /// Returns the first item from the queue immediately without consuming it, awaits indefinitely for an item to be added to the queue, or awaits until the queue is Completed.
        /// </summary>
        /// <returns>A result indicating whether or not a value was consumed and the value that was or was not consumed.</returns>
        public ValueTask<Result<T>> PeekAsync()
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Peek()));
                if (_completed)
                    return default;
                var peeker = new AsyncResult<Result<T>>();
                _peekers.Enqueue(peeker);
                return peeker.ValueSource;
            }
        }

        /// <summary>
        /// Returns the first item from the queue immediately without consuming it, awaits the specified amount of time for an item to be added to the queue, or awaits until the queue is Completed.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for an item.</param>
        /// <returns>A result indicating whether or not a value was consumed and the value that was or was not consumed.</returns>
        public ValueTask<Result<T>> PeekAsync(int millisecondsTimeout)
        {
            lock (this)
            {
                if (_items.Count > 0)
                    return new ValueTask<Result<T>>(new Result<T>(_items.Peek()));
                while (_peekers.Count > 0 && _peekers.Peek().Completed)
                    _peekers.Dequeue();
                if (_completed)
                    return default;
                var peeker = new AsyncResult<Result<T>>();
                AsyncTimer.Delay(millisecondsTimeout).Continue(() => peeker.TryComplete(default)).Catch();
                _peekers.Enqueue(peeker);
                return peeker.ValueSource;
            }
        }

        /// <summary>
        /// Clears the queue and returns the items cleared.
        /// </summary>
        /// <returns>The items cleared from the queue.</returns>
        public IEnumerable<T> GetAndClear()
        {
            lock (this)
            {
                var items = _items.ToList();
                _items.Clear();
                return items;
            }
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (this)
                _items.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Creates a snapshot of the queue and returns an enumerator for the snapshot.
        /// </summary>
        /// <returns>Enumerator for the snapshot</returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
                return _items.ToList().GetEnumerator();
        }
    }
}