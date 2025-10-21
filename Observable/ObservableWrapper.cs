using System;
using UnityEngine.Events;

namespace Framework.Observable
{
    /// <summary>
    /// A wrapper class that implements IObservable T and properly disposes of both the inner subject and subscription.
    /// </summary>
    /// <typeparam name="T">Type of value emitted.</typeparam>
    public class ObservableWrapper<T> : IObservable<T>
    {
        private readonly Subject<T> innerSubject;
        private readonly IDisposable subscription;
        private bool isDisposed;

        public ObservableWrapper(Subject<T> innerSubject, IDisposable subscription)
        {
            this.innerSubject = innerSubject ?? throw new ArgumentNullException(nameof(innerSubject));
            this.subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        }

        public T Value { get; set; }

        public IDisposable Subscribe(UnityAction<T> listener, bool trigger = false)
        {
            return isDisposed
                ? throw new ObjectDisposedException(nameof(ObservableWrapper<T>))
                : innerSubject.Subscribe(listener, trigger);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            subscription.Dispose();
            innerSubject.Dispose();
            isDisposed = true;
        }
    }
}