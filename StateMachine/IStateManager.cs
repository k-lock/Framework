using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.StateMachine
{
    /// <summary>
    /// Represents a generic asynchronous state machine.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public interface IStateMachine<TState>
    {
        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        TState CurrentState { get; }

        /// <summary>
        /// Attempts to transition to the specified new state asynchronously.
        /// </summary>
        /// <param name="newState">The target state to transition to.</param>
        /// <param name="cancellationToken">Optional token to cancel the transition.</param>
        /// <returns>True if the transition succeeded, false otherwise.</returns>
        UniTask<bool> TransitionToAsync(TState newState, CancellationToken cancellationToken = default);
    }
}