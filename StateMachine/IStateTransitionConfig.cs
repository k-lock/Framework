using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Framework.StateMachine
{
    /// <summary>
    /// Represents a configuration for a state transition.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public interface IStateTransitionConfig<TState>
    {
        /// <summary>
        /// State to transition to on success.
        /// </summary>
        [CanBeNull]
        TState OnSuccess { get; }

        /// <summary>
        /// State to transition to on error.
        /// </summary>
        [CanBeNull]
        TState OnError { get; }

        /// <summary>
        /// Optional toggle state allowed for transition.
        /// </summary>
        [CanBeNull]
        TState ToggleState { get; }

        /// <summary>
        /// Determines whether the transition from the current state to the next state is allowed.
        /// </summary>
        bool AllowsTransitionTo(TState currentState, TState nextState);
        
        /// <summary>
        /// Indicates if the state should automatically transition after the async action completes.
        /// </summary>
        public bool AutoTransition { get; set; } 
    }

    /// <summary>
    /// Extends <see cref="IStateTransitionConfig{TState}" /> with enter, exit, and asynchronous actions.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public interface IStateTransitionConfigWithAction<TState> : IStateTransitionConfig<TState>
    {
        /// <summary>
        /// Action to execute when entering this state.
        /// </summary>
        [CanBeNull]
        Action<TState> OnEnter { get; }

        /// <summary>
        /// Action to execute when exiting this state.
        /// </summary>
        [CanBeNull]
        Action<TState> OnExit { get; }

        /// <summary>
        /// Optional asynchronous action to execute when in this state.
        /// </summary>
        [CanBeNull]
        Func<UniTask> AsyncAction { get; }
    }
}