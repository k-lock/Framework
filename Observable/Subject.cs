using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Observable
{
    /// <summary>
    /// Simple non-generic Subject that can notify subscribers when invoked.
    /// </summary>
    public class Subject : IObservable
    {
        private bool isDisposed;

        /// <summary>
        /// Subscribes a listener to this subject.
        /// </summary>
        /// <param name="listener">Callback to invoke when a subject is triggered.</param>
        /// <returns>IDisposable to unsubscribe.</returns>
        public IDisposable Subscribe(UnityAction listener)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject));
            }

            OnInvoked += listener ?? throw new ArgumentNullException(nameof(listener));
            return new Subscription(new WeakReference(this), () => OnInvoked -= listener);
        }

        /// <summary>
        /// Disposes this subject and clears all listeners.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            OnInvoked = null;
            isDisposed = true;
        }

        /// <summary>
        /// Event invoked when the subject is triggered. All subscribed listeners will be notified.
        /// </summary>
        private event UnityAction OnInvoked = delegate { };

        /// <summary>
        /// Invokes the subject, notifying all subscribers.
        /// </summary>
        public void Invoke()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject));
            }

            var handlers = OnInvoked;
            if (handlers == null)
            {
                return;
            }

            foreach (var handler in handlers.GetInvocationList())
            {
                try
                {
                    ((UnityAction)handler)();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Converts this non-generic subject to a generic observable that emits a value.
        /// </summary>
        /// <typeparam name="T">Type of value to emit.</typeparam>
        /// <param name="getParameters">Function to retrieve the emitted value.</param>
        /// <returns>Generic observable subject that properly disposes both the inner subject and subscription.</returns>
        public IObservable<T> WithParameters<T>(Func<T> getParameters)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject));
            }

            if (getParameters == null)
            {
                throw new ArgumentNullException(nameof(getParameters));
            }

            var innerSubject = new Subject<T>();
            var subscription = Subscribe(() =>
            {
                try
                {
                    innerSubject.Invoke(getParameters());
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });

            // Return a wrapper that will dispose of both the inner subject and subscription
            return new ObservableWrapper<T>(innerSubject, subscription);
        }
    }

    /// <summary>
    /// Generic Subject that can notify subscribers with a value.
    /// Can be cast to IReadOnlyObservable T to prevent external modification.
    /// </summary>
    /// <typeparam name="T">Type of value emitted.</typeparam>
    public class Subject<T> : IObservable<T>
    {
        private bool isDisposed;

        /// <summary>
        /// Current value of the subject (for convenience).
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Subscribes a listener to this subject.
        /// </summary>
        /// <param name="listener">Callback invoked when subject is triggered.</param>
        /// <param name="trigger">If true, immediately invokes listener with current value.</param>
        /// <returns>IDisposable to unsubscribe.</returns>
        public IDisposable Subscribe(UnityAction<T> listener, bool trigger = false)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject<T>));
            }

            OnValueChanged += listener ?? throw new ArgumentNullException(nameof(listener));

            if (!trigger)
            {
                return new Subscription(new WeakReference(this), () => OnValueChanged -= listener);
            }

            try
            {
                listener(Value); // Use the current value instead of default
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return new Subscription(new WeakReference(this), () => OnValueChanged -= listener);
        }

        /// <summary>
        /// Disposes this subject and clears all listeners.
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

        private event UnityAction<T> OnValueChanged = delegate { };

        /// <summary>
        /// Invokes the subject, notifying all subscribers with a value.
        /// </summary>
        /// <param name="value">Value to emit.</param>
        public void Invoke(T value)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(Subject<T>));
            }

            Value = value; // Update the current value

            UnityAction<T> handlers = OnValueChanged;
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