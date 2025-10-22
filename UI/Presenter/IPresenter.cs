using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.UI.Model;
using UnityEngine.UIElements;

namespace Framework.UI.Presenter
{
    public interface IPresenter : IDisposable
    {
        UniTask ShowAsync();
        UniTask HideAsync();
        void SetData(object[] payload);
    }

    /// <summary>
    /// Provides context and services for presenter components (controllers, services, etc.).
    /// Exposes limited presenter functionality to external components while maintaining encapsulation.
    /// </summary>
    /// <typeparam name="TPresenterModel">Type of the presenter model.</typeparam>
    public interface IPresenterService<out TPresenterModel> where TPresenterModel : IPresenterModel
    {
        /// <summary>
        /// The model associated with this presenter, providing data and state.
        /// </summary>
        TPresenterModel Model { get; }

        /// <summary>
        /// The root visual element of the presenter.
        /// </summary>
        VisualElement PresenterRoot { get; }

        /// <summary>
        /// Indicates whether the presenter is currently visible or hidden.
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// Cancellation token that is canceled when the presenter is disposed of.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Registers a UI event callback with automatic cleanup on presenter disposal.
        /// </summary>
        void RegisterCallback<T>(VisualElement element, EventCallback<T> callback)
            where T : EventBase<T>, new();

        /// <summary>
        /// Adds a disposable to be cleaned up when the presenter is disposed of.
        /// </summary>
        void AddDisposable(IDisposable disposable);
    }
}