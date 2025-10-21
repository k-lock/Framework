using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Framework.Observable;
using Framework.UI.Events;
using Framework.UI.Model;
using Framework.UI.Presenter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.UI.Manager
{
    /// <summary>
    /// Manages registration, navigation, and lifecycle of UI presenters.
    /// Handles showing, hiding, and routing events between presenters.
    /// </summary>
    public class PresenterManager : IDisposable
    {
        /// <summary>
        /// Stores all registered presenters mapped by their type for a quick lookup.
        /// </summary>
        private readonly Dictionary<Type, IPresenter> presenters = new();

        /// <summary>
        /// Reference to the root UI Document's VisualElement.
        /// </summary>
        private readonly VisualElement rootElement;

        /// <summary>
        /// Holds all active subscriptions to presenter events for proper disposal.
        /// </summary>
        private readonly CompositeDisposable subscriptions = new();

        /// <summary>
        /// The currently active presenter being shown in the UI.
        /// </summary>
        private IPresenter currentPresenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterManager" /> class.
        /// </summary>
        /// <param name="uiDocument">The root UIDocument for the UI hierarchy.</param>
        public PresenterManager(UIDocument uiDocument)
        {
            rootElement = uiDocument?.rootVisualElement;
        }

        /// <summary>
        /// Disposes all presenters and their subscriptions.
        /// </summary>
        public void Dispose()
        {
            subscriptions.Dispose();
            if (presenters is null) return;

            foreach (var presenter in presenters.Values) presenter.Dispose();

            presenters?.Clear();
        }

        /// <summary>
        /// Registers a presenter with an optional model instance and subscribes to its events.
        /// </summary>
        /// <typeparam name="TPresenter">The type of the presenter to register.</typeparam>
        /// <typeparam name="TPresenterModel">The type of the presenter model.</typeparam>
        /// <param name="presenterModel">Optional model instance; if null, a default instance is created.</param>
        public void RegisterPresenter<TPresenter, TPresenterModel>(TPresenterModel presenterModel = default)
            where TPresenter : Presenter<IPresenterEvent, TPresenterModel>, new()
            where TPresenterModel : IPresenterModel, new()
        {
            Type presenterType = typeof(TPresenter);
            if (presenters.ContainsKey(presenterType))
            {
                Debug.LogWarning(
                    $"[PresenterManager] RegisterPresenter :: Presenter '{presenterType.Name}' is already registered.");
                return;
            }

            TPresenter presenter = new TPresenter();
            presenterModel ??= new TPresenterModel();

            presenter.Initialize(rootElement, presenterModel);

            presenters[presenterType] = presenter;
            subscriptions.Add(presenter.Events.Subscribe(HandlePresenterEvent));
        }

        /// <summary>
        /// Registers a presenter without a model
        /// </summary>
        /// <typeparam name="TPresenter">The type of the presenter to register.</typeparam>
        public void RegisterPresenter<TPresenter>()
            where TPresenter : Presenter<IPresenterEvent, IPresenterModel>, new()
        {
            var presenterType = typeof(TPresenter);
            if (presenters.ContainsKey(presenterType))
            {
                Debug.LogWarning(
                    $"[PresenterManager] RegisterPresenter :: Presenter '{presenterType.Name}' is already registered.");
                return;
            }

            var presenter = new TPresenter();
            presenter.Initialize(rootElement);

            presenters[presenterType] = presenter;
            subscriptions.Add(presenter.Events.Subscribe(HandlePresenterEvent));
        }

        /// <summary>
        /// Unregisters a presenter, disposing it and removing it from internal tracking.
        /// </summary>
        /// <typeparam name="TPresenter">The type of the presenter to unregister.</typeparam>
        public void UnregisterPresenter<TPresenter>()
        {
            Type presenterType = typeof(TPresenter);
            if (!presenters.TryGetValue(presenterType, out var presenter))
            {
                Debug.LogWarning(
                    $"[PresenterManager] UnregisterPresenter :: Presenter'{presenterType.Name}' is not registered.");
                return;
            }

            presenter.Dispose();
            presenters.Remove(presenterType);

            if (currentPresenter == presenter) currentPresenter = null;
        }

        /// <summary>
        /// Shows the presenter of type T asynchronously.
        /// Hides the current presenter if one is active.
        /// </summary>
        /// <typeparam name="T">The type of presenter to show.</typeparam>
        public async UniTask Show<T>() where T : Presenter<IPresenterEvent, IPresenterModel>
        {
            await Show(typeof(T));
        }

        /// <summary>
        /// Hides a specific presenter by type asynchronously.
        /// </summary>
        /// <param name="presenterType">The type of the presenter to hide.</param>
        public async UniTask Hide(Type presenterType)
        {
            if (!presenters.TryGetValue(presenterType, out var presenter) ||
                (currentPresenter != null && currentPresenter != presenter))
            {
                Debug.LogError($"Presenter {presenterType.Name} not registered!");
                return;
            }

            await presenter.HideAsync();
            currentPresenter = null;
        }

        /// <summary>
        /// Shows a presenter of a specified type with an optional payload.
        /// </summary>
        /// <param name="presenterType">The type of presenter to show.</param>
        /// <param name="payload">Optional data to pass to the presenter.</param>
        private async UniTask Show(Type presenterType, object[] payload = null)
        {
            if (!presenters.TryGetValue(presenterType, out var presenter))
            {
                Debug.LogWarning($"[PresenterManager] Show :: Presenter '{presenterType.Name}' is not registered.");
                return;
            }

            if (currentPresenter == presenter) return;
            if (currentPresenter != null) await HideCurrent();

            currentPresenter = presenter;
            currentPresenter.SetData(payload);

            await currentPresenter.ShowAsync();
        }

        /// <summary>
        /// Hides the currently active presenter asynchronously.
        /// </summary>
        private async UniTask HideCurrent()
        {
            if (currentPresenter != null) await currentPresenter.HideAsync();
            currentPresenter = null;
        }

        /// <summary>
        /// Handles presenter events by routing them to the appropriate target presenter.
        /// </summary>
        /// <param name="presenterEvent">The presenter event containing the target type and optional payload.</param>
        private void HandlePresenterEvent(IPresenterEvent presenterEvent)
        {
            if (presenterEvent?.TargetPresenter == null) return;

            Debug.Log($"[PresenterManager] Navigate to: {presenterEvent.TargetPresenter.Name}");
            Show(presenterEvent.TargetPresenter, presenterEvent.Payload).Forget();
        }
    }
}