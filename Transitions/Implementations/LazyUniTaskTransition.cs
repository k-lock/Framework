using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that lazily executes a function returning a UniTask.
    /// The function is only invoked when WaitAsync is called, not when the transition is created.
    /// </summary>
    public class LazyUniTaskTransition : TransitionBase
    {
        private readonly Func<UniTask> taskFunc;

        /// <summary>
        /// Creates a new lazy UniTask transition.
        /// </summary>
        /// <param name="taskFunc">The function to execute lazily. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if taskFunc is null.</exception>
        public LazyUniTaskTransition(Func<UniTask> taskFunc)
        {
            this.taskFunc = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
        }

        /// <summary>
        /// Executes the function and waits for the resulting UniTask to complete.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A UniTask that completes when the function's task completes.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            // Invoke the function NOW (lazy execution)
            UniTask task = taskFunc();
            
            // Wait for the task to complete
            await task.AttachExternalCancellation(cancellationToken);
        }
    }

    /// <summary>
    /// A transition that lazily executes a function returning a UniTask-T-.
    /// The function is only invoked when WaitAsync is called, not when the transition is created.
    /// The result value is discarded.
    /// </summary>
    /// <typeparam name="T">The type of value returned by the UniTask.</typeparam>
    public class LazyUniTaskTransition<T> : TransitionBase
    {
        private readonly Func<UniTask<T>> taskFunc;

        /// <summary>
        /// Creates a new lazy UniTask&lt;T&gt; transition.
        /// </summary>
        /// <param name="taskFunc">The function to execute lazily. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if taskFunc is null.</exception>
        public LazyUniTaskTransition(Func<UniTask<T>> taskFunc)
        {
            this.taskFunc = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
        }

        /// <summary>
        /// Executes the function and waits for the resulting UniTask&lt;T&gt; to complete.
        /// The result value is discarded.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A UniTask that completes when the function's task completes.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            // Invoke the function NOW (lazy execution)
            UniTask<T> task = taskFunc();
            
            // Wait for the task to complete and discard the result
            await task.AttachExternalCancellation(cancellationToken);
        }
    }
}