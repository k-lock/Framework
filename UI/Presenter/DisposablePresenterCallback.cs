using System;
using UnityEngine.UIElements;

namespace Framework.UI.Presenter
{
    /// <summary>
    /// Utility class that wraps a UI callback registration and allows automatic unregistration.
    /// Used internally by presenters to manage event lifetimes.
    /// </summary>
    /// <typeparam name="T">The type of UIElements event.</typeparam>
    internal class DisposablePresenterCallback<T> : IDisposable where T : EventBase<T>, new()
    {
        private readonly EventCallback<T> callback;
        private readonly VisualElement element;

        /// <summary>
        /// Creates a disposable wrapper for a callback registered on a VisualElement.
        /// </summary>
        /// <param name="element">The UI element the callback is registered on.</param>
        /// <param name="callback">The callback delegate.</param>
        public DisposablePresenterCallback(VisualElement element, EventCallback<T> callback)
        {
            this.element = element;
            this.callback = callback;
        }

        /// <summary>
        /// Unregisters the callback from the VisualElement when disposed.
        /// </summary>
        public void Dispose()
        {
            if (element is { panel: not null })
            {
                element.UnregisterCallback(callback);
            }
        }
    }
}