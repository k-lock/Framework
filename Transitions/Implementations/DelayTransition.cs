using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that waits for a specified amount of time.
    /// Uses TimeSpan clamping to prevent overflow with extreme values.
    /// </summary>
    public class DelayTransition : TransitionBase
    {
        private readonly TimeSpan delay;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayTransition" /> class.
        /// </summary>
        /// <param name="seconds">The delay duration in seconds. Negative values are clamped to 0, values exceeding TimeSpan.MaxValue are clamped to TimeSpan.MaxValue.</param>
        public DelayTransition(float seconds)
        {
            // Clamp to valid TimeSpan range [0, TimeSpan.MaxValue.TotalSeconds]
            // - Negative values → 0 (UniTask.Delay doesn't accept negative delays)
            // - Values > TimeSpan.MaxValue.TotalSeconds → TimeSpan.MaxValue (prevents overflow)
            
            // Handle edge cases explicitly to prevent TimeSpan overflow
            if (seconds <= 0)
            {
                delay = TimeSpan.Zero;
            }
            else if (seconds >= TimeSpan.MaxValue.TotalSeconds)
            {
                delay = TimeSpan.MaxValue;
            }
            else
            {
                delay = TimeSpan.FromSeconds(seconds);
            }
        }

        /// <summary>
        /// Waits for the specified delay duration.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for canceled requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override UniTask WaitAsync(CancellationToken cancellationToken)
        {
            return UniTask.Delay(delay, cancellationToken: cancellationToken);
        }
    }
}