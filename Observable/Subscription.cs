using System;
using UnityEngine;

namespace Framework.Observable
{
    /// <summary>
    /// Represents a disposable subscription to an observable.
    /// Ensures safe unsubscription when disposed or owner is gone.
    /// </summary>
    public class Subscription : IDisposable
    {
        private readonly WeakReference owner;
        private bool isDisposed;
        private Action unsubscribe;

        /// <summary>
        /// Creates a new subscription tied to an owner object.
        /// </summary>
        /// <param name="owner">The object owning this subscription (weak reference).</param>
        /// <param name="unsubscribe">Action to execute when disposed.</param>
        public Subscription(WeakReference owner, Action unsubscribe)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.unsubscribe = unsubscribe;
        }

        /// <summary>
        /// Disposes the subscription, invoking the unsubscribe action if the owner is alive.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure cleanup if Dispose is not called.
        /// </summary>
        ~Subscription()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internally dispose implementation that can be called from finalizer.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Only execute unsubscribing if the owner is still alive
                if (owner.IsAlive && owner.Target?.Equals(null) != true)
                {
                    try
                    {
                        unsubscribe?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            unsubscribe = null;
            isDisposed = true;
        }
    }
}