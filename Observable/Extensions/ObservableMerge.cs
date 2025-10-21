using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Observable.Extensions
{
    /// <summary>
    /// Static helper class providing common Observable operations such as merging multiple streams.
    /// </summary>
    public static class ObservableMerge
    {
        /// <summary>
        /// Merges multiple observables of the same type into a single observable sequence.
        /// All events from each source will be forwarded to subscribers.
        /// </summary>
        /// <typeparam name="T">Type of the event/data.</typeparam>
        /// <param name="observables">Array of observables to merge.</param>
        /// <returns>A merged observable that emits all events from the sources.</returns>
        public static IObservable<T> Merge<T>(params IObservable<T>[] observables)
        {
            if (observables == null || observables.Length == 0)
            {
                return new EmptyObservable<T>();
            }

            return new MergeObservable<T>(observables);
        }

        /// <summary>
        /// Internal class representing a merged observable stream.
        /// </summary>
        private class MergeObservable<T> : IObservable<T>
        {
            private readonly IObservable<T>[] sources;
            private bool isDisposed;

            public MergeObservable(IObservable<T>[] sources)
            {
                this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
            }

            /// <summary>
            /// Not used for merged observables; present for interface compatibility.
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// Subscribes a listener to all source observables.
            /// </summary>
            /// <param name="listener">Listener callback to invoke on event emission.</param>
            /// <param name="trigger">Whether to trigger immediately with the current value (ignored here).</param>
            /// <returns>A composite disposable containing all subscriptions.</returns>
            public IDisposable Subscribe(UnityAction<T> listener, bool trigger = false)
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(nameof(MergeObservable<T>));
                }

                if (listener == null)
                {
                    throw new ArgumentNullException(nameof(listener));
                }

                CompositeDisposable composite = new();

                foreach (var source in sources)
                {
                    if (source == null)
                    {
                        continue;
                    }

                    try
                    {
                        IDisposable sub = source.Subscribe(listener, trigger);
                        composite.Add(sub);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                return composite;
            }

            /// <summary>
            /// Disposes all source observables.
            /// </summary>
            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }

                foreach (var source in sources)
                {
                    try
                    {
                        source?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Represents an empty observable that never emits events.
        /// </summary>
        private class EmptyObservable<T> : IObservable<T>
        {
            private bool isDisposed;

            public T Value { get; set; }

            public IDisposable Subscribe(UnityAction<T> listener, bool trigger = false)
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException(nameof(EmptyObservable<T>));
                }

                return listener == null
                    ? throw new ArgumentNullException(nameof(listener))
                    : Disposables.Empty;
            }

            public void Dispose()
            {
                isDisposed = true;
            }
        }

        /// <summary>
        /// Internal static class holding a reusable empty disposable.
        /// </summary>
        private static class Disposables
        {
            public static readonly IDisposable Empty = new EmptyDisposable();

            private class EmptyDisposable : IDisposable
            {
                public void Dispose()
                {
                    // Nothing to dispose
                }
            }
        }
    }
}