using System;
using System.Collections.Generic;

namespace Framework.StateMachine
{
    internal static class FluentStateConfigBuilderGuard
    {
#if UNITY_EDITOR
        public static void ThrowIfAlreadyConfigured<TState>(Dictionary<TState, StateTransitionConfig<TState>> configs,
            TState state)
        {
            if (configs.ContainsKey(state))
            {
                throw new InvalidOperationException($"State '{state}' has already been configured.");
            }
        }

        /// <summary>
        /// Ensures the current configuration context is active.
        /// </summary>
        public static void ThrowIfNoActiveConfiguration<TState>(StateTransitionConfig<TState> config,
            TState state,
            string methodName)
        {
            if (config == null)
            {
                throw new InvalidOperationException(
                    $"{methodName}() requires an active configuration. Call For(state) first.");
            }

            if (EqualityComparer<TState>.Default.Equals(state, default))
            {
                throw new InvalidOperationException(
                    $"{methodName}() cannot be used before defining a valid state via For(state).");
            }
        }

        /// <summary>
        /// Validates that the state is not the default value.
        /// </summary>
        public static void ThrowIfInvalidState<TState>(TState state, HashSet<TState> allowedStates,
            string methodName)
        {
            if (state == null)
            {
                throw new InvalidOperationException($"State is null in {methodName}().");
            }

            if (EqualityComparer<TState>.Default.Equals(state, default))
            {
                throw new InvalidOperationException(
                    $"State '{state}' in {methodName}() is the default value, which is invalid.");
            }

            if (allowedStates.Count > 0 && !allowedStates.Contains(state))
            {
                throw new InvalidOperationException(
                    $"State '{state}' in {methodName}() is not in the allowed states collection.");
            }
        }

        /// <summary>
        /// Validates transitions, auto-transitions, and allowed transitions for a state.
        /// Throws exceptions on invalid configurations.
        /// </summary>
        public static void ThrowIfInvalidTransitions<TState>(Dictionary<TState, StateTransitionConfig<TState>> configs)
        {
            foreach ((TState state, StateTransitionConfig<TState> config) in configs)
            {
                HashSet<string> errors = new();

                if (config.HasAutoTransition)
                {
                    if (EqualityComparer<TState>.Default.Equals(config.AutoTransitionTarget, default))
                    {
                        errors.Add($"State '{state}' defines an auto-transition but target state is not set.");
                    }

                    if (!config.AllowsTransitionTo(config.AutoTransitionTarget))
                    {
                        errors.Add(
                            $"State '{state}' auto-transition target '{config.AutoTransitionTarget}' is not in AllowedTransitions.");
                    }
                }

                if ((config.AllowedTransitions?.Count ?? 0) == 0 && !config.HasAutoTransition)
                {
                    errors.Add($"State '{state}' has no allowed transitions and no auto-transition defined.");
                }

                if (config.AllowedTransitions?.Contains(state) == true)
                {
                    errors.Add($"State '{state}' allows transition to itself (self-transition), which is invalid.");
                }

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid configuration for state '{state}': {string.Join("; ", errors)}");
                }
            }
        }
#endif
#if !UNITY_EDITOR
        public static void ThrowIfAlreadyConfigured<TState>(Dictionary<TState, StateTransitionConfig<TState>> configs, TState currentState){}
        public static void ThrowIfNoActiveConfiguration<TState>(StateTransitionConfig<TState> config, TState currentState,string methodName){ }
        public static void ThrowIfInvalidState<TState>(TState state, HashSet<TState> allowedStates,string methodName){ }
        public static void ThrowIfInvalidTransitions<TState>(StateTransitionConfig<TState> config, TState state){ }
#endif
    }
}