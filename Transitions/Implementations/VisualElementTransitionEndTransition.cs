using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;
using UnityEngine.UIElements;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that waits for a VisualElement's transition to end.
    /// </summary>
    public class VisualElementTransitionEndTransition : TransitionBase
    {
        private readonly VisualElement element;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualElementTransitionEndTransition" /> class.
        /// </summary>
        /// <param name="element">The visual element to observe.</param>
        public VisualElementTransitionEndTransition(VisualElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Waits for the visual element's transition to end.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            // If the element is not visible or not in the visual tree, complete immediately
            if (element is not { visible: true } || element.parent == null)
            {
                return;
            }

            UniTaskCompletionSource<bool> tcs = new();

            // Register for cancellation
            CancellationTokenRegistration registration = cancellationToken.Register
            (() =>
                {
                    element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                    tcs.TrySetCanceled(cancellationToken);
                }
            );

            element.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

            try
            {
                await tcs.Task;
            }
            finally
            {
                await registration.DisposeAsync();
            }

            return;

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                element.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                tcs.TrySetResult(true);
            }
        }
    }
}