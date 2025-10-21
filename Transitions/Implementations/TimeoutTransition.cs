using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that adds a timeout to another transition.
    /// </summary>
    public class TimeoutTransition : TransitionBase
    {
        private readonly TimeSpan timeout;
        private readonly ITransition transition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutTransition" /> class.
        /// </summary>
        /// <param name="transition">The transition to wrap with a timeout.</param>
        /// <param name="seconds">The timeout duration in seconds. Negative values are clamped to 0.</param>
        public TimeoutTransition(ITransition transition, float seconds)
        {
            this.transition = transition;
            // Clamp negative values to 0 (UniTask.Delay doesn't accept negative delays)
            timeout = TimeSpan.FromSeconds(Math.Max(0, seconds));
        }

        /// <summary>
        /// Waits for the transition to complete or times out.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="TimeoutException">Thrown when the transition exceeds the timeout duration.</exception>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource timeoutCts = new();
            CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource
            (
                cancellationToken,
                timeoutCts.Token
            );

            UniTask timeoutTask = UniTask.Delay(timeout, cancellationToken: timeoutCts.Token);
            UniTask transitionTask = transition.WaitAsync(linkedCts.Token);

            try
            {
                int result = await UniTask.WhenAny(timeoutTask, transitionTask);

                if (result == 0)
                {
                    // Cancel the transition before throwing the timeout exception
                    linkedCts.Cancel();
                    throw new TimeoutException($"Transition timed out after {timeout.TotalSeconds} seconds");
                }
            }
            finally
            {
                timeoutCts.Cancel();
                timeoutCts.Dispose();
                linkedCts.Dispose();
            }
        }
    }
}