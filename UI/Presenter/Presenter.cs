using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Observable;
using Framework.UI.Model;
using UnityEngine.UIElements;

namespace Framework.UI.Presenter
{
    /// <summary>
    /// Base class for all UI presenters.
    /// Provides lifecycle management, callback registration, and observable events for UI elements.
    /// </summary>
    /// <typeparam name="TPresenterEvent">Type of events that the presenter publishes.</typeparam>
    /// <typeparam name="TPresenterModel">Type of the presenter model.</typeparam>
    public abstract class Presenter<TPresenterEvent, TPresenterModel> : IPresenter, IPresenterService<TPresenterModel>
        where TPresenterEvent : class
        where TPresenterModel : IPresenterModel
    {
        /// <summary>
        /// Holds all disposable resources, including event callbacks, for automatic cleanup.
        /// </summary>
        protected readonly CompositeDisposable Disposables = new();

        /// <summary>
        /// Subject that publishes events of type <typeparamref name="TPresenterEvent" /> to subscribers.
        /// </summary>
        private readonly Subject<TPresenterEvent> events = new();

        /// <summary>
        /// Cancellation token source for managing async operations lifecycle.
        /// Automatically canceled when the presenter is disposed of.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Cancellation token that is canceled when the presenter is disposed of.
        /// Use this token for async operations that should be canceled at the presenter disposal.
        /// </summary>
        public CancellationToken CancellationToken => cancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        /// Root container element for this presenter.
        /// </summary>
        public VisualElement PresenterRoot {get; protected set;}

        /// <summary>
        /// The root element of the UI hierarchy this presenter belongs to.
        /// </summary>
        protected VisualElement RootElement;

        /// <summary>
        /// The name of the root element inside the UI hierarchy.
        /// Derived classes must be implemented to locate their main container.
        /// </summary>
        protected abstract string RootElementName { get; }

        /// <summary>
        /// The model associated with this presenter, providing data and state.
        /// </summary>
        public TPresenterModel Model { get; protected set; }

        /// <summary>
        /// Observable stream of events emitted by this presenter.
        /// Subscribers can react to actions or navigation requests.
        /// </summary>
        public Observable.IObservable<TPresenterEvent> Events => events;

        /// <summary>
        /// Disposes all subscriptions and resources associated with this presenter.
        /// Cancels all ongoing async operations and cleans up resources.
        /// </summary>
        public virtual void Dispose()
        {
            CancelAndDisposeCancellations();

            events?.Dispose();
            Disposables?.Dispose();
        }

        /// <summary>
        /// Shows the presenter asynchronously with optional transition animations.
        /// </summary>
        public async UniTask ShowAsync()
        {
            OnShowBegin();

            SetPresenterVisibility();

            await WaitForTransitionAsync(GetShowTransitionObservable());

            OnShowComplete();
        }

        /// <summary>
        /// Hides the presenter asynchronously with optional transition animations.
        /// </summary>
        public async UniTask HideAsync()
        {
            OnHideBegin();

            await WaitForTransitionAsync(GetHideTransitionObservable());

            SetPresenterVisibility(false);

            OnHideComplete();
        }

        /// <summary>
        /// Override to handle payload data passed to this presenter.
        /// </summary>
        /// <param name="objects">Array of objects passed from another presenter.</param>
        public virtual void SetData(object[] objects)
        {
            // Optional override in derived classes to handle payload data.
        }

        /// <summary>
        /// Initializes the presenter with the given root element and optional model.
        /// Sets up callbacks and hides the presenter initially.
        /// </summary>
        /// <param name="root">Root VisualElement of the UI document.</param>
        /// <param name="model">Optional presenter model instance.</param>
        public void Initialize(VisualElement root, TPresenterModel model = default)
        {
            // Initialize cancellation token source
            cancellationTokenSource = new CancellationTokenSource();

            Model = model;
            
            // Automatically set presenter reference if model implements IRequiresPresenterService
            if (Model is IRequiresPresenterService<TPresenterModel> modelWithPresenter)
            {
                modelWithPresenter.Presenter = this;
            }
            
            PresenterRoot = root;
            RootElement = root.Q<VisualElement>(RootElementName);

            Setup();
            HideImediately();
        }

        /// <summary>
        /// Abstract method for presenter-specific setup logic.
        /// </summary>
        protected abstract void Setup();

        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <param name="e">The event instance.</param>
        public void Publish(TPresenterEvent e)
        {
            events?.Invoke(e);
        }

        /// <summary>
        /// Registers a callback on a VisualElement and ensures automatic disposal.
        /// </summary>
        /// <typeparam name="T">Type of the UIElements event.</typeparam>
        /// <param name="element">The element to attach the callback to.</param>
        /// <param name="callback">The callback delegate.</param>
        public void RegisterCallback<T>(VisualElement element, EventCallback<T> callback)
            where T : EventBase<T>, new()
        {
            element.RegisterCallback(callback);
            AddDisposable(new DisposablePresenterCallback<T>(element, callback));
        }

        public void AddDisposable(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        /// <summary>
        /// Called at the very beginning of the show sequence, before visibility changes or transitions.
        /// Override to add custom logic at the start of showing the presenter.
        /// </summary>
        protected virtual void OnShowBegin()
        {
        }

        /// <summary>
        /// Called after the show sequence and any transitions have completed.
        /// Override to add custom logic after the presenter is fully visible.
        /// </summary>
        protected virtual void OnShowComplete()
        {
        }

        /// <summary>
        /// Called at the very beginning of the hide sequence, before visibility changes or transitions.
        /// Override to add custom logic at the start of hiding the presenter.
        /// </summary>
        protected virtual void OnHideBegin()
        {
        }

        /// <summary>
        /// Called after the hide sequence and any transitions have completed.
        /// Override to add custom logic after the presenter is fully hidden.
        /// </summary>
        protected virtual void OnHideComplete()
        {
        }

        private void HideImediately()
        {
            OnHideBegin();
            SetPresenterVisibility(false);
            OnHideComplete();
        }

        /// <summary>
        /// Override to provide an observable that completes when the show transition ends.
        /// </summary>
        protected virtual Observable.IObservable<TransitionEndEvent> GetShowTransitionObservable()
        {
            return null;
        }


        /// <summary>
        /// Override to provide an observable that completes when the hide transition ends.
        /// </summary>
        protected virtual Observable.IObservable<TransitionEndEvent> GetHideTransitionObservable()
        {
            return null;
        }

        /// <summary>
        /// Sets presenter visibility directly without invoking transitions.
        /// </summary>
        /// <param name="visible">Whether the presenter should be visible.</param>
        private void SetPresenterVisibility(bool visible = true)
        {
            PresenterRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            PresenterRoot.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Awaits the completion of a UI transition observable.
        /// If the observable is null, it yields for a single frame.
        /// </summary>
        /// <param name="observable">The transition observable to await.</param>
        private static async UniTask WaitForTransitionAsync(Observable.IObservable<TransitionEndEvent> observable)
        {
            if (observable == null)
            {
                await UniTask.Yield();
                return;
            }

            UniTaskCompletionSource taskCompletionSource = new UniTaskCompletionSource();
            IDisposable disposable = observable.Subscribe(_ => taskCompletionSource.TrySetResult());

            await taskCompletionSource.Task;

            disposable.Dispose();
        }
        
        /// <summary>
        /// Cancels and disposes the cancellation token source.
        /// Safely handles cases where the token source is already disposed of.
        /// </summary>
        private void CancelAndDisposeCancellations()
        {
            // Cancel any ongoing async operations
            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Token source was already disposed, ignore
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }

    }
}