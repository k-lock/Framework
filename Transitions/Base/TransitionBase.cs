using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Transitions.Base
{
    /// <summary>
    /// Base implementation for transitions with cancellation support.
    /// </summary>
    public abstract class TransitionBase : ITransition
    {
        /// <summary>
        /// Waits for the transition to complete.
        /// The default implementation delegates to WaitAsync(CancellationToken.None).
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual UniTask WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        /// <summary>
        /// Waits for the transition to complete with cancellation support.
        /// This is the primary method that derived classes should implement.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract UniTask WaitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Makes the transition awaitable without calling WaitAsync() explicitly.
        /// </summary>
        /// <returns>An awaiter for the transition.</returns>
        public UniTask.Awaiter GetAwaiter()
        {
            return WaitAsync().GetAwaiter();
        }

        /// <summary>
        /// Implementation for ICriticalNotifyCompletion (required for await support).
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void OnCompleted(Action continuation)
        {
            GetAwaiter().OnCompleted(continuation);
        }

        /// <summary>
        /// Implementation for ICriticalNotifyCompletion (required for await support).
        /// </summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        public void UnsafeOnCompleted(Action continuation)
        {
            GetAwaiter().UnsafeOnCompleted(continuation);
        }
    }
}