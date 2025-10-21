using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that waits for all child transitions to complete.
    /// </summary>
    public class CombinedAllTransition : TransitionBase
    {
        private readonly ITransition[] transitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinedAllTransition" /> class.
        /// </summary>
        /// <param name="transitions">The transitions to wait for.</param>
        public CombinedAllTransition(params ITransition[] transitions)
        {
            this.transitions = transitions ?? Array.Empty<ITransition>();
        }

        /// <summary>
        /// Waits for all transitions to complete.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            UniTask[] tasks = transitions.Select(t => t.WaitAsync(cancellationToken)).ToArray();
            await UniTask.WhenAll(tasks);
        }
    }
}