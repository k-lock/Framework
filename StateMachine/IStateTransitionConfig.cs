using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Framework.StateMachine
{
    /// <summary>
    /// Defines the basic structure for a state transition configuration.
    /// Each configuration represents a single state and its transition rules.
    /// </summary>
    /// <typeparam name="TState">The enum or type used for representing states.</typeparam>
    public interface IStateTransitionConfig<TState>
    {
        /// <summary>
        /// Gets the set of states that this state can transition to.
        /// </summary>
        [CanBeNull]
        HashSet<TState> AllowedTransitions { get; }

        /// <summary>
        /// Gets a value indicating whether an automatic transition is configured.
        /// </summary>
        bool HasAutoTransition { get; }

        /// <summary>
        /// Gets the automatically triggered next state, if configured.
        /// </summary>
        [CanBeNull]
        TState AutoTransitionTarget { get; }

        /// <summary>
        /// Gets or sets the state to transition to on error.
        /// </summary>
        [CanBeNull]
        TState OnError { get; set; }

        /// <summary>
        /// Checks whether a transition to the given state is allowed.
        /// </summary>
        /// <param name="nextState">The state to transition to.</param>
        /// <returns>True if the transition is allowed, false otherwise.</returns>
        bool AllowsTransitionTo(TState nextState);
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