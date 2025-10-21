using System;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;
using Framework.Transitions.Implementations;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for Func&lt;UniTask&gt; integration with the transition system.
    /// Enables lazy execution of async operations within the transition pipeline.
    /// </summary>
    public static class FuncTransitionExtensions
    {
        /// <summary>
        /// Converts a function that returns a UniTask into a transition.
        /// The function is executed when the transition is awaited, enabling lazy evaluation.
        /// </summary>
        /// <param name="taskFunc">The function that returns a UniTask to execute.</param>
        /// <returns>A transition that executes the function and waits for its completion.</returns>
        /// <example>
        /// <code>
        /// // Lazy execution - the function is only called when the transition runs
        /// Func-UniTask- loadData = async () => {
        ///     await LoadDataFromServerAsync();
        /// };
        /// 
        /// await Transition.Delay(1f)
        ///     .Then(loadData.WaitForExecution());
        /// 
        /// // The function is executed at this point, not when WaitForExecution was called
        /// </code>
        /// </example>
        public static ITransition WaitForExecution(this Func<UniTask> taskFunc)
        {
            return new LazyUniTaskTransition(taskFunc);
        }

        /// <summary>
        /// Converts a function that returns a UniTask T into a transition.
        /// The function is executed when the transition is awaited, enabling lazy evaluation.
        /// Note: The result value is discarded in the transition system.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the UniTask.</typeparam>
        /// <param name="taskFunc">The function that returns a UniTask T  to execute.</param>
        /// <returns>A transition that executes the function and waits for its completion.</returns>
        /// <example>
        /// <code>
        /// // Lazy execution with a result (result is discarded in transition)
        /// Func-UniTask-int- calculateScore = async () => {
        ///     await UniTask.Delay(100);
        ///     return 42;
        /// };
        /// 
        /// await Transition.Delay(0.5f)
        ///     .Then(calculateScore.WaitForExecution());
        /// 
        /// // If you need the result, use await directly instead:
        /// // int score = await calculateScore();
        /// </code>
        /// </example>
        public static ITransition WaitForExecution<T>(this Func<UniTask<T>> taskFunc)
        {
            return new LazyUniTaskTransition<T>(taskFunc);
        }
    }
}