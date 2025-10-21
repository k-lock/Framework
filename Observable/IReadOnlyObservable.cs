using System;
using UnityEngine.Events;

namespace Framework.Observable
{
    /// <summary>
    /// Read-only observable interface that prevents external modification of the value.
    /// Provides subscription and value reading, but not value setting.
    /// </summary>
    /// <typeparam name="T">Type of the observed value.</typeparam>
    public interface IReadOnlyObservable<T> : IDisposable
    {
        /// <summary>
        /// Gets the current value of the observable (read-only).
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Subscribes a listener to this observable.
        /// </summary>
        /// <param name="listener">Callback invoked when the value changes.</param>
        /// <param name="trigger">If true, immediately invokes listener with current value.</param>
        /// <returns>IDisposable to remove the subscription.</returns>
        IDisposable Subscribe(UnityAction<T> listener, bool trigger = false);
    }
}