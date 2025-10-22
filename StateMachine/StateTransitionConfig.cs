using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Framework.StateMachine
{
    /// <summary>
    /// Represents a state configuration containing allowed transitions,
    /// optional automatic transitions, and callbacks for enter/exit events.
    /// </summary>
    /// <typeparam name="TState">The type used for states.</typeparam>
    public class StateTransitionConfig<TState> : IStateTransitionConfigWithAction<TState>
    {
        /// <summary>
        /// Internal collection of allowed target states for manual transitions.
        /// </summary>
        private readonly HashSet<TState> allowedTransitionStates = new();

        /// <summary>
        /// Backing field for the automatic transition target.
        /// </summary>
        private TState autoTransitionTarget;

        /// <summary>
        /// Indicates whether this state has an auto-transition defined.
        /// </summary>
        private bool hasAutoTransitionTarget;

        /// <inheritdoc />
        public HashSet<TState> AllowedTransitions => new(allowedTransitionStates);

        /// <inheritdoc />
        public bool HasAutoTransition => hasAutoTransitionTarget;

        /// <inheritdoc />
        public TState AutoTransitionTarget => autoTransitionTarget;

        /// <inheritdoc />
        public TState OnError { get; set; }

        /// <inheritdoc />
        public Action<TState> OnEnter { get; set; }

        /// <inheritdoc />
        public Action<TState> OnExit { get; set; }

        /// <inheritdoc />
        public Func<UniTask> AsyncAction { get; set; }

        /// <inheritdoc />
        public bool AllowsTransitionTo(TState nextState)
        {
            return !EqualityComparer<TState>.Default.Equals(nextState, default) &&
                   allowedTransitionStates.Contains(nextState);
        }

        /// <summary>
        /// Adds one or more allowed target states to this configuration.
        /// </summary>
        /// <param name="states">The states that can be transitioned to.</param>
        public void AddAllowedTransitions(params TState[] states)
        {
            foreach (var state in states)
            {
                if (!EqualityComparer<TState>.Default.Equals(state, default))
                {
                    allowedTransitionStates.Add(state);
                }
            }
        }

        /// <summary>
        /// Sets a specific state as the automatic transition target.
        /// The state is also added to the allowed transitions list.
        /// </summary>
        /// <param name="targetState">The state to automatically transition to after execution.</param>
        public void SetAutoTransition(TState targetState)
        {
            if (EqualityComparer<TState>.Default.Equals(targetState, default))
            {
                hasAutoTransitionTarget = false;
                autoTransitionTarget = default;
                return;
            }

            allowedTransitionStates.Add(targetState);
            autoTransitionTarget = targetState;
            hasAutoTransitionTarget = true;
        }

        /// <summary>
        /// Returns a human-readable representation of this configuration.
        /// </summary>
        public override string ToString()
        {
            return $"Allowed: [{string.Join(", ", allowedTransitionStates)}], " +
                   $"Auto: {(HasAutoTransition ? AutoTransitionTarget?.ToString() : "None")}, " +
                   $"Error: {OnError?.ToString() ?? "None"}";
        }
    }
}