using UnityEngine.UIElements;

namespace Framework.Observable.Extensions
{
    /// <summary>
    /// Extension methods for VisualElement to convert UI Toolkit events into observables.
    /// </summary>
    public static class ObservableVisualElementExtensions
    {
        /// <summary>
        /// Converts a UI Toolkit event into an observable stream.
        /// Each event fired on the VisualElement will invoke subscribers.
        /// </summary>
        /// <typeparam name="TEvent">Type of UI event (must inherit from EventBase).</typeparam>
        /// <param name="element">VisualElement to observe.</param>
        /// <param name="oneShoot">If true, the observable automatically unsubscribes after the first event.</param>
        /// <returns>An IObservable of the specified event type.</returns>
        public static IObservable<TEvent> OnEventAsObservable<TEvent>(this VisualElement element, bool oneShoot = false)
            where TEvent : EventBase<TEvent>, new()
        {
            Subject<TEvent> subject = new();

            element.RegisterCallback<TEvent>(Callback);

            return subject;

            void Callback(TEvent evt)
            {
                subject.Invoke(evt);
                if (oneShoot)
                {
                    element.UnregisterCallback<TEvent>(Callback);
                }
            }
        }
    }
}