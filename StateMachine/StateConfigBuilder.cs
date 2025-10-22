using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.StateMachine
{
    /// <summary>
    /// Builder class for creating state machine configurations.
    /// Provides both fluent API and callback-based configuration methods.
    /// </summary>
    public static class StateConfigBuilder
    {
        /// <summary>
        /// Starts a fluent configuration builder for a manually defined set of states.
        /// </summary>
        /// <typeparam name="TState">The type used for states.</typeparam>
        /// <returns>A fluent builder instance for chaining configuration calls.</returns>
        public static FluentStateConfigBuilder<TState> CreateFluent<TState>()
        {
            return new FluentStateConfigBuilder<TState>();
        }

        /// <summary>
        /// Starts a fluent configuration builder for an enum-based state machine.
        /// Automatically extracts all enum values and enables strict state validation.
        /// </summary>
        /// <typeparam name="TState">The enum type used for states.</typeparam>
        /// <returns>A fluent builder instance for chaining configuration calls.</returns>
        public static FluentStateConfigBuilder<TState> CreateFluentForEnum<TState>() where TState : Enum
        {
            return new FluentStateConfigBuilder<TState>();
        }

        /// <summary>
        /// Creates a dictionary of state configurations from a collection of states using a callback.
        /// Legacy method - consider using <see cref="CreateFluent{TState}" /> for better type safety.
        /// </summary>
        /// <typeparam name="TState">The type used for states.</typeparam>
        /// <param name="states">Collection of states to configure.</param>
        /// <param name="configure">Action to configure each state.</param>
        /// <returns>Dictionary mapping states to their configurations.</returns>
        public static Dictionary<TState, IStateTransitionConfig<TState>> Create<TState>
            (IEnumerable<TState> states, Action<StateTransitionConfig<TState>, TState> configure)
        {
            Dictionary<TState, IStateTransitionConfig<TState>> dict = new();
            foreach (TState state in states)
            {
                AddStateConfig(dict, state, configure);
            }

            return dict;
        }

        /// <summary>
        /// Creates a dictionary of state configurations for all values of an enum type.
        /// </summary>
        /// <typeparam name="TState">The enum type used for states.</typeparam>
        /// <param name="configure">Action to configure each state.</param>
        /// <returns>Dictionary mapping all enum values to their configurations.</returns>
        public static Dictionary<TState, IStateTransitionConfig<TState>> CreateForEnum<TState>
            (Action<StateTransitionConfig<TState>, TState> configure) where TState : Enum
        {
            Dictionary<TState, IStateTransitionConfig<TState>> dict = new();
            foreach (TState state in Enum.GetValues(typeof(TState)))
            {
                AddStateConfig(dict, state, configure);
            }

            return dict;
        }

        /// <summary>
        /// Adds a state configuration to the dictionary.
        /// </summary>
        /// <typeparam name="TState">The type used for states.</typeparam>
        /// <param name="dict">The dictionary to add the configuration to.</param>
        /// <param name="state">The state to configure.</param>
        /// <param name="configure">Action to configure the state.</param>
        private static void AddStateConfig<TState>(Dictionary<TState, IStateTransitionConfig<TState>> dict,
            TState state, Action<StateTransitionConfig<TState>, TState> configure)
        {
            StateTransitionConfig<TState> config = new();
            configure?.Invoke(config, state);

            if (!dict.TryAdd(state, config))
            {
                Debug.LogWarning($"[StateConfigBuilder] AddStateConfig : State {state} already exists in the config.");
            }
        }
    }
}