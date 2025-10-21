using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that completes immediately.
    /// </summary>
    public class EmptyTransition : TransitionBase
    {
        /// <summary>
        /// Completes immediately after checking for cancellation.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A completed task.</returns>
        public override UniTask WaitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UniTask.CompletedTask;
        }
    }
}