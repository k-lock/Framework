using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Framework.StateMachine
{
    /// <summary>
    /// Default fluent builder for configuring state transition behaviors.
    /// Supports enum and non-enum states and optional strict validation against a set of allowed states.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public class FluentStateConfigBuilder<TState> : IFluentStateConfigBuilder<TState>
    {
        /// <summary>
        /// Optional set of allowed states for strict validation.
        /// </summary>
        private readonly HashSet<TState> allowedStates = new();

        /// <summary>
        /// Stores all finalized state configurations. Each state maps to its transition configuration.
        /// </summary>
        private readonly Dictionary<TState, StateTransitionConfig<TState>> configs;

        /// <summary>
        /// The configuration currently being built for the active state.
        /// </summary>
        private StateTransitionConfig<TState> currentConfig;

        /// <summary>
        /// The state currently being configured via the fluent API.
        /// </summary>
        private TState currentState;

        /// <summary>
        /// Initializes a new instance of <see cref="FluentStateConfigBuilder{TState}" />.
        /// </summary>
        /// <param name="states">Optional set of allowed states for validation.</param>
        internal FluentStateConfigBuilder(IEnumerable<TState> states = null)
        {
            configs = new Dictionary<TState, StateTransitionConfig<TState>>();

            if (states == null)
            {
                return;
            }

            HashSet<TState> stateSet = states as HashSet<TState> ?? states.ToHashSet();
            if (stateSet.Count > 0)
            {
                allowedStates = new HashSet<TState>(stateSet);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Begins configuration for a specific state.
        /// </summary>
        /// <param name="state">The state to configure.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> For(TState state)
        {
            FluentStateConfigBuilderGuard.ThrowIfAlreadyConfigured(configs, state);
            FluentStateConfigBuilderGuard.ThrowIfInvalidState(state, allowedStates, nameof(For));

            currentState = state;
            currentConfig = new StateTransitionConfig<TState>();
            configs[state] = currentConfig;

            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds allowed transitions for the current state.
        /// </summary>
        /// <param name="states">Target states that are allowed to be transitioned to.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> AllowTransitionsTo(params TState[] states)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState,
                nameof(AllowTransitionsTo));

            foreach (TState state in states)
            {
                FluentStateConfigBuilderGuard.ThrowIfInvalidState(state, allowedStates, nameof(AllowTransitionsTo));
            }

            currentConfig.AddAllowedTransitions(states);
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Configures an automatic transition to a target state.
        /// </summary>
        /// <param name="targetState">The target state for the auto transition.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> WithAutoTransition(TState targetState)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState,
                nameof(WithAutoTransition));
            FluentStateConfigBuilderGuard.ThrowIfInvalidState(targetState, allowedStates, nameof(WithAutoTransition));
            currentConfig.SetAutoTransition(targetState);
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Assigns an asynchronous action to execute when the state is active.
        /// </summary>
        /// <param name="action">The async action.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> Async(Func<UniTask> action)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(Async));
            currentConfig.AsyncAction = action;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Assigns a synchronous action to execute when the state is active.
        /// </summary>
        /// <param name="action">The sync action.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> Sync(Action action)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(Sync));
            currentConfig.AsyncAction = () =>
            {
                action?.Invoke();
                return UniTask.CompletedTask;
            };
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Assigns an action to execute when entering the state.
        /// </summary>
        /// <param name="onEnter">Action to execute.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> OnEnter(Action<TState> onEnter)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(OnEnter));
            currentConfig.OnEnter = onEnter;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Assigns an action to execute when exiting the state.
        /// </summary>
        /// <param name="onExit">Action to execute.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> OnExit(Action<TState> onExit)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(OnExit));
            currentConfig.OnExit = onExit;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Configures a target state to transition to when an error occurs.
        /// </summary>
        /// <param name="targetState">The error target state.</param>
        /// <returns>The fluent builder instance for chaining.</returns>
        public IFluentStateConfigBuilder<TState> OnError(TState targetState)
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(OnError));
            FluentStateConfigBuilderGuard.ThrowIfInvalidState(targetState, allowedStates, nameof(OnError));
            currentConfig.OnError = targetState;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Completes configuration for the current state.
        /// </summary>
        /// <returns>The fluent builder instance.</returns>
        public IFluentStateConfigBuilder<TState> Done()
        {
            FluentStateConfigBuilderGuard.ThrowIfNoActiveConfiguration(currentConfig, currentState, nameof(Done));
            currentConfig = null;
            currentState = default;
            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a dictionary of all configured states and their transition configurations.
        /// Validates transition rules before returning.
        /// </summary>
        /// <returns>Dictionary mapping state to configuration.</returns>
        public Dictionary<TState, IStateTransitionConfig<TState>> Build()
        {
            FluentStateConfigBuilderGuard.ThrowIfInvalidTransitions(configs);
            return configs.ToDictionary
            (
                kvp => kvp.Key,
                kvp => (IStateTransitionConfig<TState>)kvp.Value
            );
        }
    }
}