using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Transitions.Base;
using UnityEngine.UIElements;

namespace Framework.Transitions.Implementations
{
    /// <summary>
    /// A transition that waits for a specific UI Toolkit event to be raised on a VisualElement.
    /// This enables event-driven transitions in the fluent API.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to wait for (must derive from EventBase).</typeparam>
    public class EventTransition<TEvent> : TransitionBase where TEvent : EventBase<TEvent>, new()
    {
        private readonly VisualElement element;

        /// <summary>
        /// Creates a new transition that waits for the specified event type on the given element.
        /// </summary>
        /// <param name="element">The VisualElement to observe for events.</param>
        public EventTransition(VisualElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Waits for the event to be raised on the element.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that completes when the event is raised or cancellation is requested.</returns>
        public override async UniTask WaitAsync(CancellationToken cancellationToken)
        {
            // Create a completion source to signal when the event occurs
            var completionSource = new UniTaskCompletionSource();

            // Register the event handler
            void OnEvent(TEvent evt)
            {
                completionSource.TrySetResult();
            }

            element.RegisterCallback<TEvent>(OnEvent);

            try
            {
                // Wait for either the event or cancellation
                await completionSource.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                // Always unregister the callback to prevent memory leaks
                element.UnregisterCallback<TEvent>(OnEvent);
            }
        }
    }
}