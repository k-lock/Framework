using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Transitions.Base
{
    /// <summary>
    /// Represents a transition that can be awaited and canceled.
    /// </summary>
    public interface ITransition : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Waits for the transition to complete.
        /// </summary>
        UniTask WaitAsync();

        /// <summary>
        /// Waits for the transition to complete with cancellation support.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        UniTask WaitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Makes the transition awaitable without calling WaitAsync() explicitly.
        /// </summary>
        UniTask.Awaiter GetAwaiter();
    }
}