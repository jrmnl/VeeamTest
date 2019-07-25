using System;
using System.Collections.Concurrent;
using System.Threading;

namespace VeeamTest
{
    public class Consumer<T> : IDisposable
    {
        private bool _isDisposed = false;
        private readonly BlockingCollection<T> _items;
        private readonly CountdownEvent _event;

        public Consumer(Action<T> action, int degreeOfParallelism = 1)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (degreeOfParallelism <= 0) throw new ArgumentOutOfRangeException();

            _items = new BlockingCollection<T>();
            _event = new CountdownEvent(degreeOfParallelism);

            for (int i = 0; i < degreeOfParallelism; i++)
            {
                var thread = new Thread(() =>
                {
                    while (!_items.IsCompleted)
                    {
                        if (_items.TryTake(out var item))
                        {
                            action(item);
                        }
                    }
                    _event.Signal();
                });
                thread.Start();
            }
        }

        public void Enqueue(T item)
        {
            ThrowIfDisposed();
            _items.Add(item);
        }

        public void RequestCompletion()
        {
            ThrowIfDisposed();
            _items.CompleteAdding();
        }

        public void Wait()
        {
            ThrowIfDisposed();
            _event.Wait();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _items.Dispose();
                _event.Dispose();

                _isDisposed = true;
            }
        }
        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(CountdownEvent));
        }
    }
}
