using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Framework.StateMachine
{
    /// <summary>
    /// Default implementation of <see cref="IStateTransitionConfigWithAction{TState}"/>.
    /// Stores state transition targets, lifecycle actions, and optional async actions.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public class StateTransitionConfig<TState> : IStateTransitionConfigWithAction<TState>
    {
        /// <summary>
        /// The target state if the transition succeeds.
        /// </summary>
        public TState OnSuccess { get; set; }

        /// <summary>
        /// The target state to roll back to if the transition fails.
        /// </summary>
        public TState OnError { get; set; }

        /// <summary>
        /// An optional toggle state that can also be transitioned to from the current state.
        /// </summary>
        public TState ToggleState { get; set; }

        /// <summary>
        /// Action invoked when entering this state.
        /// </summary>
        public Action<TState> OnEnter { get; set; }

        /// <summary>
        /// Action invoked when exiting this state.
        /// </summary>
        public Action<TState> OnExit { get; set; }

        /// <summary>
        /// Optional asynchronous action executed when entering this state.
        /// </summary>
        public Func<UniTask> AsyncAction { get; set; }

        /// <summary>
        /// Determines if a transition from the current state to the next state is allowed.
        /// </summary>
        /// <param name="currentState">The current state.</param>
        /// <param name="nextState">The target state.</param>
        /// <returns>True if the transition is allowed; otherwise false.</returns>
        public bool AllowsTransitionTo(TState currentState, TState nextState)
        {
            return !EqualityComparer<TState>.Default.Equals(nextState, currentState) &&
                   (EqualityComparer<TState>.Default.Equals(OnSuccess, nextState) ||
                    EqualityComparer<TState>.Default.Equals(ToggleState, nextState));
        }
    }
}
