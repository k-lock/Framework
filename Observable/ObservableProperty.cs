using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Observable
{
    /// <summary>
    /// Observable property that notifies subscribers whenever its value changes. Implements IObservable T.
    /// Can be cast to IReadOnlyObservable T to prevent external modification.
    /// </summary>
    /// <typeparam name="T">Type of the value being observed.</typeparam>
    public class ObservableProperty<T> : IObservable<T>, IReadOnlyObservable<T>
    {
        private readonly IEqualityComparer<T> comparer;
        private bool isDisposed;

        // Internal storage of the property value
        private T valueInternal;

        /// <summary>
        /// Creates a new ObservableProperty with an optional initial value.
        /// </summary>
        /// <param name="value">Initial value of the property.</param>
        /// <param name="comparer">Optional custom equality comparer for value comparison.</param>
        public ObservableProperty(T value = default, IEqualityComparer<T> comparer = null)
        {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            valueInternal = value;
        }

        /// <summary>
        /// Gets or sets the value of this property.
        /// Setting the value triggers notifications to subscribers.
        /// </summary>
        public T Value
        {
            get => valueInternal;
            set => Set(value);
        }

        /// <summary>
        /// Subscribes a listener to this property.
        /// </summary>
        /// <param name="listener">Callback invoked when value changes.</param>
        /// <param name="trigger">If true, immediately invokes the listener with the current value.</param>
        /// <returns>IDisposable to remove the subscription.</returns>
        public IDisposable Subscribe(UnityAction<T> listener, bool trigger = false)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(ObservableProperty<T>));
            }

            OnValueChanged += listener ?? throw new ArgumentNullException(nameof(listener));

            if (!trigger)
            {
                return new Subscription(new WeakReference(this), () => OnValueChanged -= listener);
            }

            try
            {
                listener(valueInternal);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return new Subscription(new WeakReference(this), () => OnValueChanged -= listener);
        }

        /// <summary>
        /// Disposes the observable and removes all subscribers.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            OnValueChanged = null;
            isDisposed = true;
        }

        // Event invoked whenever the property value changes
        private event UnityAction<T> OnValueChanged = delegate { };

        /// <summary>
        /// Sets the value and optionally notifies subscribers if it changes.
        /// </summary>
        /// <param name="newValue">New value to set.</param>
        /// <param name="withNotification">Whether to notify subscribers of the change.</param>
        /// <returns>True if the value was changed, false if unchanged.</returns>
        public bool Set(T newValue, bool withNotification = true)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(ObservableProperty<T>));
            }

            if (comparer.Equals(valueInternal, newValue))
            {
                return false;
            }

            valueInternal = newValue;

            if (withNotification)
            {
                NotifySubscribers(valueInternal);
            }

            return true;
        }

        /// <summary>
        /// Safely notifies all subscribers with exception handling.
        /// </summary>
        /// <param name="value">The value to pass to subscribers.</param>
        private void NotifySubscribers(T value)
        {
            var handlers = OnValueChanged;
            if (handlers == null)
            {
                return;
            }

            foreach (var handler in handlers.GetInvocationList())
            {
                try
                {
                    ((UnityAction<T>)handler)(value);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}