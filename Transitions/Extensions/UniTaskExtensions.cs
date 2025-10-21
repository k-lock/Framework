using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;
using Framework.Transitions.Implementations;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for UniTask integration with the transition system.
    /// </summary>
    public static class UniTaskExtensions
    {
        /// <summary>
        /// Converts a UniTask into an ITransition, allowing it to be used in the fluent transition API.
        /// This enables arbitrary async operations to be combined with transitions using And(), Or(), Then(), etc.
        /// </summary>
        /// <param name="task">The UniTask to convert.</param>
        /// <returns>A transition that completes when the UniTask completes.</returns>
        /// <example>
        /// <code>
        /// UniTask myAsyncOperation = DoSomethingAsync();
        /// await Transition.Create()
        ///     .And(myAsyncOperation.WaitForComplete())
        ///     .WithCancellation(token);
        /// </code>
        /// </example>
        public static ITransition WaitForComplete(this UniTask task)
        {
            return new UniTaskTransition(task);
        }

        /// <summary>
        /// Converts a UniTask&lt;T&gt; into an ITransition, allowing it to be used in the fluent transition API.
        /// Note: The result value is discarded in the transition system.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the UniTask.</typeparam>
        /// <param name="task">The UniTask to convert.</param>
        /// <returns>A transition that completes when the UniTask completes.</returns>
        /// <example>
        /// <code>
        /// UniTask&lt;int&gt; myAsyncOperation = CalculateScoreAsync();
        /// 
        /// // Use in transition (result is discarded)
        /// await Transition.Delay(1f)
        ///     .Then(myAsyncOperation.WaitForComplete());
        /// 
        /// // If you need the result, await directly instead:
        /// // int score = await myAsyncOperation;
        /// </code>
        /// </example>
        public static ITransition WaitForComplete<T>(this UniTask<T> task)
        {
            // Convert UniTask<T> to UniTask (discarding the result)
            return new UniTaskTransition(task.AsUniTask());
        }
    }
}