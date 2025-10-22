using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Framework.StateMachine
{
    /// <summary>
    /// Defines a fluent interface for configuring and building
    /// state transition definitions used in a <see cref="StateMachine{TState}" />.
    /// Supports both enum and non-enum state types.
    /// </summary>
    /// <typeparam name="TState">The type representing the state. Can be an enum or any comparable type.</typeparam>
    public interface IFluentStateConfigBuilder<TState>
    {
        /// <summary>
        /// Begins configuration for the specified state.
        /// </summary>
        /// <param name="state">The state to configure.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> For(TState state);

        /// <summary>
        /// Assigns an asynchronous action that will be executed
        /// when the specified state is entered.
        /// </summary>
        /// <param name="asyncAction">The asynchronous function to execute during this state.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> Async(Func<UniTask> asyncAction);

        /// <summary>
        /// Assigns a synchronous action that will be executed
        /// when the specified state is entered.
        /// </summary>
        /// <param name="action">The synchronous action to execute during this state.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> Sync(Action action);

        /// <summary>
        /// Defines a callback executed when entering the specified state.
        /// </summary>
        /// <param name="onEnter">The action to execute on entering this state.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> OnEnter(Action<TState> onEnter);

        /// <summary>
        /// Defines a callback executed when exiting the specified state.
        /// </summary>
        /// <param name="onExit">The action to execute on exiting this state.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> OnExit(Action<TState> onExit);

        /// <summary>
        /// Specifies which state should follow if the transition fails.
        /// </summary>
        /// <param name="errorState">The target state after a failed transition.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> OnError(TState errorState);

        /// <summary>
        /// Defines allowed target states for manual transitions.
        /// </summary>
        IFluentStateConfigBuilder<TState> AllowTransitionsTo(params TState[] states);

        /// <summary>
        /// Defines an automatic transition to a specific target state.
        /// </summary>
        IFluentStateConfigBuilder<TState> WithAutoTransition(TState targetState);

        /*    /// <summary>
        /// Specifies which state should follow when the transition completes successfully.
        /// </summary>
        /// <param name="nextState">The target state after a successful transition.</param>
        /// <param name="autoTransition">If true, the state machine will automatically transition to the next state after completion.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> OnSuccess(TState nextState, bool autoTransition = false);

        /// <summary>
        /// Specifies multiple states that can follow when the transition completes successfully.
        /// Cannot be used with OnSuccess and does not support auto-transition.
        /// </summary>
        /// <param name="states">The target states after a successful transition.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> OnSuccessSet(params TState[] states);*/

        /// <summary>
        /// Completes the configuration for the current state
        /// and prepares the builder for the next one.
        /// </summary>
        /// <returns>The fluent builder instance for chaining.</returns>
        IFluentStateConfigBuilder<TState> Done();

        /// <summary>
        /// Builds and returns a dictionary containing all configured
        /// state transitions and their associated actions.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}" /> mapping each state
        /// to its transition configuration.
        /// </returns>
        Dictionary<TState, IStateTransitionConfig<TState>> Build();
    }
}