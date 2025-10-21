using System;
using UnityEngine.Events;

namespace Framework.Observable
{
    /// <summary>
    /// Generic observable interface.
    /// Provides subscription to value changes and supports disposal.
    /// Extends IReadOnlyObservable to allow implicit conversion to read-only access.
    /// </summary>
    /// <typeparam name="T">Type of the observed value.</typeparam>
    public interface IObservable<T> : IReadOnlyObservable<T>
    {
        /// <summary>
        /// Gets or sets the current value of the observable.
        /// Setting the value can trigger notifications to subscribers.
        /// </summary>
        new T Value { get; set; }
    }

    /// <summary>
    /// Non-generic observable interface.
    /// Provides subscription to trigger events and supports disposal.
    /// </summary>
    public interface IObservable : IDisposable
    {
        /// <summary>
        /// Subscribes a listener to this observable.
        /// </summary>
        /// <param name="listener">Callback invoked when the event occurs.</param>
        /// <returns>IDisposable to remove the subscription.</returns>
        IDisposable Subscribe(UnityAction listener);
    }
}