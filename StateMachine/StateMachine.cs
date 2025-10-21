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
    /// </summary>
    /// <typeparam name="TState">Type used for states.</typeparam>
    public class StateMachine<TState> : IStateMachine<TState>, IDisposable
    {
        /// <summary>
        /// Async lock to ensure thread-safe state transitions.
        /// </summary>
        private readonly AsyncLock asyncLock = new();

        /// <summary>
        /// Dictionary mapping states to their transition configurations.
        /// </summary>
        private readonly Dictionary<TState, IStateTransitionConfig<TState>> stateConfigs;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachine{TState}" /> class.
        /// </summary>
        /// <param name="configs">Dictionary of state configurations.</param>
        /// <param name="initialState">The initial state of the state machine.</param>
        /// <exception cref="ArgumentNullException">Thrown when configs are null.</exception>
        /// <exception cref="ArgumentException">Thrown when the initialState is not found in configs.</exception>
        public StateMachine(Dictionary<TState, IStateTransitionConfig<TState>> configs, TState initialState)
        {
            stateConfigs = configs ?? throw new ArgumentNullException(nameof(configs));
            if (!stateConfigs.ContainsKey(initialState))
            {
                throw new ArgumentException($"Initial state {initialState} not found in state configs.");
            }

            currentStateObservable.Value = initialState;
        }

        /// <summary>
        /// Disposes of the state machine and releases resources.
        /// </summary>
        public void Dispose()
        {
            stateConfigs?.Clear();
            asyncLock?.Dispose();
        }

        /// <summary>
        /// Forces the state machine into the specified state without executing any transition logic.
        /// </summary>
        /// <param name="state"></param>
        public void ForceState(TState state)
        {
            currentStateObservable.Value = state;
        }

        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        public TState CurrentState => currentStateObservable.Value;

        private readonly ObservableProperty<TState> currentStateObservable = new ObservableProperty<TState>();
        public IReadOnlyObservable<TState> CurrentStateObservable => currentStateObservable;

        /// <summary>
        /// Attempts to transition to the given state asynchronously, handling enter/exit actions, async actions, and rollback.
        /// </summary>
        /// <param name="nextState">The target state to transition to.</param>
        /// <param name="cancellationToken">Optional token to cancel the transition.</param>
        /// <returns>True if the transition succeeded, false otherwise.</returns>
        public async UniTask<bool> TransitionToAsync(TState nextState, CancellationToken cancellationToken = default)
        {
            TState nextAutoState = default;

            using (await asyncLock.LockAsync(cancellationToken))
            {
                if (!TryGetConfigs(CurrentState, nextState, out var currentConfig, out var nextConfig))
                {
                    Debug.LogWarning(
                        $"[StateMachine] ⚠️ No valid config found for transition {CurrentState} → {nextState}.");
                    return false;
                }

                if (!currentConfig.AllowsTransitionTo(CurrentState, nextState))
                {
                    Debug.LogWarning($"[StateMachine] ⚠️ Transition not allowed: {CurrentState} → {nextState}.");
                    return false;
                }

                TState originalState = CurrentState;

                try
                {
                    ExitState(currentConfig, originalState);
                    currentStateObservable.Value = nextState;
                    EnterState(nextConfig, nextState);
                    await ExecuteAsyncAction(nextConfig, cancellationToken);

                    Debug.Log($"[StateMachine] ✅ Transition completed: {originalState} → {nextState}");

                    if (nextConfig is { AutoTransition: true } && !IsDefault(nextConfig.OnSuccess))
                    {
                        nextAutoState = nextConfig.OnSuccess;
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning($"[StateMachine] ⚠️ Transition {originalState} → {nextState} canceled.");
                    await RollbackState(currentConfig, originalState, nextState);
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StateMachine] ❌ Transition failed {originalState} → {nextState}: {ex}");
                    await RollbackState(currentConfig, originalState, nextState);
                    return false;
                }
            }

            if (IsDefault(nextAutoState))
            {
                return true;
            }

            Debug.Log($"[StateMachine] 🔁 Auto-transitioning from {nextState} → {nextAutoState}");
            await TransitionToAsync(nextAutoState, cancellationToken);

            return true;
        }

        /// <summary>
        /// Attempts to retrieve configurations for the current and next states.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="next">The next state.</param>
        /// <param name="currentConfig">Output parameter for the current state's configuration.</param>
        /// <param name="nextConfig">Output parameter for the next state's configuration.</param>
        /// <returns>True if both configurations were found, false otherwise.</returns>
        private bool TryGetConfigs(TState current, TState next, out IStateTransitionConfig<TState> currentConfig,
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
            if (config is IStateTransitionConfigWithAction<TState> withAction)
            {
                withAction.OnExit?.Invoke(state);
            }
        }

        /// <summary>
        /// Executes the OnEnter action of the given state if available.
        /// </summary>
        /// <param name="config">The state configuration.</param>
        /// <param name="state">The state being entered.</param>
        private static void EnterState(IStateTransitionConfig<TState> config, TState state)
        {
            if (config is IStateTransitionConfigWithAction<TState> withAction)
            {
                withAction.OnEnter?.Invoke(state);
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
        /// </summary>
        /// <param name="currentConfig">Configuration of the original state.</param>
        /// <param name="originalState">The state before the failed transition.</param>
        /// <param name="failedState">The state that failed during transition.</param>
        private async UniTask RollbackState(IStateTransitionConfig<TState> currentConfig, TState originalState,
            TState failedState)
        {
            if (stateConfigs.TryGetValue(failedState, out var failedConfig))
            {
                ExitState(failedConfig, failedState);
            }

            TState rollbackTarget = originalState;
            if (currentConfig.OnError != null &&
                !IsDefault(currentConfig.OnError) &&
                stateConfigs.ContainsKey(currentConfig.OnError))
            {
                rollbackTarget = currentConfig.OnError;
            }

            currentStateObservable.Value = rollbackTarget;

            if (stateConfigs.TryGetValue(rollbackTarget, out var rollbackConfig))
            {
                EnterState(rollbackConfig, rollbackTarget);
            }

            await UniTask.Yield();
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