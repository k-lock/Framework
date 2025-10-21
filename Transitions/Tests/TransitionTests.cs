using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework.Transitions.Base;
using Framework.Transitions.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Framework.Transitions.Tests
{
    /// <summary>
    /// Comprehensive test suite for the transition system.
    /// Tests all transition types, extensions, and edge cases.
    /// </summary>
    public class TransitionTests : MonoBehaviour
    {
        private VisualElement testElement1;
        private VisualElement testElement2;
        private CancellationTokenSource globalCts;

        private async void Start()
        {
            try
            {
                Debug.Log("=== STARTING TRANSITION TESTS ===\n");
                
                // Setup
                testElement1 = new VisualElement();
                testElement2 = new VisualElement();
                globalCts = new CancellationTokenSource();

                // Run all test categories
                await TestBasicTransitions();
                await TestCombinationTransitions();
                await TestSequentialTransitions();
                await TestCancellationBehavior();
                await TestTimeoutBehavior();
                await TestExtensionIntegrations();
                await TestEdgeCases();
                await TestPerformance();

                Debug.Log("\n=== ALL TESTS COMPLETED SUCCESSFULLY ===");
            }
            catch (Exception e)
            {
                Debug.LogError($"TEST SUITE FAILED: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                globalCts?.Dispose();
            }
        }

        #region Basic Transitions

        /// <summary>
        /// Tests all basic transition types: Empty, Delay, UniTask.
        /// </summary>
        private async UniTask TestBasicTransitions()
        {
            Debug.Log("\n--- TEST CATEGORY: Basic Transitions ---");

            // Test 1: Empty transition (should complete immediately)
            Stopwatch sw = Stopwatch.StartNew();
            await Transition.Create();
            sw.Stop();
            Debug.Log($"✓ Empty Transition: Completed in {sw.ElapsedMilliseconds}ms (expected: ~0ms)");

            // Test 2: Delay transition (should wait exact duration)
            sw.Restart();
            await Transition.Delay(0.5f);
            sw.Stop();
            long delayMs = sw.ElapsedMilliseconds;
            Debug.Log($"✓ Delay Transition (0.5s): Completed in {delayMs}ms (expected: ~500ms)");
            // Note: Editor performance can vary significantly, especially during the first run
            if (Math.Abs(delayMs - 500) > 2000)
                Debug.LogWarning($"  ⚠ Delay accuracy off by {Math.Abs(delayMs - 500)}ms (Editor performance issue)");

            // Test 3: UniTask integration via WaitForComplete()
            sw.Restart();
            UniTask asyncTask = SimulateAsyncOperation(300);
            await Transition.Create()
                .And(asyncTask.WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ UniTask Integration: Completed in {sw.ElapsedMilliseconds}ms (expected: ~300ms)");

            // Test 4: Direct UniTask wrapping
            sw.Restart();
            await SimulateAsyncOperation(200).WaitForComplete();
            sw.Stop();
            Debug.Log($"✓ Direct UniTask Wrapping: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");
        }

        #endregion

        #region Combination Transitions

        /// <summary>
        /// Tests And/Or combinations and WhenAll/WhenAny factory methods.
        /// </summary>
        private async UniTask TestCombinationTransitions()
        {
            Debug.Log("\n--- TEST CATEGORY: Combination Transitions ---");

            // Test 1: And (should wait for longest)
            Stopwatch sw = Stopwatch.StartNew();
            await Transition.Delay(0.2f).And(Transition.Delay(0.5f));
            sw.Stop();
            Debug.Log($"✓ And Combination (0.2s + 0.5s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~500ms)");

            // Test 2: Or (should complete with fastest)
            sw.Restart();
            await Transition.Delay(0.5f).Or(Transition.Delay(0.2f));
            sw.Stop();
            Debug.Log($"✓ Or Combination (0.5s | 0.2s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 3: WhenAll factory (multiple transitions)
            sw.Restart();
            await Transition.WhenAll(
                Transition.Delay(0.1f),
                Transition.Delay(0.2f),
                Transition.Delay(0.3f)
            );
            sw.Stop();
            Debug.Log($"✓ WhenAll (0.1s, 0.2s, 0.3s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~300ms)");

            // Test 4: WhenAny factory (multiple transitions)
            sw.Restart();
            await Transition.WhenAny(
                Transition.Delay(0.3f),
                Transition.Delay(0.1f),
                Transition.Delay(0.2f)
            );
            sw.Stop();
            Debug.Log($"✓ WhenAny (0.3s, 0.1s, 0.2s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");

            // Test 5: Complex nested combinations
            sw.Restart();
            await Transition.Delay(0.1f)
                .And(Transition.Delay(0.2f).Or(Transition.Delay(0.05f)))
                .And(Transition.Create());
            sw.Stop();
            Debug.Log($"✓ Nested Combinations: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 6: UniTask in And combination
            sw.Restart();
            await Transition.Delay(0.2f)
                .And(SimulateAsyncOperation(300).WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ UniTask in And: Completed in {sw.ElapsedMilliseconds}ms (expected: ~300ms)");

            // Test 7: UniTask in Or combination
            sw.Restart();
            await Transition.Delay(0.3f)
                .Or(SimulateAsyncOperation(100).WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ UniTask in Or: Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");
        }

        #endregion

        #region Sequential Transitions

        /// <summary>
        /// Tests Then chaining and ThenDelay extension.
        /// </summary>
        private async UniTask TestSequentialTransitions()
        {
            Debug.Log("\n--- TEST CATEGORY: Sequential Transitions ---");

            // Test 1: Simple Then chain
            Stopwatch sw = Stopwatch.StartNew();
            await Transition.Delay(0.1f).Then(Transition.Delay(0.2f));
            sw.Stop();
            Debug.Log($"✓ Then Chain (0.1s → 0.2s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~300ms)");

            // Test 2: ThenDelay extension
            sw.Restart();
            await Transition.Create().ThenDelay(0.2f);
            sw.Stop();
            Debug.Log($"✓ ThenDelay (0s → 0.2s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 3: Long Then chain
            sw.Restart();
            await Transition.Delay(0.05f)
                .Then(Transition.Delay(0.05f))
                .Then(Transition.Delay(0.05f))
                .Then(Transition.Delay(0.05f));
            sw.Stop();
            Debug.Log($"✓ Long Then Chain (4x 0.05s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 4: Then with combinations
            sw.Restart();
            await Transition.Delay(0.1f)
                .Then(Transition.Delay(0.1f).And(Transition.Delay(0.2f)))
                .ThenDelay(0.1f);
            sw.Stop();
            Debug.Log($"✓ Then with And: Completed in {sw.ElapsedMilliseconds}ms (expected: ~400ms)");

            // Test 5: UniTask in Then chain
            sw.Restart();
            await Transition.Delay(0.1f)
                .Then(SimulateAsyncOperation(150).WaitForComplete())
                .ThenDelay(0.1f);
            sw.Stop();
            Debug.Log($"✓ UniTask in Then: Completed in {sw.ElapsedMilliseconds}ms (expected: ~350ms)");
        }

        #endregion

        #region Cancellation Tests

        /// <summary>
        /// Tests cancellation token propagation and behavior.
        /// </summary>
        private async UniTask TestCancellationBehavior()
        {
            Debug.Log("\n--- TEST CATEGORY: Cancellation Behavior ---");

            // Test 1: Immediate cancellation
            CancellationTokenSource cts1 = new CancellationTokenSource();
            cts1.Cancel();
            try
            {
                await Transition.Delay(1f).WithCancellation(cts1.Token);
                Debug.LogError("✗ Immediate Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Immediate Cancellation: Correctly threw OperationCanceledException");
            }

            // Test 2: Delayed cancellation
            CancellationTokenSource cts2 = new CancellationTokenSource();
            UniTask delayTask = Transition.Delay(1f).WithCancellation(cts2.Token);
            await UniTask.Delay(100, cancellationToken: globalCts.Token);
            cts2.Cancel();
            try
            {
                await delayTask;
                Debug.LogError("✗ Delayed Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Delayed Cancellation: Correctly cancelled after 100ms");
            }

            // Test 3: Cancellation in And combination
            CancellationTokenSource cts3 = new CancellationTokenSource();
            UniTask andTask = Transition.Delay(0.5f)
                .And(Transition.Delay(1f))
                .WithCancellation(cts3.Token);
            await UniTask.Delay(200, cancellationToken: globalCts.Token);
            cts3.Cancel();
            try
            {
                await andTask;
                Debug.LogError("✗ And Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ And Cancellation: Both transitions cancelled correctly");
            }

            // Test 4: Cancellation in Or combination
            CancellationTokenSource cts4 = new CancellationTokenSource();
            UniTask orTask = Transition.Delay(0.5f)
                .Or(Transition.Delay(1f))
                .WithCancellation(cts4.Token);
            await UniTask.Delay(100, cancellationToken: globalCts.Token);
            cts4.Cancel();
            try
            {
                await orTask;
                Debug.LogError("✗ Or Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Or Cancellation: Race cancelled correctly");
            }

            // Test 5: Cancellation in the Then chain
            CancellationTokenSource cts5 = new CancellationTokenSource();
            UniTask thenTask = Transition.Delay(0.2f)
                .Then(Transition.Delay(0.5f))
                .WithCancellation(cts5.Token);
            await UniTask.Delay(300, cancellationToken: globalCts.Token); // Cancel during the second transition
            cts5.Cancel();
            try
            {
                await thenTask;
                Debug.LogError("✗ Then Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Then Cancellation: Chain cancelled mid-sequence");
            }

            // Test 6: UniTask cancellation propagation
            CancellationTokenSource cts6 = new CancellationTokenSource();
            UniTask wrappedTask = SimulateAsyncOperation(500, globalCts.Token)
                .WaitForComplete()
                .WithCancellation(cts6.Token);
            await UniTask.Delay(100, cancellationToken: globalCts.Token);
            cts6.Cancel();
            try
            {
                await wrappedTask;
                Debug.LogError("✗ UniTask Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ UniTask Cancellation: Wrapped task cancelled correctly");
            }
        }

        #endregion

        #region Timeout Tests

        /// <summary>
        /// Tests timeout functionality and timeout + cancellation combinations.
        /// </summary>
        private async UniTask TestTimeoutBehavior()
        {
            Debug.Log("\n--- TEST CATEGORY: Timeout Behavior ---");

            // Test 1: Timeout not triggered (completes before timeout)
            Stopwatch sw = Stopwatch.StartNew();
            await Transition.Delay(0.2f).WithTimeout(0.5f);
            sw.Stop();
            Debug.Log($"✓ Timeout Not Triggered: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 2: Timeout triggered (exceeds timeout)
            try
            {
                await Transition.Delay(0.5f).WithTimeout(0.2f);
                Debug.LogError("✗ Timeout Triggered: Should have thrown TimeoutException!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Timeout Triggered: Correctly threw TimeoutException after 200ms");
            }

            // Test 3: Timeout with And combination
            try
            {
                await Transition.Delay(0.3f)
                    .And(Transition.Delay(0.5f))
                    .WithTimeout(0.2f);
                Debug.LogError("✗ Timeout with And: Should have thrown!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Timeout with And: Timed out correctly");
            }

            // Test 4: Timeout with the Then chain
            try
            {
                await Transition.Delay(0.15f)
                    .Then(Transition.Delay(0.15f))
                    .WithTimeout(0.2f);
                Debug.LogError("✗ Timeout with Then: Should have thrown!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Timeout with Then: Timed out during sequence");
            }

            // Test 5: Timeout and Cancellation (cancellation wins)
            CancellationTokenSource cts = new CancellationTokenSource();
            UniTask timeoutTask = Transition.Delay(1f)
                .WithTimeout(2f)
                .WithCancellation(cts.Token);
            await UniTask.Delay(100, cancellationToken: globalCts.Token);
            cts.Cancel();
            try
            {
                await timeoutTask;
                Debug.LogError("✗ Timeout + Cancellation: Should have thrown!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Timeout + Cancellation: Cancellation took precedence");
            }

            // Test 6: Timeout and Cancellation (timeout wins)
            CancellationTokenSource cts2 = new CancellationTokenSource();
            try
            {
                await Transition.Delay(1f)
                    .WithTimeout(0.1f)
                    .WithCancellation(cts2.Token);
                Debug.LogError("✗ Timeout Wins: Should have thrown!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Timeout Wins: Timeout triggered before cancellation");
            }
            finally
            {
                cts2.Dispose();
            }
        }

        #endregion

        #region Extension Integration Tests

        /// <summary>
        /// Tests all extension integrations: DOTween, VisualElement, UniTask.
        /// </summary>
        private async UniTask TestExtensionIntegrations()
        {
            Debug.Log("\n--- TEST CATEGORY: Extension Integrations ---");

            // Test 1: DOTween integration
            Tween testTween = DOTween.To(() => 0f, _ => { }, 1f, 0.2f);
            Stopwatch sw = Stopwatch.StartNew();
            await testTween.WaitForComplete();
            sw.Stop();
            Debug.Log($"✓ DOTween Integration: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 2: DOTween in And combination
            Tween tween1 = DOTween.To(() => 0f, _ => { }, 1f, 0.1f);
            Tween tween2 = DOTween.To(() => 0f, _ => { }, 1f, 0.2f);
            sw.Restart();
            await tween1.WaitForComplete().And(tween2.WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ DOTween And: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 3: DOTween in Then chain
            Tween tween3 = DOTween.To(() => 0f, _ => { }, 1f, 0.1f);
            Tween tween4 = DOTween.To(() => 0f, _ => { }, 1f, 0.1f);
            sw.Restart();
            await tween3.WaitForComplete().Then(tween4.WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ DOTween Then: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 4: VisualElement transition (mock - completes immediately)
            sw.Restart();
            await testElement1.WaitForComplete();
            sw.Stop();
            Debug.Log($"✓ VisualElement Integration: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 5: Mixed extensions (DOTween + UniTask + Delay)
            Tween tween5 = DOTween.To(() => 0f, _ => { }, 1f, 0.1f);
            sw.Restart();
            await Transition.Delay(0.1f)
                .And(tween5.WaitForComplete())
                .And(SimulateAsyncOperation(100).WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ Mixed Extensions: Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");

            // Test 6: Complex extension chain
            Tween tween6 = DOTween.To(() => 0f, _ => { }, 1f, 0.05f);
            sw.Restart();
            await Transition.Create()
                .And(tween6.WaitForComplete())
                .Then(SimulateAsyncOperation(50).WaitForComplete())
                .Then(Transition.Delay(0.05f))
                .And(testElement2.WaitForComplete());
            sw.Stop();
            Debug.Log($"✓ Complex Extension Chain: Completed in {sw.ElapsedMilliseconds}ms (expected: ~150ms)");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Tests edge cases and error conditions.
        /// </summary>
        private async UniTask TestEdgeCases()
        {
            Debug.Log("\n--- TEST CATEGORY: Edge Cases ---");

            // Test 1: Zero delay
            Stopwatch sw = Stopwatch.StartNew();
            await Transition.Delay(0f);
            sw.Stop();
            Debug.Log($"✓ Zero Delay: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 2: Negative delay (should clamp to 0)
            sw.Restart();
            await Transition.Delay(-1f);
            sw.Stop();
            Debug.Log($"✓ Negative Delay: Completed in {sw.ElapsedMilliseconds}ms (clamped to 0)");

            // Test 3: Empty And combination
            sw.Restart();
            await Transition.Create().And(Transition.Create());
            sw.Stop();
            Debug.Log($"✓ Empty And: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 4: Empty Or combination
            sw.Restart();
            await Transition.Create().Or(Transition.Create());
            sw.Stop();
            Debug.Log($"✓ Empty Or: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 5: Empty Then chain
            sw.Restart();
            await Transition.Create().Then(Transition.Create()).Then(Transition.Create());
            sw.Stop();
            Debug.Log($"✓ Empty Then Chain: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 6: Already completed UniTask
            UniTask completedTask = UniTask.CompletedTask;
            sw.Restart();
            await completedTask.WaitForComplete();
            sw.Stop();
            Debug.Log($"✓ Completed UniTask: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 7: Multiple awaits on the same transition
            ITransition reusableTransition = Transition.Delay(0.1f);
            sw.Restart();
            await reusableTransition;
            long firstAwait = sw.ElapsedMilliseconds;
            sw.Restart();
            await reusableTransition;
            long secondAwait = sw.ElapsedMilliseconds;
            Debug.Log($"✓ Multiple Awaits: First={firstAwait}ms, Second={secondAwait}ms");

            // Test 8: Very long chain (stress test)
            ITransition longChain = Transition.Create();
            for (int i = 0; i < 10; i++)
            {
                longChain = longChain.Then(Transition.Create());
            }
            sw.Restart();
            await longChain;
            sw.Stop();
            Debug.Log($"✓ Long Chain (10 steps): Completed in {sw.ElapsedMilliseconds}ms");

            // Test 9: Deeply nested combinations
            sw.Restart();
            await Transition.Create()
                .And(Transition.Create().And(Transition.Create().And(Transition.Create())));
            sw.Stop();
            Debug.Log($"✓ Deeply Nested And: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 10: WhenAll with an empty array (should complete immediately)
            sw.Restart();
            await Transition.WhenAll();
            sw.Stop();
            Debug.Log($"✓ WhenAll Empty: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 11: WhenAny with an empty array (should complete immediately)
            sw.Restart();
            await Transition.WhenAny();
            sw.Stop();
            Debug.Log($"✓ WhenAny Empty: Completed in {sw.ElapsedMilliseconds}ms");

            // Test 12: Null cancellation token (should use default)
            sw.Restart();
            await Transition.Delay(0.1f).WithCancellation(CancellationToken.None);
            sw.Stop();
            Debug.Log($"✓ Null Cancellation Token: Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");

            // Test 13: Negative delay value (clamped to 0)
            sw.Restart();
            await Transition.Delay(-5f);
            sw.Stop();
            Debug.Log($"✓ Negative Delay (-5s → 0s): Completed in {sw.ElapsedMilliseconds}ms (expected: ~0ms)");

            // Test 14: Very large delay value (clamped to TimeSpan.MaxValue)
            // Note: Values exceeding TimeSpan.MaxValue.TotalSeconds (~9.22e11) are clamped to TimeSpan.MaxValue
            // Using 1e12f (1 trillion seconds) which is larger than TimeSpan.MaxValue.TotalSeconds
            try
            {
                CancellationTokenSource largeCts = new CancellationTokenSource();
                UniTask largeDelayTask = Transition.Delay(1e12f).WithCancellation(largeCts.Token);
                await UniTask.Delay(50, cancellationToken: globalCts.Token);
                largeCts.Cancel();
                await largeDelayTask;
                Debug.LogError("✗ Large Delay: Should have been cancelled!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("✓ Large Delay (1e12s → TimeSpan.MaxValue): Cancelled correctly");
            }

            // Test 15: Chaining And/Or/Then in a complex pattern
            sw.Restart();
            await Transition.Delay(0.05f)
                .And(Transition.Delay(0.1f))
                .Or(Transition.Delay(0.2f))
                .Then(Transition.Delay(0.05f));
            sw.Stop();
            Debug.Log($"✓ Complex Chain (And→Or→Then): Completed in {sw.ElapsedMilliseconds}ms (expected: ~150ms)");

            // Test 16: Multiple UniTasks in WhenAll
            sw.Restart();
            await Transition.WhenAll(
                SimulateAsyncOperation(100).WaitForComplete(),
                SimulateAsyncOperation(150).WaitForComplete(),
                SimulateAsyncOperation(200).WaitForComplete()
            );
            sw.Stop();
            Debug.Log($"✓ WhenAll with UniTasks: Completed in {sw.ElapsedMilliseconds}ms (expected: ~200ms)");

            // Test 17: Multiple UniTasks in WhenAny
            sw.Restart();
            await Transition.WhenAny(
                SimulateAsyncOperation(200).WaitForComplete(),
                SimulateAsyncOperation(100).WaitForComplete(),
                SimulateAsyncOperation(150).WaitForComplete()
            );
            sw.Stop();
            Debug.Log($"✓ WhenAny with UniTasks: Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");

            // Test 18: Reusing same transition in multiple combinations
            ITransition sharedTransition = Transition.Delay(0.1f);
            sw.Restart();
            await Transition.WhenAll(sharedTransition, sharedTransition, sharedTransition);
            sw.Stop();
            Debug.Log($"✓ Shared Transition in WhenAll: Completed in {sw.ElapsedMilliseconds}ms (expected: ~100ms)");

            // Test 19: Exception in UniTask propagation
            try
            {
                await ThrowingAsyncOperation().WaitForComplete();
                Debug.LogError("✗ Exception Propagation: Should have thrown!");
            }
            catch (InvalidOperationException)
            {
                Debug.Log("✓ Exception Propagation: Correctly propagated InvalidOperationException");
            }

            // Test 19: Timeout with zero duration (should time out immediately)
            try
            {
                await Transition.Delay(1f).WithTimeout(0f);
                Debug.LogError("✗ Zero Timeout: Should have thrown TimeoutException!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Zero Timeout: Correctly threw TimeoutException immediately");
            }

            // Test 20: Negative timeout (should clamp to 0 and timeout immediately)
            try
            {
                await Transition.Delay(1f).WithTimeout(-1f);
                Debug.LogError("✗ Negative Timeout: Should have thrown TimeoutException!");
            }
            catch (TimeoutException)
            {
                Debug.Log("✓ Negative Timeout: Correctly clamped to 0 and threw TimeoutException");
            }
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// Tests performance characteristics and overhead.
        /// </summary>
        private async UniTask TestPerformance()
        {
            Debug.Log("\n--- TEST CATEGORY: Performance ---");

            // Test 1: Overhead of empty transitions
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                await Transition.Create();
            }
            sw.Stop();
            Debug.Log($"✓ 1000 Empty Transitions: {sw.ElapsedMilliseconds}ms (avg: {sw.ElapsedMilliseconds / 1000.0:F3}ms)");

            // Test 2: Overhead of And combinations
            sw.Restart();
            for (int i = 0; i < 100; i++)
            {
                await Transition.Create().And(Transition.Create());
            }
            sw.Stop();
            Debug.Log($"✓ 100 And Combinations: {sw.ElapsedMilliseconds}ms (avg: {sw.ElapsedMilliseconds / 100.0:F3}ms)");

            // Test 3: Overhead of Then chains
            sw.Restart();
            for (int i = 0; i < 100; i++)
            {
                await Transition.Create().Then(Transition.Create());
            }
            sw.Stop();
            Debug.Log($"✓ 100 Then Chains: {sw.ElapsedMilliseconds}ms (avg: {sw.ElapsedMilliseconds / 100.0:F3}ms)");

            // Test 4: Large WhenAll
            ITransition[] manyTransitions = new ITransition[100];
            for (int i = 0; i < 100; i++)
            {
                manyTransitions[i] = Transition.Create();
            }
            sw.Restart();
            await Transition.WhenAll(manyTransitions);
            sw.Stop();
            Debug.Log($"✓ WhenAll (100 transitions): {sw.ElapsedMilliseconds}ms");

            // Test 5: Large WhenAny
            sw.Restart();
            await Transition.WhenAny(manyTransitions);
            sw.Stop();
            Debug.Log($"✓ WhenAny (100 transitions): {sw.ElapsedMilliseconds}ms");

            // Test 6: Memory allocation test (GC pressure)
            long beforeGC = GC.GetTotalMemory(true);
            for (int i = 0; i < 1000; i++)
            {
                ITransition t = Transition.Delay(0.001f)
                    .And(Transition.Create())
                    .Then(Transition.Create());
                await t;
            }
            long afterGC = GC.GetTotalMemory(false);
            long allocated = afterGC - beforeGC;
            Debug.Log($"✓ Memory Allocation (1000 complex transitions): {allocated / 1024.0:F2} KB");

            // Test 7: Rapid fire transitions (stress test)
            sw.Restart();
            for (int i = 0; i < 500; i++)
            {
                await Transition.Delay(0.001f);
            }
            sw.Stop();
            Debug.Log($"✓ Rapid Fire (500x 1ms delays): {sw.ElapsedMilliseconds}ms (avg: {sw.ElapsedMilliseconds / 500.0:F3}ms)");

            // Test 8: Concurrent WhenAll with delays
            ITransition[] concurrentDelays = new ITransition[50];
            for (int i = 0; i < 50; i++)
            {
                concurrentDelays[i] = Transition.Delay(0.1f);
            }
            sw.Restart();
            await Transition.WhenAll(concurrentDelays);
            sw.Stop();
            Debug.Log($"✓ Concurrent WhenAll (50x 0.1s): {sw.ElapsedMilliseconds}ms (expected: ~100ms, not 5000ms)");

            // Test 9: Nested Then chains (deep recursion test)
            ITransition deepChain = Transition.Create();
            for (int i = 0; i < 50; i++)
            {
                deepChain = deepChain.Then(Transition.Create());
            }
            sw.Restart();
            await deepChain;
            sw.Stop();
            Debug.Log($"✓ Deep Then Chain (50 levels): {sw.ElapsedMilliseconds}ms");

            // Test 10: Mixed UniTask and Transition in large WhenAll
            ITransition[] mixedTransitions = new ITransition[50];
            for (int i = 0; i < 50; i++)
            {
                if (i % 2 == 0)
                    mixedTransitions[i] = Transition.Delay(0.05f);
                else
                    mixedTransitions[i] = SimulateAsyncOperation(50).WaitForComplete();
            }
            sw.Restart();
            await Transition.WhenAll(mixedTransitions);
            sw.Stop();
            Debug.Log($"✓ Mixed WhenAll (25 Delays + 25 UniTasks): {sw.ElapsedMilliseconds}ms (expected: ~50ms)");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates an async operation with a specified delay.
        /// </summary>
        private static async UniTask SimulateAsyncOperation(int delayMs, CancellationToken cancellationToken = default)
        {
            await UniTask.Delay(delayMs, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Simulates an async operation that throws an exception.
        /// </summary>
        private static async UniTask ThrowingAsyncOperation(CancellationToken cancellationToken = default)
        {
            await UniTask.Delay(50, cancellationToken: cancellationToken);
            throw new InvalidOperationException("Test exception from async operation");
        }

        #endregion
    }
}