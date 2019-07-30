using System;
using System.Threading;

namespace VeeamTest.MultithreadGZip.Multithreading
{
    internal class CountdownEvent : IDisposable
    {
        private bool _isDisposed = false;
        private volatile int _counter;
        private readonly ManualResetEvent _event;

        public CountdownEvent(int counter)
        {
            if (counter <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(counter), "Counter should be equal or greater than 1");
            }

            _counter = counter;
            _event = new ManualResetEvent(false);
        }

        public void Signal()
        {
            ThrowIfDisposed();
            if (_counter <= 0) throw new InvalidOperationException("Counter already equal or lesser than 0");

            int newCount = Interlocked.Decrement(ref _counter);
            if (newCount == 0)
            {
                _event.Set();
            }
            else if (newCount < 0)
            {
                throw new InvalidOperationException("Event is setted in another thread");
            }
        }

        public void Wait()
        {
            ThrowIfDisposed();
            _event.WaitOne();
        }


        public void Dispose()
        {
            if (!_isDisposed)
            {
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
