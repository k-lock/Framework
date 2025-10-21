using Framework.Transitions.Base;
using Framework.Transitions.Implementations;
using UnityEngine.UIElements;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for VisualElement related to transitions.
    /// </summary>
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Waits for the visual element's CSS transition to complete (transitionend event).
        /// </summary>
        /// <param name="element">The visual element to observe.</param>
        /// <returns>A transition that completes when the element's transition ends.</returns>
        public static ITransition WaitForComplete(this VisualElement element)
        {
            return new VisualElementTransitionEndTransition(element);
        }

        /// <summary>
        /// Waits for a specific UI Toolkit event to be raised on the element.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to wait for (must derive from EventBase).</typeparam>
        /// <param name="element">The visual element to observe.</param>
        /// <returns>A transition that completes when the event is raised.</returns>
        /// <example>
        /// <code>
        /// // Wait for a click event
        /// await myButton.WaitForEvent&lt;ClickEvent&gt;();
        /// 
        /// // Wait for a mouse enter event
        /// await myElement.WaitForEvent&lt;MouseEnterEvent&gt;();
        /// 
        /// // Combine with other transitions
        /// await Transition.Delay(1f)
        ///     .Or(myButton.WaitForEvent&lt;ClickEvent&gt;());
        /// </code>
        /// </example>
        public static ITransition WaitForEvent<TEvent>(this VisualElement element) 
            where TEvent : EventBase<TEvent>, new()
        {
            return new EventTransition<TEvent>(element);
        }

        /// <summary>
        /// Waits for the visual element's CSS transition to complete (transitionend event).
        /// This is a convenience method equivalent to WaitForEvent&lt;TransitionEndEvent&gt;().
        /// </summary>
        /// <param name="element">The visual element to observe.</param>
        /// <returns>A transition that completes when the element's transition ends.</returns>
        public static ITransition WaitForTransitionEnd(this VisualElement element)
        {
            return element.WaitForEvent<TransitionEndEvent>();
        }
    }
}