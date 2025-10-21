using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that executes child transitions sequentially.
    /// </summary>
    public class SequentialTransition : TransitionBase
    {
        private readonly ITransition[] transitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialTransition" /> class.
        /// </summary>
        /// <param name="transitions">The transitions to execute sequentially.</param>
        public SequentialTransition(params ITransition[] transitions)
        {
            this.transitions = transitions ?? Array.Empty<ITransition>();
        }

        /// <summary>
        /// Executes all transitions sequentially.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            foreach (ITransition transition in transitions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await transition.WaitAsync(cancellationToken);
            }
        }
    }
}