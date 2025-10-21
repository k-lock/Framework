using DG.Tweening;
using Framework.Transitions.Implementations;

namespace Framework.Transitions.Base
{
    /// <summary>
    /// Main entry point for creating transitions.
    /// </summary>
    public static class Transition
    {
        /// <summary>
        /// Creates an empty transition that completes immediately.
        /// </summary>
        /// <returns>A new empty transition.</returns>
        public static ITransition Create()
        {
            return new EmptyTransition();
        }

        /// <summary>
        /// Creates a transition that waits for the specified delay.
        /// </summary>
        /// <param name="seconds">The delay duration in seconds.</param>
        /// <returns>A new delay transition.</returns>
        public static ITransition Delay(float seconds)
        {
            return new DelayTransition(seconds);
        }

        /// <summary>
        /// Creates a transition that waits for all specified transitions to complete.
        /// </summary>
        /// <param name="transitions">The transitions to wait for.</param>
        /// <returns>A new combined transition that waits for all.</returns>
        public static ITransition WhenAll(params ITransition[] transitions)
        {
            return new CombinedAllTransition(transitions);
        }

        /// <summary>
        /// Creates a transition that waits for any of the specified transitions to complete.
        /// </summary>
        /// <param name="transitions">The transitions to wait for.</param>
        /// <returns>A new combined transition that waits for any.</returns>
        public static ITransition WhenAny(params ITransition[] transitions)
        {
            return new CombinedAnyTransition(transitions);
        }

        /// <summary>
        /// Creates a transition with a timeout.
        /// </summary>
        /// <param name="seconds">The timeout duration in seconds.</param>
        /// <param name="transition">The transition to wrap with a timeout.</param>
        /// <returns>A new timeout transition.</returns>
        public static ITransition WithTimeout(float seconds, ITransition transition)
        {
            return new TimeoutTransition(transition, seconds);
        }

        /// <summary>
        /// Creates a transition that waits for the specified DOTween tween to complete.
        /// </summary>
        /// <param name="tween">The tween to wait for.</param>
        /// <returns>A new DOTween transition.</returns>
        public static ITransition FromTween(Tween tween)
        {
            return new DoTweenTransition(tween);
        }
    }
}