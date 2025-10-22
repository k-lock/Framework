using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Observable;
using Framework.Utils.AsyncLock;
using UnityEngine;

namespace Framework.StateMachine
{
    /// <summary>
    /// Core implementation of an asynchronous state machine with rollback and lifecycle events.
    /// Supports asynchronous transitions, automatic state transitions, and rollback on failure.
    /// </summary>
    /// <typeparam name="TState">Type used for states.</typeparam>
    public class StateMachine<TState> : IStateMachine<TState>, IDisposable
    {
        /// <summary>
        /// Async lock to ensure thread-safe state transitions.
        /// </summary>
        private readonly AsyncLock asyncLock = new();

        private readonly ObservableProperty<TState> currentStateObservable = new();

        /// <summary>
        /// Dictionary mapping states to their transition configurations.
        /// </summary>
        private readonly Dictionary<TState, IStateTransitionConfig<TState>> stateConfigs;

        /// <summary>
        /// Dispose helper flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState}" /> class.
        /// </summary>
        /// <param name="configs">Dictionary of state configurations.</param>
        /// <param name="initialState">The initial state of the state machine.</param>
        /// <exception cref="ArgumentNullException">Thrown when configs are null.</exception>
        /// <exception cref="ArgumentException">Thrown when the initialState is not found in configs or is default.</exception>
        public StateMachine(Dictionary<TState, IStateTransitionConfig<TState>> configs, TState initialState)
        {
            stateConfigs = configs ?? throw new ArgumentNullException(nameof(configs));
            if (!stateConfigs.ContainsKey(initialState))
            {
                throw new ArgumentException($"Initial state {initialState} not found in state configs.");
            }

            if (IsDefault(initialState))
            {
                throw new ArgumentException("Initial state must not be the default value.", nameof(initialState));
            }

            currentStateObservable.Value = initialState;
        }

        public IReadOnlyObservable<TState> CurrentStateObservable => currentStateObservable;

        /// <summary>
        /// Disposes of the state machine and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            stateConfigs?.Clear();
            asyncLock?.Dispose();
            currentStateObservable.Value = default;
            disposed = true;
        }

        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        public TState CurrentState => currentStateObservable.Value;

        /// <summary>
        /// Attempts to transition to the given state asynchronously.
        /// Handles enter/exit actions, executes async actions, and performs rollback on failure.
        /// </summary>
        /// <param name="nextState">The target state to transition to.</param>
        /// <param name="cancellationToken">Optional token to cancel the transition.</param>
        /// <param name="visitedStates">
        /// A set of states already visited during the current auto-transition chain.
        /// Used to detect and prevent cyclic auto-transitions.
        /// </param>
        /// <returns>True if the transition succeeded; false otherwise.</returns>
        public async UniTask<bool> TransitionToAsync(TState nextState, CancellationToken cancellationToken = default,
            HashSet<TState> visitedStates = null)
        {
            visitedStates ??= new HashSet<TState>();

            if (!visitedStates.Add(nextState))
            {
                Debug.LogWarning(
                    $"[StateMachine] ⚠️ Cyclic transition detected: {string.Join(" → ", visitedStates)} → {nextState}");
                return false;
            }

            try
            {
                IStateTransitionConfig<TState> nextConfig;

                using (await asyncLock.LockAsync(cancellationToken))
                {
                    if (!TryGetConfigs(CurrentState, nextState, out var currentConfig, out nextConfig))
                    {
                        Debug.LogWarning(
                            $"[StateMachine] ⚠️ No valid config found for transition {CurrentState} → {nextState}.");
                        return false;
                    }

                    if (!currentConfig.AllowsTransitionTo(nextState))
                    {
                        Debug.LogWarning($"[StateMachine] ⚠️ Transition not allowed: {CurrentState} → {nextState}.");
                        return false;
                    }

                    TState originalState = CurrentState;

                    try
                    {
                        // Exit current state
                        ExitState(currentConfig, originalState);

                        // Change state
                        currentStateObservable.Value = nextState;

                        // Enter next state
                        EnterState(nextConfig, nextState);

                        // Run optional async task
                        await ExecuteAsyncAction(nextConfig, cancellationToken);

                        Debug.Log($"[StateMachine] ✅ Transition completed: {originalState} → {nextState}");
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.LogWarning($"[StateMachine] ⚠️ Transition {originalState} → {nextState} canceled.");
                        RollbackState(currentConfig, originalState, nextState);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[StateMachine] ❌ Transition failed {originalState} → {nextState}: {ex}");
                        RollbackState(currentConfig, originalState, nextState);
                        return false;
                    }
                }

                // Auto-Transition
                return await HandleAutoTransition(nextState, nextConfig, cancellationToken, visitedStates);
            }
            finally
            {
                visitedStates.Remove(nextState);
            }
        }

        /// <summary>
        /// Forces the state machine into the specified state without executing any transition logic.
        /// </summary>
        /// <param name="state">The state to force the machine into.</param>
        /// <exception cref="ArgumentException">Thrown when the state is default.</exception>
        public void ForceState(TState state)
        {
            if (IsDefault(state))
            {
                throw new ArgumentException("Cannot force state to default value.", nameof(state));
            }

            if (!stateConfigs.ContainsKey(state))
            {
                Debug.LogWarning($"[StateMachine] ⚠️ Forcing to unconfigured state: {state}");
            }

            currentStateObservable.Value = state;
        }

        /// <summary>
        /// Attempts an automatic transition for the given state if an auto-transition is configured.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <param name="config">The configuration of the current state.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <param name="visitedStates">
        /// A set of states already visited during the current auto-transition chain.
        /// Used to detect and prevent cyclic auto-transitions.
        /// </param>
        /// <returns>True if the auto-transition succeeded or no auto-transition exists; false if a transition failed.</returns>
        private async UniTask<bool> HandleAutoTransition(TState state, IStateTransitionConfig<TState> config,
            CancellationToken cancellationToken,
            HashSet<TState> visitedStates)
        {
            if (config is not { HasAutoTransition: true })
            {
                return true;
            }

            TState nextAutoState = config.HasAutoTransition ? config.AutoTransitionTarget : default;

            if (IsDefault(nextAutoState))
            {
                return true;
            }

            Debug.Log($"[StateMachine] 🔁 Auto-Transition triggered: {state} → {nextAutoState}");
            return await TransitionToAsync(nextAutoState, cancellationToken, visitedStates);
        }

        /// <summary>
        /// Attempts to retrieve configurations for the current and next states.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="next">The next state.</param>
        /// <param name="currentConfig">Output parameter for the current state's configuration.</param>
        /// <param name="nextConfig">Output parameter for the next state's configuration.</param>
        /// <returns>True if both configurations were found; false otherwise.</returns>
        private bool TryGetConfigs(TState current, TState next,
            out IStateTransitionConfig<TState> currentConfig,
            out IStateTransitionConfig<TState> nextConfig)
        {
            currentConfig = null!;
            nextConfig = null!;

            if (!stateConfigs.TryGetValue(current, out currentConfig))
            {
                Debug.LogWarning($"[StateMachine] Config for current state {current} not found.");
                return false;
            }

            if (stateConfigs.TryGetValue(next, out nextConfig))
            {
                return true;
            }

            Debug.LogWarning($"[StateMachine] Config for next state {next} not found.");
            return false;
        }

        /// <summary>
        /// Executes the OnExit action of the given state if available.
        /// </summary>
        /// <param name="config">The state configuration.</param>
        /// <param name="state">The state being exited.</param>
        private static void ExitState(IStateTransitionConfig<TState> config, TState state)
        {
            if (config is IStateTransitionConfigWithAction<TState> { OnExit: not null } withAction)
            {
                withAction.OnExit.Invoke(state);
            }
        }

        /// <summary>
        /// Executes the OnEnter action of the given state if available.
        /// </summary>
        /// <param name="config">The state configuration.</param>
        /// <param name="state">The state being entered.</param>
        private static void EnterState(IStateTransitionConfig<TState> config, TState state)
        {
            if (config is IStateTransitionConfigWithAction<TState> { OnEnter: not null } withAction)
            {
                withAction.OnEnter.Invoke(state);
            }
        }

        /// <summary>
        /// Executes the asynchronous action of a state if available, respecting the cancellation token.
        /// </summary>
        /// <param name="config">The state configuration.</param>
        /// <param name="cancellationToken">Token to cancel the async action.</param>
        private static async UniTask ExecuteAsyncAction(IStateTransitionConfig<TState> config,
            CancellationToken cancellationToken = default)
        {
            if (config is IStateTransitionConfigWithAction<TState> { AsyncAction: not null } withAction)
            {
                await withAction.AsyncAction.Invoke().AttachExternalCancellation(cancellationToken);
            }
        }

        /// <summary>
        /// Rolls back the state machine to the original state or the OnError state if configured and valid.
        /// Executes OnExit of the failed state and OnEnter of the rollback state.
        /// AsyncAction is not executed during rollback to avoid long-running tasks blocking recovery.
        /// </summary>
        /// <param name="currentConfig">Configuration of the original state.</param>
        /// <param name="originalState">The state before the failed transition.</param>
        /// <param name="failedState">The state that failed during transition.</param>
        private void RollbackState(IStateTransitionConfig<TState> currentConfig, TState originalState,
            TState failedState)
        {
            if (stateConfigs.TryGetValue(failedState, out var failedConfig))
            {
                ExitState(failedConfig, failedState);
            }

            TState rollbackTarget = originalState;
            if (currentConfig.OnError != null && !IsDefault(currentConfig.OnError) &&
                stateConfigs.ContainsKey(currentConfig.OnError))
            {
                rollbackTarget = currentConfig.OnError;
            }

            currentStateObservable.Value = rollbackTarget;

            if (stateConfigs.TryGetValue(rollbackTarget, out var rollbackConfig))
            {
                EnterState(rollbackConfig, rollbackTarget);
            }
        }

        /// <summary>
        /// Helper method to check if a value equals its default value.
        /// </summary>
        private static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }
    }
}