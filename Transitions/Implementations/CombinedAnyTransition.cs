using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that completes when any of the specified transitions complete.
    /// </summary>
    public class CombinedAnyTransition : TransitionBase
    {
        private readonly ITransition[] transitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinedAnyTransition" /> class.
        /// </summary>
        /// <param name="transitions">The transitions to wait for. Empty array completes immediately.</param>
        public CombinedAnyTransition(params ITransition[] transitions)
        {
            this.transitions = transitions ?? Array.Empty<ITransition>();
        }

        /// <summary>
        /// Waits for any of the transitions to complete.
        /// If no transitions are provided, completes immediately.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Empty array completes immediately (consistent with WhenAll behavior)
            if (transitions.Length == 0)
            {
                return;
            }

            if (transitions.Length == 1)
            {
                await transitions[0].WaitAsync(cancellationToken);
                return;
            }

            UniTask[] tasks = transitions.Select(t => t.WaitAsync(cancellationToken)).ToArray();
            await UniTask.WhenAny(tasks);
        }
    }
}