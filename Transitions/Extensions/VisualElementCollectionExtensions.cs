using System.Collections.Generic;
using System.Linq;
using Framework.Transitions.Base;
using UnityEngine.UIElements;

namespace Framework.Transitions.Extensions
{
    /// <summary>
    /// Extension methods for collections of VisualElements related to transitions.
    /// </summary>
    public static class VisualElementCollectionExtensions
    {
        /// <summary>
        /// Waits for all CSS transitions to complete on all elements in the collection.
        /// This is useful for coordinating animations across multiple UI elements.
        /// </summary>
        /// <param name="elements">The collection of visual elements to observe.</param>
        /// <returns>A transition that completes when all elements' transitions have ended.</returns>
        /// <example>
        /// <code>
        /// // Wait for all menu items to finish animating
        /// List&lt;VisualElement&gt; menuItems = GetMenuItems();
        /// await menuItems.WaitForAllTransitionsEnd();
        /// 
        /// // Combine with other transitions
        /// await Transition.Delay(0.5f)
        ///     .Then(menuItems.WaitForAllTransitionsEnd());
        /// </code>
        /// </example>
        public static ITransition WaitForAllTransitionsEnd(this IEnumerable<VisualElement> elements)
        {
            // Convert each element to a transition and wait for all
            ITransition[] transitions = elements
                .Select(e => e.WaitForTransitionEnd())
                .ToArray();
            
            return Transition.WhenAll(transitions);
        }

        /// <summary>
        /// Waits for a specific event to be raised on all elements in the collection.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to wait for (must derive from EventBase).</typeparam>
        /// <param name="elements">The collection of visual elements to observe.</param>
        /// <returns>A transition that completes when the event has been raised on all elements.</returns>
        /// <example>
        /// <code>
        /// // Wait for all buttons to be clicked
        /// List&lt;Button&gt; buttons = GetButtons();
        /// await buttons.WaitForAllEvents&lt;ClickEvent&gt;();
        /// 
        /// // Wait for all elements to receive mouse enter
        /// await elements.WaitForAllEvents&lt;MouseEnterEvent&gt;();
        /// </code>
        /// </example>
        public static ITransition WaitForAllEvents<TEvent>(this IEnumerable<VisualElement> elements)
            where TEvent : EventBase<TEvent>, new()
        {
            // Convert each element to an event transition and wait for all
            ITransition[] transitions = elements
                .Select(e => e.WaitForEvent<TEvent>())
                .ToArray();
            
            return Transition.WhenAll(transitions);
        }

        /// <summary>
        /// Waits for any CSS transition to complete on any element in the collection.
        /// Completes as soon as the first element's transition ends.
        /// </summary>
        /// <param name="elements">The collection of visual elements to observe.</param>
        /// <returns>A transition that completes when any element's transition has ended.</returns>
        /// <example>
        /// <code>
        /// // Wait for any menu item to finish animating
        /// List&lt;VisualElement&gt; menuItems = GetMenuItems();
        /// await menuItems.WaitForAnyTransitionEnd();
        /// </code>
        /// </example>
        public static ITransition WaitForAnyTransitionEnd(this IEnumerable<VisualElement> elements)
        {
            // Convert each element to a transition and wait for any
            ITransition[] transitions = elements
                .Select(e => e.WaitForTransitionEnd())
                .ToArray();
            
            return Transition.WhenAny(transitions);
        }

        /// <summary>
        /// Waits for a specific event to be raised on any element in the collection.
        /// Completes as soon as the event is raised on the first element.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to wait for (must derive from EventBase).</typeparam>
        /// <param name="elements">The collection of visual elements to observe.</param>
        /// <returns>A transition that completes when the event has been raised on any element.</returns>
        /// <example>
        /// <code>
        /// // Wait for any button to be clicked
        /// List&lt;Button&gt; buttons = GetButtons();
        /// await buttons.WaitForAnyEvent&lt;ClickEvent&gt;();
        /// 
        /// // Race condition: which element gets hovered first?
        /// await elements.WaitForAnyEvent&lt;MouseEnterEvent&gt;();
        /// </code>
        /// </example>
        public static ITransition WaitForAnyEvent<TEvent>(this IEnumerable<VisualElement> elements)
            where TEvent : EventBase<TEvent>, new()
        {
            // Convert each element to an event transition and wait for any
            ITransition[] transitions = elements
                .Select(e => e.WaitForEvent<TEvent>())
                .ToArray();
            
            return Transition.WhenAny(transitions);
        }
    }
}