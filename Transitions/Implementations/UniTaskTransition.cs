using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that wraps a UniTask, allowing it to be used in the transition system.
    /// This enables integration of arbitrary async operations into the fluent transition API.
    /// </summary>
    public class UniTaskTransition : TransitionBase
    {
        private readonly UniTask task;

        /// <summary>
        /// Creates a new transition that wraps the specified UniTask.
        /// </summary>
        /// <param name="task">The UniTask to wrap.</param>
        public UniTaskTransition(UniTask task)
        {
            this.task = task;
        }

        /// <summary>
        /// Waits for the wrapped UniTask to complete with cancellation support.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            // Attach the cancellation token to the wrapped task
            await task.AttachExternalCancellation(cancellationToken);
        }
    }
}