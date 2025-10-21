using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;
using Framework.Transitions.Implementations;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for transitions.
    /// </summary>
    public static class TransitionExtensions
    {
        /// <summary>
        /// Combines this transition with another transition, waiting for both to complete.
        /// </summary>
        public static ITransition And(this ITransition transition, ITransition other)
        {
            return new CombinedAllTransition(transition, other);
        }

        /// <summary>
        /// Combines this transition with another transition, waiting for either to complete.
        /// </summary>
        public static ITransition Or(this ITransition transition, ITransition other)
        {
            return new CombinedAnyTransition(transition, other);
        }

        /// <summary>
        /// Executes another transition after this transition completes.
        /// </summary>
        public static ITransition Then(this ITransition transition, ITransition next)
        {
            return new SequentialTransition(transition, next);
        }

        /// <summary>
        /// Adds a delay after this transition completes.
        /// </summary>
        public static ITransition ThenDelay(this ITransition transition, float seconds)
        {
            return new SequentialTransition(transition, new DelayTransition(seconds));
        }

        /// <summary>
        /// Adds a timeout to this transition.
        /// </summary>
        public static ITransition WithTimeout(this ITransition transition, float seconds)
        {
            return new TimeoutTransition(transition, seconds);
        }

        /// <summary>
        /// Creates a transition that can be canceled with the specified token.
        /// This doesn't change the transition itself but provides a more fluent API
        /// for specifying a cancellation token when executing the transition.
        /// </summary>
        public static UniTask WithCancellation(this ITransition transition, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return transition.WaitAsync(cancellationToken);
        }
    }
}