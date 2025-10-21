using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Observable
{
    /// <summary>
    /// Aggregates multiple IDisposable objects into a single disposable.
    /// Calling Dispose() will dispose all contained disposables.
    /// </summary>
    public class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> disposablesInternal = new();
        private readonly object syncLock = new();
        private bool isDisposed;

        /// <summary>
        /// Initializes an empty CompositeDisposable.
        /// </summary>
        public CompositeDisposable()
        {
        }

        /// <summary>
        /// Initializes a CompositeDisposable with an initial set of disposables.
        /// </summary>
        /// <param name="disposables">The disposables to add.</param>
        public CompositeDisposable(params IDisposable[] disposables)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(CompositeDisposable));
            }

            if (disposables == null)
            {
                return;
            }

            lock (syncLock)
            {
                foreach (var disposable in disposables)
                {
                    if (disposable != null)
                    {
                        disposablesInternal.Add(disposable);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes all contained disposables and marks this CompositeDisposable as disposed.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            List<IDisposable> toDispose;
            lock (syncLock)
            {
                toDispose = new List<IDisposable>(disposablesInternal);
                disposablesInternal.Clear();
                isDisposed = true;
            }

            foreach (var d in toDispose)
            {
                try
                {
                    d?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Adds a disposable to the collection.
        /// </summary>
        public CompositeDisposable Add(IDisposable d)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(CompositeDisposable));
            }

            if (d == null)
            {
                return this;
            }

            lock (syncLock)
            {
                disposablesInternal.Add(d);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple disposables to the collection.
        /// </summary>
        public CompositeDisposable Add(params IDisposable[] disposables)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(CompositeDisposable));
            }

            if (disposables == null)
            {
                return this;
            }

            lock (syncLock)
            {
                foreach (var disposable in disposables)
                {
                    if (disposable != null)
                    {
                        disposablesInternal.Add(disposable);
                    }
                }
            }

            return this;
        }
    }
}