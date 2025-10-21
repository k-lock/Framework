using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Utils.AsyncLock
{
    /// <summary>
    /// Async lock helper for safe concurrent transitions.
    /// Provides thread-safe asynchronous locking using a semaphore.
    /// </summary>
    public class AsyncLock : IDisposable
    {
        /// <summary>
        /// Semaphore used to control access to the critical section.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new(1, 1);

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the lock acquisition.</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the cancellation token is triggered.</exception>
        public async UniTask<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new Releaser(semaphore);
        }

        /// <summary>
        /// Internal class that releases the semaphore when disposed.
        /// </summary>
        private class Releaser : IDisposable
        {
            /// <summary>
            /// The semaphore to release.
            /// </summary>
            private readonly SemaphoreSlim semaphore;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> class.
            /// </summary>
            /// <param name="semaphore">The semaphore to release on disposal.</param>
            public Releaser(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            /// <summary>
            /// Releases the semaphore when the releaser is disposed.
            /// </summary>
            public void Dispose()
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Disposes the underlying semaphore and releases all resources.
        /// </summary>
        public void Dispose()
        {
            semaphore?.Dispose();
        }
    }
}