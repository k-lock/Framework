using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.StateMachine
{
    /// <summary>
    /// Default fluent builder implementation for configuring state transition behaviors.
    /// Compatible with both enum and non-enum states.
    /// Supports optional state validation when a predefined set of states is provided.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public class FluentStateConfigBuilder<TState> : IFluentStateConfigBuilder<TState>
    {
        /// <summary>
        /// Optional set of allowed states for validation. If null, all states are permitted.
        /// </summary>
        private readonly HashSet<TState> allowedStates;

        /// <summary>
        /// Stores all finalized state configurations. Each state maps to its complete transition configuration.
        /// </summary>
        private readonly Dictionary<TState, StateTransitionConfig<TState>> configs = new();

        /// <summary>
        /// The configuration currently being built for the active state.
        /// </summary>
        private StateTransitionConfig<TState> currentConfig;

        /// <summary>
        /// The state currently being configured via the fluent API.
        /// </summary>
        private TState currentState;

        /// <summary>
        /// Tracks whether a state configuration is currently active (between For() and Done() calls).
        /// </summary>
        private bool hasActiveConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentStateConfigBuilder{TState}" /> class.
        /// </summary>
        /// <param name="states">
        /// Optional collection of allowed states. If provided, only these states can be configured and
        /// referenced.
        /// </param>
        public FluentStateConfigBuilder(IEnumerable<TState> states = null)
        {
            if (states == null)
            {
                return;
            }

            allowedStates = new HashSet<TState>(states);

            if (allowedStates.Count != 0)
            {
                return;
            }

            Debug.LogWarning(
                "[FluentStateConfigBuilder] Empty states collection provided. No state validation will be performed.");
            allowedStates = null;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> For(TState state)
        {
            // Validate state is allowed (if validation is enabled)
            if (allowedStates != null && !allowedStates.Contains(state))
            {
                throw new InvalidOperationException(
                    $"[FluentStateConfigBuilder] State '{state}' is not in the allowed states list. " +
                    "Only states provided during construction can be configured.");
            }

            // Warn if previous configuration wasn't finalized
            if (HasActiveConfiguration())
            {
                Debug.LogWarning(
                    $"[FluentStateConfigBuilder] Starting new state '{state}' without calling Done() on previous state '{currentState}'. Previous configuration will be lost.");
            }

            currentState = state;
            currentConfig = new StateTransitionConfig<TState>();
            hasActiveConfig = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> Async(Func<UniTask> asyncAction)
        {
            EnsureActiveConfiguration(nameof(Async),
                "Async() requires an active state configuration. Call For(state) before defining async actions.");
            currentConfig.AsyncAction = asyncAction;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> Sync(Action action)
        {
            EnsureActiveConfiguration(nameof(Sync),
                "Sync() requires an active state configuration. Call For(state) before defining sync actions.");
            currentConfig.AsyncAction = () =>
            {
                action?.Invoke();
                return UniTask.CompletedTask;
            };
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> OnEnter(Action<TState> onEnter)
        {
            EnsureActiveConfiguration(nameof(OnEnter),
                "OnEnter() requires an active state configuration. Call For(state) before defining enter callbacks.");
            currentConfig.OnEnter = onEnter;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> OnExit(Action<TState> onExit)
        {
            EnsureActiveConfiguration(nameof(OnExit),
                "OnExit() requires an active state configuration. Call For(state) before defining exit callbacks.");
            currentConfig.OnExit = onExit;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> OnSuccess(TState nextState, bool autoTransition = false)
        {
            EnsureActiveConfiguration(nameof(OnSuccess), "OnSuccess() requires an active state configuration. Call For(state) before defining success transitions.");
            ValidateStateReference(nextState, nameof(OnSuccess));
            currentConfig.OnSuccess = nextState;
            currentConfig.AutoTransition = autoTransition;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> OnError(TState errorState)
        {
            EnsureActiveConfiguration(nameof(OnError),
                "OnError() requires an active state configuration. Call For(state) before defining error transitions.");
            ValidateStateReference(errorState, nameof(OnError));
            currentConfig.OnError = errorState;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> Toggle(TState toggleState)
        {
            EnsureActiveConfiguration(nameof(Toggle),
                "Toggle() requires an active state configuration. Call For(state) before defining toggle transitions.");
            ValidateStateReference(toggleState, nameof(Toggle));
            currentConfig.ToggleState = toggleState;
            return this;
        }

        /// <inheritdoc />
        public IFluentStateConfigBuilder<TState> Done()
        {
            if (!HasActiveConfiguration())
            {
                Debug.LogWarning(
                    "[FluentStateConfigBuilder] Done() called without active state configuration. Call For() first.");
                return this;
            }

            if (!configs.TryAdd(currentState, currentConfig))
            {
                Debug.LogWarning(
                    $"[FluentStateConfigBuilder] State {currentState} already exists in the configuration. Skipping duplicate.");
            }

            currentConfig = null;
            hasActiveConfig = false;
            return this;
        }

        /// <inheritdoc />
        public Dictionary<TState, IStateTransitionConfig<TState>> Build()
        {
            if (HasActiveConfiguration())
            {
                Debug.LogWarning(
                    $"[FluentStateConfigBuilder] Build() called with unfinalized state '{currentState}'. Call Done() to include it. This state will be excluded from the final configuration.");
            }

            Dictionary<TState, IStateTransitionConfig<TState>> result = new Dictionary<TState, IStateTransitionConfig<TState>>();
            foreach ((TState state, StateTransitionConfig<TState> config) in configs)
            {
                result[state] = config;
            }

            return result;
        }

        /// <summary>
        /// Checks if there is an active state configuration being built.
        /// </summary>
        /// <returns>True if a configuration is active; otherwise, false.</returns>
        private bool HasActiveConfiguration()
        {
            return hasActiveConfig && currentConfig != null;
        }

        /// <summary>
        /// Ensures that a state configuration is active before modifying it.
        /// </summary>
        /// <param name="methodName">The name of the calling method for error reporting.</param>
        /// <param name="customMessage">Optional custom error message. If null, a default message is generated.</param>
        /// <exception cref="InvalidOperationException">Thrown when no active configuration exists.</exception>
        private void EnsureActiveConfiguration(string methodName, string customMessage = null)
        {
            if (HasActiveConfiguration())
            {
                return;
            }

            string message = customMessage ??
                             $"{methodName}() called without an active state configuration. Call For(state) first to begin configuring a state.";

            throw new InvalidOperationException($"[FluentStateConfigBuilder] {message}");
        }

        /// <summary>
        /// Validates that a referenced state is in the allowed states list (if validation is enabled).
        /// </summary>
        /// <param name="state">The state to validate.</param>
        /// <param name="methodName">The name of the calling method for error reporting.</param>
        /// <exception cref="InvalidOperationException">Thrown when the state is not in the allowed states list.</exception>
        private void ValidateStateReference(TState state, string methodName)
        {
            if (allowedStates != null && !allowedStates.Contains(state))
            {
                throw new InvalidOperationException(
                    $"[FluentStateConfigBuilder] {methodName}() references state '{state}' which is not in the allowed states list. " +
                    "Only states provided during construction can be referenced.");
            }
        }
    }
}